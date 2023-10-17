using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace NagihSkeleton.Console
{
    public class SocketDependency : MonoBehaviour, ISocketDependency
    {
        private SkeletonConsoleConfig _config;
        private SortedDictionary<string, ISocket> _socketDictionary;
        private Dictionary<string, PlayerData> _playerDict;

        // temporary variable
        private string _type;
        private string _pongRequest;
        private ControllerInputMessage _controllerInput;
        private PlayerData _playerData;
        private PlayerData _currentMaster;
        private RequestMessageData _requestFromGame;
        private List<string> _layoutIds;
        // --

        public event Action<string> OnMessage;
        public event Action<string, string> OnClose;
        public event Action<List<string>, string> OnChangedLayout;
        public event Action<PlayerData> OnUserConnected;
        public event Action<PlayerData> OnUserDisconnected;
        public event Action<PlayerData, PlayerData> OnChangeRoomMaster;
        public event Action<ControllerInputMessage> OnIncomingControllerInput;

        // temp
        private int _joinedCount = 0;
        // --

        public ISocketDependency GetDependency()
        {
            return this;
        }

        public void Initialize()
        {
            _socketDictionary = new SortedDictionary<string, ISocket>();
            _playerDict = new Dictionary<string, PlayerData>();
            _layoutIds = new List<string>();

            AddSocket<WebRTCConsole>();
            AddSocket<WebsocketConsole>();

            _controllerInput = new ControllerInputMessage();
            _playerData = new PlayerData(string.Empty, string.Empty);
            _currentMaster = new PlayerData(string.Empty, string.Empty);
        }

        public void SetConfig(SkeletonConsoleConfig config)
        {
            _config = config;
            _requestFromGame = new RequestMessageData(_config.TypeFromGame);
            _pongRequest = new RequestMessageData(_config.TypePingPong).ToJson();

            foreach (var socket in _socketDictionary.Values)
            {
                socket.SetConfig(_config);
            }
        }

        public async Task StartConnection(string url)
        {
            await Task.WhenAll(_socketDictionary.Values.Select(x => x.StartConnection(url)));
        }

        public RequestMessageData GetRequestFromGame()
        {
            _requestFromGame.Clear();
            return _requestFromGame;
        }

        public void SendChangeLayout(string id, string layoutName, int index = -1)
        {
            _requestFromGame.Clear();
            _requestFromGame.AddId(id);
            _requestFromGame.AddDataValue(ConstConsole.PROPERTY_INPUT, ConstConsole.INPUT_CHANGE_LAYOUT);

            var player = GetPlayerData(id);
            if (player != null)
            {
                player.LayoutName = layoutName;
                _requestFromGame.AddDataValue(ConstConsole.PROPERTY_IS_MASTER, Util.TranslateBoolean(player.IsMaster));
            }

            _requestFromGame.AddContentValue(ConstConsole.CONTENT_LAYOUT_NAME, layoutName);
            if (index != -1)
            {
                _requestFromGame.AddContentValue(ConstConsole.CONTENT_ID, index);
            }

            SendRequestFromGame();

            _layoutIds.Clear();
            _layoutIds.Add(id);
            OnChangedLayout?.Invoke(_layoutIds, layoutName);
        }

        public void SendChangeLayout(IEnumerable<string> ids, string layoutName, int index = -1)
        {
            _layoutIds.Clear();
            _requestFromGame.Clear();
            _requestFromGame.AddDataValue(ConstConsole.PROPERTY_INPUT, ConstConsole.INPUT_CHANGE_LAYOUT);
            _requestFromGame.AddContentValue(ConstConsole.CONTENT_LAYOUT_NAME, layoutName);

            if (index != -1)
            {
                _requestFromGame.AddContentValue(ConstConsole.CONTENT_ID, index);
            }

            var isThereMaster = false;
            var masterId = GetMasterPlayerId();
            foreach (var id in ids)
            {
                if (masterId == id)
                {
                    isThereMaster = true;
                    break;
                }
            }

            if (isThereMaster)
            {
                _layoutIds.Add(masterId);
                _requestFromGame.AddId(masterId);
                _requestFromGame.AddDataValue(ConstConsole.PROPERTY_IS_MASTER, 1);
                SendRequestFromGame();

                _requestFromGame.ClearId();
                foreach (var id in ids)
                {
                    if (masterId != id)
                    {
                        _layoutIds.Add(id);
                        _requestFromGame.AddId(id);
                    }
                }
                _requestFromGame.AddDataValue(ConstConsole.PROPERTY_IS_MASTER, 0);
                SendRequestFromGame();
            }
            else
            {
                foreach (var id in ids)
                {
                    _layoutIds.Add(id);
                    _requestFromGame.AddId(id);
                }
                _requestFromGame.AddDataValue(ConstConsole.PROPERTY_IS_MASTER, 0);
                SendRequestFromGame();
            }

            foreach (var id in ids)
            {
                var player = GetPlayerData(id);
                if (player != null)
                {
                    player.LayoutName = layoutName;
                }
            }

            OnChangedLayout?.Invoke(_layoutIds, layoutName);
        }

        public void SendRequestFromGame()
        {
            foreach (var socket in _socketDictionary.Values)
            {
                if (socket.SendSocketMessage(_requestFromGame))
                {
                    break;
                }
            }
        }

        public IEnumerable<PlayerData> GetAllPlayerData()
        {
            return _playerDict.Values;
        }

        public string GetMasterPlayerId()
        {
            return _playerDict.Values.First(x => x.IsMaster).Id;
        }

        public int GetPlayerCount()
        {
            return _playerDict.Count;
        }

        public int GetJoinedPlayerCount()
        {
            _joinedCount = 0;
            foreach (var player in _playerDict.Values)
            {
                if (player.IsJoinGame)
                {
                    _joinedCount++;
                }
            }
            return _joinedCount;
        }

        public PlayerData GetPlayerData(string id)
        {
            return _playerDict.TryGetValue(id);
        }

        private void AddSocket<T>() where T : Component, ISocket
        {
            var socket = gameObject.AddComponent<T>();
            SetListener(socket);
            _socketDictionary[socket.Name()] = socket;
        }

        private void OnSocketOpen(string type)
        {
            Debug.Log($"[{type}] is open.");
        }

        public void OnSocketMessage(string socket, string message)
        {
            if (message.Contains(ConstConsole.PROPERTY_TYPE))
            {
                _type = string.Empty;
                OnMessage?.Invoke(message);
                //Debug.Log($"[{socket}] message:{message}");

                using var sr = new StringReader(message);
                using var reader = new JsonTextReader(sr) { 
                    ArrayPool = JsonArrayPool.Instance,
                    PropertyNameTable = new AutomaticJsonNameTable(50)
                };
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject)
                        break;

                    if (reader.TokenType != JsonToken.PropertyName)
                        continue;

                    if (Convert.ToString(reader.Value) == ConstConsole.PROPERTY_TYPE)
                    {
                        reader.Read();
                        _type = Convert.ToString(reader.Value);
                        break;
                    }
                }

                if (_type == _config.TypePingPong)
                {
                    _socketDictionary[socket].SendSocketMessage(_pongRequest);
                }
                else
                {
                    reader.Read();
                    if (Convert.ToString(reader.Value) == ConstConsole.PROPERTY_DATA)
                    {
                        VerifyMessageType(socket, message, reader);
                    }
                    else
                    {
                        Debug.LogWarning($"[{socket}] no property 'data' contain in json.\nMessage:{message}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[{socket}] incoming message has no property 'type'.\nMessage:{message}");
            }
        }

        private void VerifyMessageType(string socket, string message, JsonReader reader)
        {
            if (_type == _config.TypeFromController)
            {
                //Debug.Log($"[{nameof(SocketDependency)}] VerifyMessageType:{message}");
                _controllerInput.FromJson(reader);
                //if (_controllerInput.input != ConstConsole.INPUT_JOYSTICK)
                //{
                //    Debug.Log($"[{nameof(SocketDependency)}] VerifyMessageType:{message}");
                //}
                OnIncomingControllerInput?.Invoke(_controllerInput);
            }
            else if (_type == _config.TypeUserConnected)
            {
                Debug.Log($"[{socket}] message:{message}");
                _playerData.FromJson(reader);
                _playerDict[_playerData.Id] = new PlayerData(_playerData.Id, _playerData.Name);
                OnUserConnected?.Invoke(_playerData);
                Debug.Log($"[{socket}] OnUserConnected. {_playerData}");
            }
            else if (_type == _config.TypeUserDisconnected)
            {
                Debug.Log($"[{socket}] message:{message}");
                _playerData.FromJson(reader);
                if (_playerDict.ContainsKey(_playerData.Id))
                {
                    var removedPlayer = _playerDict[_playerData.Id];
                    _playerDict.Remove(_playerData.Id);
                    Debug.Log($"[{socket}] OnUserDisconnected. {removedPlayer}");
                    OnUserDisconnected?.Invoke(removedPlayer);
                }
            }
            else if (_type == _config.TypeChangeRoomMaster)
            {
                Debug.Log($"[{socket}] message:{message}");
                _playerData.FromJson(reader);
                PlayerData newMaster = null;
                foreach (var player in _playerDict.Values)
                {
                    player.IsMaster = player.Id == _playerData.Id;
                    if (player.IsMaster)
                    {
                        newMaster = player;
                    }
                }

                if (_currentMaster != null)
                {
                    if (newMaster.IsJoinGame)
                    {
                        _currentMaster.Index = newMaster.Index;
                    }
                    else
                    {
                        newMaster.CharacterIndex = _currentMaster.CharacterIndex;
                    }
                }

                if (newMaster != null)
                {
                    newMaster.Index = 0;

                    Debug.Log($"[{socket}] OnChangedRoomMaster. Prev:[{_currentMaster}] Current:[{newMaster}]");
                    OnChangeRoomMaster?.Invoke(_currentMaster, newMaster);
                    _currentMaster = newMaster;
                }
                else
                {
                    Debug.LogWarning("Id for new room master is not found! Current master is still old master.");
                    OnChangeRoomMaster?.Invoke(null, _currentMaster);
                }
            }
            else
            {
                Debug.LogWarning($"[{socket}] incoming 'type':{_type} is not recognized.\nMessage:{message}");
            }
        }

        private void OnSocketClose(string type, string message)
        {
            Debug.Log($"[{type}] is closed. Message:{message}");
            OnClose?.Invoke(type, message);
        }

        private void OnSocketError(string type, string error)
        {
            Debug.Log($"[{type}] error. Message:{error}");
        }

        private void SetListener(ISocket socket)
        {
            socket.OnOpen += OnSocketOpen;
            socket.OnMessage += OnSocketMessage;
            socket.OnClose += OnSocketClose;
            socket.OnError += OnSocketError;
        }
    }
}