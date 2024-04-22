using Epic.OnlineServices;
using FishNet.Transporting;
using RudyAtkinson.Lobby.Message;

namespace RudyAtkinson.Lobby.Repository
{
    public class LobbyRepository
    {
        public string Address { get; set; }
        public LocalConnectionState LocalConnectionState { get; set; }
        public AllLobbyPlayersReadyCountdownMessage AllLobbyPlayersReadyCountdownMessageData { get; set; } = new(false);
        public Result EOSLoginResult { get; set; } = Result.NoConnection;
        public bool TriedToLoginEOSAtInitialTime { get; set; }
        public bool IsServerBrowserActive { get; set; }
    }
}