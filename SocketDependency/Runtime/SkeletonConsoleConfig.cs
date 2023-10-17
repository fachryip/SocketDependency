using System.Collections;
using UnityEngine;

namespace NagihSkeleton.Console
{
    [CreateAssetMenu(fileName = "SkeletonConsoleConfig", menuName = "ScriptableObjects/SkeletonConsoleConfig", order = 1)]
    public class SkeletonConsoleConfig : ScriptableObject
    {
        [Header("Dummy")]
        public string WebsocketURLDummy;
        public bool IsLocalWebsocket;
        public bool IsDummyPlayer;

        [Header("Url")]
        public string WebsocketURLBase;
        public string ConsoleBundleIdentifier;
        public string ConsoleMarket;
        public string ConsoleURLHeader;
        public string ConsoleURLHeaderDummy;
        public string WebRTCApiUrl;

        [Header("Type")]
        public string TypeUserConnected;
        public string TypeUserDisconnected;
        public string TypeChangeRoomMaster;
        public string TypePingPong;
        public string TypeFromController;
        public string TypeFromGame;
        public string ErrorGameAlreadyExist;
        public string ErrorGameNotExist;
        public string ErrorIdNotExist;
        public string[] TypeHasDataList;
        public string RequestPingPong;

        [Header("API")]
        public StringPlatform ApiBaseUrlPlatform;
        public string ApiVersion;

        public string WebsocketURLFull => IsLocalWebsocket ? WebsocketURLDummy + WebsocketURLBase :
           "wss://{host}/" + WebsocketURLBase + "{room}";

        public string ApiBaseUrl => ApiBaseUrlPlatform.Value;
        public string WebRTCUrl => ApiBaseUrl + WebRTCApiUrl;

        private void Reset()
        {
            // Dummy
            WebsocketURLDummy = "ws://localhost:3000/";
            IsLocalWebsocket = false;
            IsDummyPlayer = false;

            // Url
            WebsocketURLBase = "";
            ConsoleBundleIdentifier = "";
            ConsoleMarket = "market://details?id=";
            ConsoleURLHeader = "";
            ConsoleURLHeaderDummy = "";
            WebRTCApiUrl = "webrtc";

            // type
            TypeUserConnected = "1001";
            TypeUserDisconnected = "1002";
            TypeChangeRoomMaster = "1003";
            TypePingPong = "1004";
            TypeFromController = "3001";
            TypeFromGame = "4001";
            ErrorGameAlreadyExist = "8001";
            ErrorGameNotExist = "9001";
            ErrorIdNotExist = "9002";
            TypeHasDataList = new string[]
            {
                TypeUserConnected, TypeUserDisconnected,
                TypeChangeRoomMaster, TypeFromController
            };
            RequestPingPong = $"\"type\":\"{TypePingPong}\"";

            // API
            ApiBaseUrlPlatform = new StringPlatform(
                "",
                "",
                "");
            ApiVersion = "api/v1/";
        }

        public string GetApiUrl(string api)
        {
            return ApiBaseUrl + ApiVersion + api;
        }

        
    }
}