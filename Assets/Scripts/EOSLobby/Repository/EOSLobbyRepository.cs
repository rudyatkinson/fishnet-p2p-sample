using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

namespace RudyAtkinson.EOSLobby.Repository
{
    public class EOSLobbyRepository
    {
        public Utf8String LobbyId { get; set; }
        public Dictionary<LobbyDetails, LobbyDetailsInfo> LobbyDetails { get; set; } = new();
    }
}