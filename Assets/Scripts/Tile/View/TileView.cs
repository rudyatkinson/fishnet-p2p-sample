using System;
using FishNet.Object;
using MessagePipe;
using RudyAtkinson.Tile.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RudyAtkinson.Tile.View
{
    public class TileView : NetworkBehaviour, IPointerDownHandler
    {
        public TileModel Tile { get; private set; }
        
        [SerializeField] private Image _tileBackground;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _baseOddColor;
        [SerializeField] private Color _baseEvenColor;

        private IPublisher<TileClick> _tileClick;
        
        public void Init(TileModel tileModel,
            IPublisher<TileClick> tileClick)
        {
            Tile = tileModel;
            _tileClick = tileClick;
            
            SetBaseColor();
        }
        
        private void SetBaseColor()
        {
            _tileBackground.color = (Tile.X % 2 == 0 && Tile.Y % 2 != 0) || (Tile.X % 2 != 0 && Tile.Y % 2 == 0) ? _baseEvenColor : _baseOddColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _tileClick.Publish(new TileClick());
            
            SetBaseColor();
        }
    }
}
