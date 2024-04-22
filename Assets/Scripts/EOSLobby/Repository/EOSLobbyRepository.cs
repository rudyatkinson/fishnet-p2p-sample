using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

namespace RudyAtkinson.EOSLobby.Repository
{
    public class EOSLobbyRepository
    {
        public Utf8String LobbyId { get; set; }
        public List<LobbyDetails> LobbyDetails { get; set; } = new();
    }
}