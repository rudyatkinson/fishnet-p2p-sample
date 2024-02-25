using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using Zenject;

namespace Script.Player.LobbyPlayer.Network
{
    public class LobbyPlayerSpawner: MonoBehaviour
    {
        private NetworkManager _networkManager;
        
        [SerializeField] private NetworkObject _lobbyPlayerPrefab;

        private void Awake()
        {
            _networkManager = InstanceFinder.NetworkManager;
        }

        private void OnEnable()
        {
            _networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScene;
        }

        private void OnDisable()
        {
            _networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScene;
        }

        private void OnClientLoadedStartScene(NetworkConnection conn, bool isServer)
        {
            if (!isServer)
                return;
            
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
        }
    }
}