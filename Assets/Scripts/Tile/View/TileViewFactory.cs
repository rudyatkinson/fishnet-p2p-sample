using MessagePipe;
using RudyAtkinson.Tile.Model;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Tile.View
{
    public class TileViewFactory
    {
        private TileView _tileView;
        private IPublisher<TileClick> _tileClickPublisher;

        [Inject]
        private TileViewFactory(TileView tileView,
            IPublisher<TileClick> tileClickPublisher)
        {
            _tileView = tileView;
            _tileClickPublisher = tileClickPublisher;
        }

        public TileView Create(TileModel tileModel)
        {
            var obj = Object.Instantiate(_tileView, null, false);
            obj.Init(tileModel, _tileClickPublisher);
            
            return obj;
        }
    }
}