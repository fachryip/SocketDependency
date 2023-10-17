using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NagihSkeleton.Console
{
    public interface ISocketDependency : IDependency<ISocketDependency>
    {
        event Action<string> OnMessage;
        event Action<string, string> OnClose;
        event Action<List<string>, string> OnChangedLayout;
        event Action<PlayerData> OnUserConnected;
        event Action<PlayerData> OnUserDisconnected;
        event Action<PlayerData, PlayerData> OnChangeRoomMaster;
        event Action<ControllerInputMessage> OnIncomingControllerInput;

        void SendChangeLayout(string id, string layoutName, int index = -1);
        void SendChangeLayout(IEnumerable<string> ids, string layoutName, int index = -1);
        void SendRequestFromGame();

        void SetConfig(SkeletonConsoleConfig config);
        Task StartConnection(string url);
        RequestMessageData GetRequestFromGame();

        PlayerData GetPlayerData(string id);
        IEnumerable<PlayerData> GetAllPlayerData();
        int GetPlayerCount();
        int GetJoinedPlayerCount();
        string GetMasterPlayerId();
    }
}