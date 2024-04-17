using System;
using FishNet.Connection;
using FishNet.Object;
using MessagePipe;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Tile.Controller
{
    public class TileServerController : NetworkBehaviour
    {
        private ISubscriber<TileClick> _tileClickSubscriber;

        private IDisposable _subscriberDisposables;
        
        [SerializeField] private TileView[] _tileViews;
        
        [Inject]
        private void Construct(ISubscriber<TileClick> tileClickSubscriber)
        {
            _tileClickSubscriber = tileClickSubscriber;
        }

        private void OnEnable()
        {
            var tileClickDisposable = _tileClickSubscriber.Subscribe(tileClick =>
            {
                Server_OnTileClick(tileClick);
            });

            _subscriberDisposables = DisposableBag.Create(tileClickDisposable);
        }

        private void OnDisable()
        {
            _subscriberDisposables?.Dispose();
        }

        [ServerRpc(RequireOwnership = false)]
        private void Server_OnTileClick(TileClick tileClick, NetworkConnection conn = null)
        {
            var mark = conn.IsHost ? 'X' : 'O';

            var tile = tileClick.Tile;
            var tileModel = tile.TileModel.Value;

            if (mark == tileModel.Mark)
            {
                Debug.Log($"Player clicked the tile owned by already.");
                return;
            }
            
            tileModel.Mark = mark;

            tile.TileModel.Value = tileModel;
        }
    }
}
