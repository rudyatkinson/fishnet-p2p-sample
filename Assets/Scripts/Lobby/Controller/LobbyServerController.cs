using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.Lobby.Repository;
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
        private LobbyRepository _lobbyRepository;

        [SerializeField] private LobbyPlayerView _lobbyPlayerView;
        [SerializeField] private Transform _lobbyPlayerParent;
        
        [Inject]
        private void Construct(NetworkManager networkManager,
            FishyEOS fishyEos,
            LobbyPlayerViewFactory lobbyPlayerViewFactory,
            LobbyRepository lobbyRepository)
        {
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _lobbyPlayerViewFactory = lobbyPlayerViewFactory;
            _lobbyRepository = lobbyRepository;
        }
        
        public void OnEnable()
        {
            _networkManager.ServerManager.OnServerConnectionState += OnServerStateChanged;
            _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += OnClientStateChanged;
            _networkManager.ClientManager.Connection.OnObjectAdded += OnConnectionObjectAdded;
        }
        
        public void OnDisable()
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnServerStateChanged;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientStateChanged;
            _networkManager.ClientManager.Connection.OnObjectAdded -= OnConnectionObjectAdded;
        }

        #region Client Callbacks
        
        private void OnClientStateChanged(ClientConnectionStateArgs args)
        {
            Debug.Log($"[Client] Connection state: {args.ConnectionState}");
        }
        
        private void OnConnectionObjectAdded(NetworkObject obj)
        {
            var lobbyPlayerViewValid = obj.TryGetComponent<LobbyPlayerView>(out var lobbyPlayerView);

            if (lobbyPlayerViewValid)
            {
                lobbyPlayerView.ServerRPCSetName(_lobbyRepository.PlayerName);
            }
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
                var lobbyPlayerTransform = lobbyPlayer.transform;
                
                lobbyPlayerTransform.SetParent(_lobbyPlayerParent);
                lobbyPlayerTransform.localScale = Vector3.one;
                
                _networkManager.ServerManager.Spawn(lobbyPlayer.gameObject, connection);
            }
            else
            {
                var lobbyPlayer = connection.FirstObject.GetComponent<LobbyPlayerView>();
                _networkManager.ServerManager.Despawn(lobbyPlayer.gameObject, DespawnType.Destroy);
            }
        }
        
        #endregion
    }
}