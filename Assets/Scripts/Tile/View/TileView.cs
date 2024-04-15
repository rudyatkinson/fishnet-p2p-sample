using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MessagePipe;
using RudyAtkinson.Tile.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace RudyAtkinson.Tile.View
{
    public class TileView : NetworkBehaviour, IPointerDownHandler
    {
        private readonly SyncVar<TileModel> _tileModel = new (new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        
        [SerializeField] private Image _tileBackground;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _baseOddColor;
        [SerializeField] private Color _baseEvenColor;

        private IPublisher<TileClick> _tileClick;
        
        [Inject]
        private void Construct(IPublisher<TileClick> tileClick)
        {
            _tileClick = tileClick;
        }

        public override void OnStartServer()
        {
            var index = transform.GetSiblingIndex();

            var x = index / 3;
            var y = index % 3;

            _tileModel.Value = new TileModel(x, y, '.');
        }

        public override void OnStartClient()
        {
            _tileModel.OnChange += OnTileModelModelChange;
        }

        public override void OnStopClient()
        {
            _tileModel.OnChange -= OnTileModelModelChange;
        }

        private void OnTileModelModelChange(TileModel prev, TileModel next, bool asserver)
        {
            SetBaseColor();
        }

        private void SetBaseColor()
        {
            var tile = _tileModel.Value;
            _tileBackground.color = (tile.X % 2 == 0 && tile.Y % 2 != 0) || (tile.X % 2 != 0 && tile.Y % 2 == 0) ? _baseEvenColor : _baseOddColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _tileClick.Publish(new TileClick());
            
            SetBaseColor();
        }

        public TileModel TileModel => _tileModel.Value;
    }
}
