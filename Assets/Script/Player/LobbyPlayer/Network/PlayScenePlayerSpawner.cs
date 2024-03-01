using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using Zenject;

namespace Script.Player.LobbyPlayer.Network
{
    public class PlayScenePlayerSpawner : MonoBehaviour
    {
        private NetworkManager _networkManager;

        private int _nextSpawnIndex;

        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private Transform[] _playerSpawnTransforms;
        
        [Inject]
        private void Construct(NetworkManager networkManager)
        {
            Debug.Log($"[PlayScenePlayerSpawner] Construct");
            _networkManager = networkManager;
            
            _networkManager.SceneManager.OnLoadEnd += OnLoadEnd;
        }
        
        private void OnDestroy()
        {
            Debug.Log($"[PlayScenePlayerSpawner] OnDestroy");
            _networkManager.SceneManager.OnLoadEnd -= OnLoadEnd;
        }
        
        [Server]
        private void OnLoadEnd(SceneLoadEndEventArgs e)
        {
            Debug.Log($"[PlayScenePlayerSpawner] OnClientLoadedStartScene");

            if (_playerPrefab == null)
            {
                Debug.LogWarning($"Lobby player prefab is empty and cannot be spawned for connection.");
                return;
            }

            foreach (var serverManagerClient in _networkManager.ServerManager.Clients)
            {
                var obj = _networkManager.GetPooledInstantiated(_playerPrefab, true);
                var spawnTransform = _playerSpawnTransforms[_nextSpawnIndex++];

                obj.transform.position = spawnTransform.position;
            
                _networkManager.ServerManager.Spawn(obj, serverManagerClient.Value);
            
                if (_nextSpawnIndex >= _playerSpawnTransforms.Length)
                {
                    _nextSpawnIndex = 0;
                }
            }
        }
    }
}