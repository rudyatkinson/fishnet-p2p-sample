using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using Script.Player.LobbyPlayer.View;
using UnityEngine;
using Zenject;

namespace Script.Networking.Lobby
{
    // TODO: Organise all players ready logic and fix host's lobby player obj was not create sometimes bug.
    public class LobbyServerController: NetworkBehaviour
    {
        private NetworkManager _networkManager;

        private List<LobbyPlayerView> _lobbyPlayers = new();
        private int readyPlayerCount = 0;
        
        [Inject]
        public void Construct(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            
        }

        public void AddLobbyPlayer(LobbyPlayerView lobbyPlayerView)
        {
            _lobbyPlayers.Add(lobbyPlayerView);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RpcLobbyPlayerReady(LobbyPlayerView lobbyPlayer, NetworkConnection conn = null)
        {
            lobbyPlayer.IsReady.Value = !lobbyPlayer.IsReady.Value;

            if (lobbyPlayer.IsReady.Value)
            {
                readyPlayerCount++;
            }
            else
            {
                readyPlayerCount--;
            }
            
            var totalPlayerCount = _lobbyPlayers.Count;

            Debug.Log($"[RpcLobbyPlayerReady] totalPlayerCount: {totalPlayerCount}, readyPlayerCount: {readyPlayerCount}");

            RpcLobbyPlayerReadyConfirmation(totalPlayerCount == readyPlayerCount);
        }

        [ObserversRpc]
        private void RpcLobbyPlayerReadyConfirmation(bool allPlayersReady)
        {
            Debug.Log($"All players are ready: {allPlayersReady}");
        }
    }
}