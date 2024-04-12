namespace RudyAtkinson.Lobby.Model
{
    public class AllLobbyPlayersReadyCountdown
    {
        public bool Enabled;
        public int Countdown;

        public AllLobbyPlayersReadyCountdown(bool enabled, int countdown = 5)
        {
            Enabled = enabled;
            Countdown = countdown;
        }
    }
}