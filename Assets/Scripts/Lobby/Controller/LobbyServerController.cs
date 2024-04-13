using System;
using System.Collections;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using MessagePipe;
using RudyAtkinson.Lobby.Model;
using RudyAtkinson.LobbyPlayer.Model;
using RudyAtkinson.LobbyPlayer.View;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Lobby.Controller
{
    public class LobbyServerController: NetworkBehaviour
    {
        private NetworkManager _networkManager;
        private FishyEOS _fishyEos;
        private LobbyPlayerViewFactory _lobbyPlayerViewFactory;

        private IPublisher<AllLobbyPlayersReadyCountdown> _allLobbyPlayersReadyCountdownPublisher;
        
        private ISubscriber<LobbyPlayerReady> _lobbyPlayerReadySubscriber;

        private IDisposable _messageSubscriptionCombinedDisposable;

        private IEnumerator _playGameCountdownCoroutine;

        [SerializeField] private NetworkBehaviour _lobbyPlayerParent;
        
        [Inject]
        private void Construct(NetworkManager networkManager,
            FishyEOS fishyEos,
            LobbyPlayerViewFactory lobbyPlayerViewFactory,
            ISubscriber<LobbyPlayerReady> lobbyPlayerReadySubscriber,
            IPublisher<AllLobbyPlayersReadyCountdown> allLobbyPlayersReadyCountdownPublisher)
        {
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _lobbyPlayerViewFactory = lobbyPlayerViewFactory;

            _allLobbyPlayersReadyCountdownPublisher = allLobbyPlayersReadyCountdownPublisher;
            _lobbyPlayerReadySubscriber = lobbyPlayerReadySubscriber;
        }

        private void Awake()
        {
            _playGameCountdownCoroutine = PlayGameCountdown();
        }

        public void OnEnable()
        {
            _networkManager.ServerManager.OnServerConnectionState += OnServerStateChanged;
            _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += OnClientStateChanged;

            var lobbyPlayerReadyDisposable = _lobbyPlayerReadySubscriber.Subscribe(_ => ServerCheckAllPlayersReady());
            
            _messageSubscriptionCombinedDisposable = DisposableBag.Create(lobbyPlayerReadyDisposable);
        }
        
        public void OnDisable()
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnServerStateChanged;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientStateChanged;
            
            _messageSubscriptionCombinedDisposable?.Dispose();
        }

        #region Client Callbacks
        
        private void OnClientStateChanged(ClientConnectionStateArgs args)
        {
            Debug.Log($"[Client] Connection state: {args.ConnectionState}");
        }
        
        #endregion
        
        #region Server Callbacks
        
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
            
            if (args.ConnectionState == RemoteConnectionState.Started)
            {
                var lobbyPlayer = _lobbyPlayerViewFactory.Create();
                
                lobbyPlayer.NetworkObject.SetParent(_lobbyPlayerParent);
                lobbyPlayer.transform.localScale = Vector3.one;
                
                _networkManager.ServerManager.Spawn(lobbyPlayer.gameObject, connection);
            }
            else
            {
                var lobbyPlayer = connection.FirstObject.GetComponent<LobbyPlayerView>();
                _networkManager.ServerManager.Despawn(lobbyPlayer.gameObject, DespawnType.Destroy);
            }

            ServerCheckAllPlayersReady();
        }

        [Server]
        private void ServerCheckAllPlayersReady()
        {
            var clientConnections = _networkManager.ServerManager.Clients.Values;

            if (clientConnections.Count <= 1)
            {
                StopCoroutine(_playGameCountdownCoroutine);
                PublishAllLobbyPlayersReadyCountdown(false, 0);
                Debug.Log($"[Server] Not enough player to start game.");
                return;
            }
            
            var allPlayersReady = clientConnections.All(connection =>
            {
                var isConnectionOwnedLobbyPlayer = connection.FirstObject.TryGetComponent<LobbyPlayerView>(out var lobbyPlayerView);
                
                if (!isConnectionOwnedLobbyPlayer)
                {
                    return false;
                }
                
                return lobbyPlayerView.Ready;
            });

            if (allPlayersReady)
            {
                _playGameCountdownCoroutine = PlayGameCountdown();
                StartCoroutine(_playGameCountdownCoroutine);
            }
            else
            {
                Debug.Log($"[Server] All players not ready yet to start game.");
                StopCoroutine(_playGameCountdownCoroutine);
                PublishAllLobbyPlayersReadyCountdown(false, 0);
            }
        }

        private IEnumerator PlayGameCountdown()
        {
            var countdown = 5;
            
            while (countdown > 0)
            {
                PublishAllLobbyPlayersReadyCountdown(true, countdown);
                yield return new WaitForSeconds(1);
                countdown--;
            }
            
            _networkManager.SceneManager.LoadGlobalScenes(new SceneLoadData(sceneNames: new []{"PlayScene"}));
            _networkManager.SceneManager.UnloadGlobalScenes(new SceneUnloadData(sceneNames: new []{"LobbyScene"}));
        }
        
        [ObserversRpc]
        private void PublishAllLobbyPlayersReadyCountdown(bool isEnabled, int countdown)
        {
            _allLobbyPlayersReadyCountdownPublisher.Publish(new AllLobbyPlayersReadyCountdown(isEnabled, countdown));
        }

        #endregion
    }
}