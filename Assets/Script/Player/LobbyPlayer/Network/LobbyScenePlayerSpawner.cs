using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
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

        [Inject]
        private void Construct(NetworkManager networkManager,
            LobbyServerController lobbyServerController)
        {
            _lobbyServerController = lobbyServerController;
            
            _networkManager = InstanceFinder.NetworkManager;
            
            _networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScene;
        }
        
        private void OnDestroy()
        {
            _networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScene;
        }
        
        private void OnClientLoadedStartScene(NetworkConnection conn, bool isServer)
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