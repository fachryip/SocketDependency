using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NagihSkeleton.Console
{
    public class WebRTCConsole : MonoBehaviour, ISocket
    {
        private const char LIMITER = '|';
        private const string ON_OPEN = "on-open";
        private const string ON_CLOSE = "on-close";
        private const string ON_MESSAGE = "on-message";
        private const string ICE_CANDIDATE = "ice-candidate";
        private const string SEND_ANSWER = "send-answer";

        private List<string> _activeIds;
        private WebViewObject _webview;
        private SkeletonConsoleConfig _config;
        private ISocketDependency _socket;

        // temporary variable
        private int _sentCount = 0;
        private int _idCount = 0;
        private StringBuilder _builder = new StringBuilder();
        // --

        public event Action<string> OnOpen;
        public event Action<string, string> OnMessage;
        public event Action<string, string> OnClose;
        public event Action<string, string> OnError;

        private void Awake()
        {
            _activeIds = new List<string>();
        }

        private void OnDisable()
        {
            if (_socket != null)
            {
                _socket.OnUserDisconnected -= OnWebsocketDisconnect;
                _socket.OnIncomingControllerInput -= IncomingWebsocketMessage;
            }
        }

        public bool IsActive(string id)
        {
            return _activeIds.Contains(id);
        }

        public string Name()
        {
            return "WebRTC";
        }

        public void SetConfig(SkeletonConsoleConfig config)
        {
            _config = config;

            if (_socket == null)
            {
                _socket = DependencyCollections.Get<ISocketDependency>();
                _socket.OnUserDisconnected += OnWebsocketDisconnect;
                _socket.OnIncomingControllerInput += IncomingWebsocketMessage;
            }
        }

        public Task StartConnection(string url)
        {
            if (_webview == null)
            {
                _webview = gameObject.AddComponent<WebViewObject>();
                _webview.Init(cb: OnWebviewMessage, ld: OnWebviewLoaded, started: OnWebviewStarted, err: OnWebviewError);
                _webview.SetMargins(0, 0, 0, Screen.height * 10);
                _webview.SetVisibility(false);

                url = _config.WebRTCUrl;
                Debug.Log($"[{Name()}] Start load URL:{url}");
                _webview.LoadURL(url.Replace(" ", "%20"));
            }
            
            return Task.CompletedTask;
        }

        public bool SendSocketMessage(RequestMessageData request)
        {
            _sentCount = 0;
            _idCount = 0;
            foreach (var id in request.Ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    _idCount++;
                    if (IsActive(id))
                    {
                        _sentCount++;
                        SendSocketMessage($"SendMessage('{id}', {request.ToJson()})");
                    }
                }
            }

            return _sentCount == _idCount;
        }

        public void SendSocketMessage(string message)
        {
            if (!message.Contains(_config.RequestPingPong))
            {
                Debug.Log($"Send with [{Name()}]. Request:{message}");
            }
            _webview?.EvaluateJS(message);
        }

        private int _idx;
        private string _type;
        private void OnWebviewMessage(string message)
        {
            //Debug.Log($"[{Name()}] OnWebviewMessage: {message}");

            var span = message.AsSpan();
            _idx = span.IndexOf(LIMITER);
            if (_idx >= 0)
            {
                _type = span.Slice(0, _idx).ToString();
                if (_idx < span.Length)
                {
                    span = span.Slice(_idx + 1);
                }

                switch (_type)
                {
                    case ON_OPEN:
                        OnWebRTCOpen(span.ToString()); break;
                    case ON_CLOSE:
                        OnWebRTCClose(span.ToString()); break;
                    case ON_MESSAGE:
                        _idx = span.IndexOf(LIMITER);
                        OnWebRTCMessage(span.Slice(0, _idx).ToString(), span.Slice(_idx + 1).ToString()); break;
                    case ICE_CANDIDATE:
                        _idx = span.IndexOf(LIMITER);
                        IceCandidate(span.Slice(0, _idx).ToString(), span.Slice(_idx + 1).ToString()); break;
                    case SEND_ANSWER:
                        _idx = span.IndexOf(LIMITER);
                        SendAnswer(span.Slice(0, _idx).ToString(), span.Slice(_idx + 1).ToString()); break;
                    default:
                        Debug.Log($"[{Name()}] Message: {message}");
                        break;
                }
            }
        }

        private void OnWebviewLoaded(string message)
        {
            Debug.Log($"[Webview] On Loaded. Message: {message}");
        }

        private void OnWebviewStarted(string message)
        {
            Debug.Log($"[Webview] On Started. Message: {message}");
        }

        private void OnWebviewError(string message)
        {
            OnError?.Invoke(Name(), message);
            Debug.Log($"[Webview] On Error. Message: {message}");
        }

        private void OnWebRTCOpen(string id)
        {
            Debug.Log($"[{Name()}] On Open. Id {id}");
            if (!_activeIds.Contains(id))
            {
                _activeIds.Add(id);
            }
            OnOpen?.Invoke(Name() + " " + id);
        }

        private void OnWebRTCClose(string id)
        {
            _activeIds.Remove(id);
            OnClose?.Invoke(Name() + " " + id, "WebRTC Closed.");
        }

        private void OnWebRTCMessage(string id, string message)
        {
            _builder.Clear();
            _builder.Append(message);
            _builder.Insert(message.Length - 2, $",\"{ControllerInputMessage.ID}\":\"{id}\"");
            //Debug.Log($"[WebRTC] message:{builder}");

            OnMessage?.Invoke(Name(), _builder.ToString());
        }

        private void OnWebsocketDisconnect(PlayerData player)
        {
            var func = $"Close('{player.Id}')";
            Debug.Log(func);
            _webview?.EvaluateJS(func);
        }

        private void IncomingWebsocketMessage(ControllerInputMessage message)
        {
            switch (message.input)
            {
                case ConstConsole.INPUT_WEBRTC_OFFER:
                    var func = $"CreateAnswer('{message.id}', {message.content[ConstConsole.CONTENT_OFFER]})";
                    Debug.Log(func);
                    _webview?.EvaluateJS(func);
                    break;

                case ConstConsole.INPUT_WEBRTC_CANDIDATE:
                    func = $"AddIceCandidate('{message.id}', {message.content[ConstConsole.CONTENT_CANDIDATE]})";
                    Debug.Log(func);
                    _webview?.EvaluateJS(func);
                    break;

                case ConstConsole.INPUT_PING:
                    var request = _socket.GetRequestFromGame();
                    request.AddId(message.id);
                    request.AddDataValue(ConstConsole.PROPERTY_INPUT, ConstConsole.INPUT_PONG);
                    _socket.SendRequestFromGame();
                    break;
            }
        }

        private void IceCandidate(string id, string message)
        {
            Debug.Log($"[WebRTC] Ice Candidate id:{id} message:{message}");

            var request = _socket.GetRequestFromGame();
            request.AddId(id);
            request.AddContentValue(ConstConsole.CONTENT_CANDIDATE, message);
            request.AddDataValue(ConstConsole.PROPERTY_INPUT, ConstConsole.INPUT_WEBRTC_CANDIDATE);
            _socket.SendRequestFromGame();
        }

        private void SendAnswer(string id, string message)
        {
            Debug.Log($"[WebRTC] Send Answer id:{id} message:{message}");

            var request = _socket.GetRequestFromGame();
            request.AddId(id);
            request.AddContentValue(ConstConsole.CONTENT_ANSWER, message);
            request.AddDataValue(ConstConsole.PROPERTY_INPUT, ConstConsole.INPUT_WEBRTC_ANSWER);
            _socket.SendRequestFromGame();
        }
    }
}