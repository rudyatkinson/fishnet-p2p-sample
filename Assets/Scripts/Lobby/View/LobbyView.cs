using System;
using FishNet.Managing;
using RudyAtkinson.Lobby.Repository;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Lobby.View
{
    public class LobbyView : MonoBehaviour
    {
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
            
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 375, Screen.height / 2 - 75, 750, 150));
            if (GUILayout.Button("HOST", new GUIStyle("button"){fontSize = 42},GUILayout.Width(750), GUILayout.Height(75)))
            {
                HostButtonClick?.Invoke();
            }

            GUILayout.BeginHorizontal();
            
            _lobbyRepository.Address = GUILayout.TextField(_lobbyRepository.Address, new GUIStyle("textfield"){fontSize = 42}, GUILayout.Width(550), GUILayout.Height(75));

            if (GUILayout.Button("JOIN", new GUIStyle("button"){fontSize = 42}, GUILayout.Width(200), GUILayout.Height(75)))
            {
                JoinButtonClick?.Invoke();
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
