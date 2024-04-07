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
        }

        public void Dispose()
        {
            _lobbyView.HostButtonClick -= OnHostButtonClick;
            _lobbyView.JoinButtonClick -= OnJoinButtonClick;
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
    }
}