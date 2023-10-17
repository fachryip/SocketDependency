using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NagihSkeleton.Console
{
    public class WebsocketConsole : MonoBehaviour, ISocket
    {
        private string _url;
        private bool _isActive;
        private WebSocket _websocket;
        private SkeletonConsoleConfig _config;

        public event Action<string> OnOpen;
        public event Action<string, string> OnMessage;
        public event Action<string, string> OnClose;
        public event Action<string, string> OnError;

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (_websocket != null && _isActive)
            {
                _websocket.DispatchMessageQueue();
            }
#endif
        }

        private async void OnApplicationQuit()
        {
            if (_websocket != null)
            {
                await _websocket.Close();
            }
        }

        public bool IsActive(string id)
        {
            return _isActive;
        }

        public string Name()
        {
            return "Websocket";
        }

        public void SetConfig(SkeletonConsoleConfig config)
        {
            _config = config;
        }

        public async Task StartConnection(string url)
        {
            if (_websocket == null)
            {
                _url = url;
                Debug.Log($"[Websocket] attempt to connect. Url:{_url}");

                _websocket = new WebSocket(_url);
                _websocket.OnOpen += OnWebsocketOpen;
                _websocket.OnMessage += OnWebsocketMessage;
                _websocket.OnClose += OnWebsocketClose;
                _websocket.OnError += OnWebsocketError;

                await _websocket.Connect();
                await new WaitUntil(() => _isActive);
            }
        }

        public bool SendSocketMessage(RequestMessageData request)
        {
            SendSocketMessage(request.ToJson());
            return _isActive;
        }

        public void SendSocketMessage(string message)
        {
            if (_isActive)
            {
                if (!message.Contains(_config.RequestPingPong))
                {
                    Debug.Log($"Send with [{Name()}]. Request:{message}");
                }
                _websocket.SendText(message);
            }
        }

        private void OnWebsocketOpen()
        {
            _isActive = true;
            OnOpen?.Invoke(Name());
            _websocket.Listen();
        }

        private void OnWebsocketMessage(byte[] data)
        {
            OnMessage?.Invoke(Name(), Encoding.UTF8.GetString(data));
        }

        private void OnWebsocketClose(WebSocketCloseCode code)
        {
            _isActive = false;
            OnClose?.Invoke(Name(), code.ToString());
        }

        private void OnWebsocketError(string error)
        {
            _isActive = false;
            OnError?.Invoke(Name(), error);
        }
    }
}