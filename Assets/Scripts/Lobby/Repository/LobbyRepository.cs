using FishNet.Transporting;
using RudyAtkinson.Lobby.Model;

namespace RudyAtkinson.Lobby.Repository
{
    public class LobbyRepository
    {
        public string Address { get; set; }
        public LocalConnectionState LocalConnectionState { get; set; }
        public AllLobbyPlayersReadyCountdown AllLobbyPlayersReadyCountdownData { get; set; } = new(false);
    }
}