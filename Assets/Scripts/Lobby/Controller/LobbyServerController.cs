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
        }
        
        public void OnDisable()
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnServerStateChanged;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientStateChanged;
        }

        #region Client Callbacks
        
        private void OnClientStateChanged(ClientConnectionStateArgs args)
        {
            Debug.Log($"[Client] Connection state: {args.ConnectionState}");
        }

        [TargetRpc]
        private void RpcSetLobbyPlayerName(NetworkConnection target)
        {
            Debug.Log($"[Client] target: {target.ClientId}");

            var lobbyPlayerValid = _networkManager.ClientManager.Connection.FirstObject.TryGetComponent<LobbyPlayerView>(out var lobbyPlayerView);

            if (lobbyPlayerValid)
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

                if (connection.IsHost)
                {
                    lobbyPlayer.ServerRPCSetName(_lobbyRepository.PlayerName);
                }
                else
                {
                    RpcSetLobbyPlayerName(connection);
                }
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