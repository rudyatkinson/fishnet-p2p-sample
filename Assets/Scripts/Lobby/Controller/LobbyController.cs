using System;
using FishNet.Managing;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.EOSLobby.Service;
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
        private readonly EOSLobbyService _eosLobbyService;
        
        private Coroutine _searchLobbiesCoroutine;

        public LobbyController(LobbyView lobbyView,
            LobbyRepository lobbyRepository,
            NetworkManager networkManager,
            FishyEOS fishyEos,
            EOSLobbyService eosLobbyService)
        {
            _lobbyView = lobbyView;
            _lobbyRepository = lobbyRepository;
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _eosLobbyService = eosLobbyService;
        }
        
        public void Start()
        {
            _lobbyView.HostButtonClick += OnHostButtonClick;
            _lobbyView.JoinButtonClick += OnJoinButtonClick;
            _lobbyView.ServerBrowserButtonClick += OnServerBrowserButtonClick;
            _lobbyView.CloseServerBrowserButtonClick += OnCloseServerBrowserButtonClick;
        }

        public void Dispose()
        {
            _lobbyView.HostButtonClick -= OnHostButtonClick;
            _lobbyView.JoinButtonClick -= OnJoinButtonClick;
            _lobbyView.ServerBrowserButtonClick -= OnServerBrowserButtonClick;
            _lobbyView.CloseServerBrowserButtonClick -= OnCloseServerBrowserButtonClick;
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
        
        private void OnServerBrowserButtonClick()
        {
            _lobbyRepository.IsServerBrowserActive = true;
            
            StartSearchLobbiesCoroutine();
        }
        
        private void OnCloseServerBrowserButtonClick()
        {
            _lobbyRepository.IsServerBrowserActive = false;

            _fishyEos.StopCoroutine(_searchLobbiesCoroutine);
        }
        
        private void StartSearchLobbiesCoroutine()
        {
            _searchLobbiesCoroutine = _fishyEos.StartCoroutine(_eosLobbyService.SearchLobbiesCoroutine());
        }
    }
}