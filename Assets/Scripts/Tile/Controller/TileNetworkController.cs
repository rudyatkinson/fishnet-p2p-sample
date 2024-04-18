using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using MessagePipe;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.Repository;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Tile.Controller
{
    public class TileNetworkController : NetworkBehaviour
    {
        private ISubscriber<TileClick> _tileClickSubscriber;
        private TileRepository _tileRepository;

        private IDisposable _subscriberDisposables;
        
        [SerializeField] private TileView[] _tileViews;
        
        [Inject]
        private void Construct(ISubscriber<TileClick> tileClickSubscriber,
            TileRepository tileRepository)
        {
            _tileClickSubscriber = tileClickSubscriber;
            _tileRepository = tileRepository;
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
            tile.Color.Value = Color.white;
            
            var tileDict = _tileRepository.TileMarkQueueDictionary;
            tileDict.TryAdd(mark, new Queue<TileView>());
            
            var markQueue = tileDict[mark];
            markQueue.Enqueue(tile);

            if (markQueue.Count > 3)
            {
                var obsoleteTile = markQueue.Dequeue();
                var obsoleteTileModel = obsoleteTile.TileModel.Value;

                obsoleteTileModel.Mark = '.';
                
                obsoleteTile.TileModel.Value = obsoleteTileModel;
            }

            if (markQueue.Count >= 3)
            {
                var color = Color.white;
                color.a = .25f;
                markQueue.First().Color.Value = color;
            }
        }
    }
}
