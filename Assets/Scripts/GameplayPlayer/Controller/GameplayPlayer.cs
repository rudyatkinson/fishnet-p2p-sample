using System;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using MessagePipe;
using RudyAtkinson.Tile.Model;

namespace RudyAtkinson.GameplayPlayer.Controller
{
    public class GameplayPlayer : NetworkBehaviour
    {
        private NetworkManager _networkManager;
        private ISubscriber<TileClick> _onTileClickSubscriber;

        private IDisposable _eventDisposables;
        
        public void SetDependencies(NetworkManager networkManager,
            ISubscriber<TileClick> tileClickSubscriber)
        {
            _networkManager = networkManager;
            _onTileClickSubscriber = tileClickSubscriber;
        }
        
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            if (prevOwner == _networkManager.ClientManager.Connection)
            {
                _eventDisposables?.Dispose();
            }
            else
            {
                var tileClickDisposable = _onTileClickSubscriber.Subscribe(OnTileClick);

                _eventDisposables = DisposableBag.Create(tileClickDisposable);
            }
        }

        public override void OnStopClient()
        {
            _eventDisposables?.Dispose();
        }

        [ServerRpc(RequireOwnership = true)]
        private void OnTileClick(TileClick tileClick)
        {
            
        }
    }
}
