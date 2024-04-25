using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

namespace RudyAtkinson.EOSLobby.Repository
{
    public class EOSLobbyRepository
    {
        public Utf8String LobbyId { get; set; } = new("");
        public Dictionary<LobbyDetails, LobbyDetailsInfo> LobbyDetails { get; set; } = new();
        public bool IsServerBrowserActive { get; set; }
        public bool TriedToJoinLobbyViaServerBrowser { get; set; }
        public Result EOSLoginResult { get; set; } = Result.NoConnection;
        public bool TriedToLoginEOSAtInitialTime { get; set; }
    }
}