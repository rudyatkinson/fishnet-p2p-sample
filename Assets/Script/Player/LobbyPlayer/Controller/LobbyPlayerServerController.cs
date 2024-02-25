using System;
using FishNet.Connection;
using FishNet.Object;
using Script.Player.LobbyPlayer.Signal;
using Script.Player.LobbyPlayer.View;
using UnityEngine;
using Zenject;

namespace Script.Player.LobbyPlayer.Player
{
    public class LobbyPlayerServerController: NetworkBehaviour
    {
        private SignalBus _signalBus;
        
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            
            _signalBus.Subscribe<LobbyPlayerReady>(OnLobbyPlayerReady);
        }
        
        public void Dispose()
        {
            _signalBus.Unsubscribe<LobbyPlayerReady>(OnLobbyPlayerReady);
        }

        private void OnLobbyPlayerReady(LobbyPlayerReady obj)
        {
            RpcLobbyPlayerReady(obj.LobbyPlayer);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RpcLobbyPlayerReady(LobbyPlayerView lobbyPlayer, NetworkConnection conn = null)
        {
            lobbyPlayer.IsReady.Value = !lobbyPlayer.IsReady.Value;
            
            RpcLobbyPlayerReadyConfirmation(conn);
        }

        [TargetRpc]
        private void RpcLobbyPlayerReadyConfirmation(NetworkConnection conn)
        {
            Debug.Log($"Server confirmed your ready status!");
        }
    }
}