using System;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using MessagePipe;
using RudyAtkinson.EOSLobby.Repository;
using RudyAtkinson.EOSLobby.Service;
using RudyAtkinson.Lobby.Message;
using RudyAtkinson.Lobby.Repository;
using RudyAtkinson.Lobby.View;
using UniRx;
using VContainer.Unity;

namespace RudyAtkinson.Lobby.Controller
{
    public class LobbyController: IDisposable, IStartable
    {
        private readonly LobbyView _lobbyView;
        private readonly NetworkManager _networkManager;
        private readonly FishyEOS _fishyEos;
        private readonly EOSLobbyService _eosLobbyService;
        private readonly EOSLobbyRepository _eosLobbyRepository;
        private readonly LobbyRepository _lobbyRepository;
        
        private IPublisher<AllLobbyPlayersReadyCountdownMessage> _allLobbyPlayersReadyCountdownPublisher;
        
        private IDisposable _searchLobbiesDisposable;

        public LobbyController(LobbyView lobbyView,
            NetworkManager networkManager,
            FishyEOS fishyEos,
            EOSLobbyService eosLobbyService,
            EOSLobbyRepository eosLobbyRepository,
            LobbyRepository lobbyRepository,
            IPublisher<AllLobbyPlayersReadyCountdownMessage> allLobbyPlayersReadyCountdownPublisher)
        {
            _lobbyView = lobbyView;
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _eosLobbyService = eosLobbyService;
            _eosLobbyRepository = eosLobbyRepository;
            _lobbyRepository = lobbyRepository;
            _allLobbyPlayersReadyCountdownPublisher = allLobbyPlayersReadyCountdownPublisher;
        }
        
        public void Start()
        {
            _fishyEos.OnClientConnectionState += OnClientConnectionState;
            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            _lobbyView.HostButtonClick += OnHostButtonClick;
            _lobbyView.LeaveButtonClick += OnLeaveButtonClick;
            _lobbyView.ServerBrowserButtonClick += OnServerBrowserButtonClick;
            _lobbyView.CloseServerBrowserButtonClick += OnCloseServerBrowserButtonClick;
        }

        public void Dispose()
        {
            _fishyEos.OnClientConnectionState -= OnClientConnectionState;
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            _lobbyView.HostButtonClick -= OnHostButtonClick;
            _lobbyView.LeaveButtonClick -= OnLeaveButtonClick;
            _lobbyView.ServerBrowserButtonClick -= OnServerBrowserButtonClick;
            _lobbyView.CloseServerBrowserButtonClick -= OnCloseServerBrowserButtonClick;
        }

        private void OnHostButtonClick()
        {
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();
        }

        private void OnLeaveButtonClick()
        {
            if (_networkManager.IsHostStarted)
            {
                _networkManager.ServerManager.StopConnection(true);
            }
            
            _networkManager.ClientManager.StopConnection();
            
            _allLobbyPlayersReadyCountdownPublisher.Publish(new AllLobbyPlayersReadyCountdownMessage(false, 0));
        }
        
        private void OnServerBrowserButtonClick()
        {
            _eosLobbyRepository.IsServerBrowserActive = true;
            
            StartSearchLobbiesCoroutine();
        }
        
        private void OnCloseServerBrowserButtonClick()
        {
            _eosLobbyRepository.IsServerBrowserActive = false;

            _searchLobbiesDisposable?.Dispose();
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            _eosLobbyRepository.IsServerBrowserActive = false;

            _searchLobbiesDisposable?.Dispose();
        }
        
        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                _lobbyRepository.PlayGameCountdownDisposable?.Dispose();
            }
        }
        
        private void StartSearchLobbiesCoroutine()
        {
            _searchLobbiesDisposable = Observable.FromCoroutine(_eosLobbyService.SearchLobbiesCoroutine)
                .Subscribe();
        }
    }
}