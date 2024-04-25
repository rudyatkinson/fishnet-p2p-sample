using System;
using FishNet.Managing;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.EOSLobby.Repository;
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
        private readonly EOSLobbyRepository _eosLobbyRepository;
        
        private Coroutine _searchLobbiesCoroutine;

        public LobbyController(LobbyView lobbyView,
            LobbyRepository lobbyRepository,
            NetworkManager networkManager,
            FishyEOS fishyEos,
            EOSLobbyService eosLobbyService,
            EOSLobbyRepository eosLobbyRepository)
        {
            _lobbyView = lobbyView;
            _lobbyRepository = lobbyRepository;
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _eosLobbyService = eosLobbyService;
            _eosLobbyRepository = eosLobbyRepository;
        }
        
        public void Start()
        {
            _lobbyView.HostButtonClick += OnHostButtonClick;
            _lobbyView.ServerBrowserButtonClick += OnServerBrowserButtonClick;
            _lobbyView.CloseServerBrowserButtonClick += OnCloseServerBrowserButtonClick;
        }

        public void Dispose()
        {
            _lobbyView.HostButtonClick -= OnHostButtonClick;
            _lobbyView.ServerBrowserButtonClick -= OnServerBrowserButtonClick;
            _lobbyView.CloseServerBrowserButtonClick -= OnCloseServerBrowserButtonClick;
        }

        private void OnHostButtonClick()
        {
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
        }
        
        private void OnServerBrowserButtonClick()
        {
            _eosLobbyRepository.IsServerBrowserActive = true;
            
            StartSearchLobbiesCoroutine();
        }
        
        private void OnCloseServerBrowserButtonClick()
        {
            _eosLobbyRepository.IsServerBrowserActive = false;

            _fishyEos.StopCoroutine(_searchLobbiesCoroutine);
        }
        
        private void StartSearchLobbiesCoroutine()
        {
            _searchLobbiesCoroutine = _fishyEos.StartCoroutine(_eosLobbyService.SearchLobbiesCoroutine());
        }
    }
}