using System;
using FishNet.Managing;
using RudyAtkinson.Lobby.Repository;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Lobby.View
{
    public class LobbyView : MonoBehaviour
    {
        private const int _width = 750;
        private const int _height = 225;
        
        private NetworkManager _networkManager;
        private LobbyRepository _lobbyRepository;
        
        public event Action HostButtonClick;
        public event Action JoinButtonClick;
        
        [Inject]
        public void Construct(LobbyRepository lobbyRepository,
            NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _lobbyRepository = lobbyRepository;
        }

        private void OnGUI()
        {
            if (_networkManager.IsClientStarted)
            {
                return;
            }
            
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .5f - _height * .5f, _width, _height));
            
            var playerName = GUILayout.TextField(_lobbyRepository.PlayerName, new GUIStyle("textfield"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75))
                .Trim()
                .Replace(" ", string.Empty);

            if (playerName.Length > 18)
            {
                playerName = playerName[..18];
            }

            _lobbyRepository.PlayerName = playerName;

            if (GUILayout.Button("HOST", new GUIStyle("button"){fontSize = 42},GUILayout.Width(750), GUILayout.Height(75)))
            {
                HostButtonClick?.Invoke();
            }

            GUILayout.BeginHorizontal();
            
            _lobbyRepository.Address = GUILayout.TextField(_lobbyRepository.Address, new GUIStyle("textfield"){fontSize = 42, alignment = TextAnchor.MiddleLeft}, GUILayout.Width(550), GUILayout.Height(75));

            if (GUILayout.Button("JOIN", new GUIStyle("button"){fontSize = 42}, GUILayout.Width(200), GUILayout.Height(75)))
            {
                JoinButtonClick?.Invoke();
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
