using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using Script.Installer;
using Script.Networking.Lobby;
using Script.Player.LobbyPlayer.View;
using UnityEngine;
using Zenject;

namespace Script.Player.LobbyPlayer.Network
{
    public class LobbyScenePlayerSpawner: MonoBehaviour
    {
        private NetworkManager _networkManager;
        private LobbyServerController _lobbyServerController;
        
        [SerializeField] private NetworkObject _lobbyPlayerPrefab;

        [Server]
        public void Start()
        {
            Inject();
            Subscribe();
        }

        private void Inject()
        {
            _lobbyServerController = LobbySceneInstaller.ContainerInstance.Resolve<LobbyServerController>();
            _networkManager = LobbySceneInstaller.ContainerInstance.Resolve<NetworkManager>();
        }

        private void Subscribe()
        {
            _networkManager.SceneManager.OnClientLoadedStartScenes += OnClientConnected;
        }

        private void OnDestroy()
        {
            _networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientConnected;
        }
        
        private void OnClientConnected(NetworkConnection conn, bool isServer)
        {
            if (!isServer)
            {
                return;
            }
            
            if (_lobbyPlayerPrefab == null)
            {
                Debug.LogWarning($"Lobby player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }

            var obj = _networkManager.GetPooledInstantiated(_lobbyPlayerPrefab, true);
            var objTransform = obj.transform;
            
            objTransform.SetParent(transform);
            objTransform.localScale = Vector3.one;
            
            _networkManager.ServerManager.Spawn(obj, conn);

            var lobbyPlayerView = obj.GetComponent<LobbyPlayerView>();
            _lobbyServerController.AddLobbyPlayer(lobbyPlayerView);
        }
    }
}