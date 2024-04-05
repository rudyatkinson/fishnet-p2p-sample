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
            
            if (GUILayout.Button("HOST", GUILayout.Width(450), GUILayout.Height(150)))
            {
                HostButtonClick?.Invoke();
            }

            GUILayout.BeginHorizontal(GUILayout.Width(450), GUILayout.Height(150));
            
            _lobbyRepository.Address = GUILayout.TextField(_lobbyRepository.Address);

            if (GUILayout.Button("JOIN"))
            {
                JoinButtonClick?.Invoke();
            }
            
            GUILayout.EndHorizontal();
        }
    }
}
