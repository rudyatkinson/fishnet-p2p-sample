using TMPro;
using UnityEngine;

namespace Script.Networking.Lobby.View
{
    public class LobbyView: MonoBehaviour
    {
        [SerializeField] private TMP_Text _startTimerText;

        public void RefreshLobbyStartCountdown(int countdown)
        {
            _startTimerText.SetText($"Game is starting in {countdown}");
        }

        public void DisableLobbyStartCountdown()
        {
            _startTimerText.SetText(string.Empty);
        }
    }
}