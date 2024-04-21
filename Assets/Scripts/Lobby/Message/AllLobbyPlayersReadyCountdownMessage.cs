namespace RudyAtkinson.Lobby.Message
{
    public class AllLobbyPlayersReadyCountdownMessage
    {
        public bool Enabled;
        public int Countdown;

        public AllLobbyPlayersReadyCountdownMessage(bool enabled, int countdown = 5)
        {
            Enabled = enabled;
            Countdown = countdown;
        }
    }
}