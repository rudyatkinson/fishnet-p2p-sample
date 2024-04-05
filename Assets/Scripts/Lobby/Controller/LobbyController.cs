using System;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.Lobby.Repository;
using RudyAtkinson.Lobby.View;
using UnityEngine;
using VContainer.Unity;

namespace RudyAtkinson.Lobby.Controller
{
    public class LobbyController: IDisposable, IStartable
    {
        private readonly LobbyView _lobbyView;
        private readonly LobbyRepository _lobbyRepository;
        private readonly NetworkManager _networkManager;
        private readonly FishyEOS _fishyEos;
        
        public LobbyController(LobbyView lobbyView,
            LobbyRepository lobbyRepository,
            NetworkManager networkManager,
            FishyEOS fishyEos)
        {
            _lobbyView = lobbyView;
            _lobbyRepository = lobbyRepository;
            _networkManager = networkManager;
            _fishyEos = fishyEos;
        }
        
        public void Start()
        {
            _lobbyView.HostButtonClick += OnHostButtonClick;
            _lobbyView.JoinButtonClick += OnJoinButtonClick;
            _networkManager.ServerManager.OnServerConnectionState += OnServerStateChanged;
            _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += OnClientStateChanged;
        }

        public void Dispose()
        {
            _lobbyView.HostButtonClick -= OnHostButtonClick;
            _lobbyView.JoinButtonClick -= OnJoinButtonClick;
            _networkManager.ServerManager.OnServerConnectionState -= OnServerStateChanged;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientStateChanged;
        }

        private void OnHostButtonClick()
        {
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
        }

        private void OnJoinButtonClick()
        {
            if (string.IsNullOrEmpty(_lobbyRepository.Address))
            {
                return;
            }

            _fishyEos.RemoteProductUserId = _lobbyRepository.Address;
            
            _networkManager.ClientManager.StartConnection();
        }
        
        private void OnServerStateChanged(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                Debug.Log($"[Server] LocalUserId: {_fishyEos.LocalProductUserId}, This ID is required for other players to join the server.");
            }
        }
    
        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            Debug.Log($"[Server] connection: Id: {connection.ClientId}, Valid: {connection.IsValid}, Status: {args.ConnectionState}, ConnectionId: {args.ConnectionId}");
        }
    
        private void OnClientStateChanged(ClientConnectionStateArgs args)
        {
            Debug.Log($"[Client] Connection state: {args.ConnectionState}");
        }
    }
}