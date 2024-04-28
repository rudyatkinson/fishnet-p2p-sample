using System;
using FishNet.Transporting;
using RudyAtkinson.Lobby.Message;

namespace RudyAtkinson.Lobby.Repository
{
    public class LobbyRepository
    {
        public LocalConnectionState LocalConnectionState { get; set; }
        public AllLobbyPlayersReadyCountdownMessage AllLobbyPlayersReadyCountdownMessageData { get; set; } = new(false);
        public IDisposable PlayGameCountdownDisposable { get; set; }
    }
}