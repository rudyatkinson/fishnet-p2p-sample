using System;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using Script.Installer;
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

        [Server]
        public void Start()
        {
            Inject();
            InstantiatePlayerObjects();
        }

        private void Inject()
        {
            _networkManager = LobbySceneInstaller.ContainerInstance.Resolve<NetworkManager>();
        }

        private void InstantiatePlayerObjects()
        {
            if (_playerPrefab == null)
            {
                Debug.LogWarning($"Lobby player prefab is empty and cannot be spawned for connection.");
                return;
            }

            foreach (var serverManagerClient in _networkManager.ServerManager.Clients)
            {
                var obj = Instantiate(_playerPrefab);
                var spawnTransform = _playerSpawnTransforms[_nextSpawnIndex++];

                obj.transform.position = spawnTransform.position;
            
                _networkManager.ServerManager.Spawn(obj, serverManagerClient.Value, UnityEngine.SceneManagement.SceneManager.GetSceneByName("PlayScene"));
            
                if (_nextSpawnIndex >= _playerSpawnTransforms.Length)
                {
                    _nextSpawnIndex = 0;
                }
            }
        }
    }
}