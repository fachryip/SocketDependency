using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NagihSkeleton.Console
{
    public interface ISocket
    {
        event Action<string> OnOpen;
        event Action<string, string> OnMessage;
        event Action<string, string> OnClose;
        event Action<string, string> OnError;

        bool IsActive(string id);
        string Name();
        bool SendSocketMessage(RequestMessageData request);
        void SendSocketMessage(string message);
        void SetConfig(SkeletonConsoleConfig config);
        Task StartConnection(string url);
    }
}