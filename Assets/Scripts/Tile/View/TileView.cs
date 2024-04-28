using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MessagePipe;
using RudyAtkinson.Tile.Message;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.Repository;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace RudyAtkinson.Tile.View
{
    public class TileView : NetworkBehaviour, IPointerDownHandler
    {
        public readonly SyncVar<TileModel> TileModel = new (new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        public readonly SyncVar<Color> MarkColor = new (new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        
        [SerializeField] private Image _tileBackground;
        [SerializeField] private Color _baseOddColor;
        [SerializeField] private Color _baseEvenColor;
        [SerializeField] private TMP_Text _debugText;

        private IPublisher<TileClickMessage> _tileClick;
        private TileRepository _tileRepository;
        
        [Inject]
        private void Construct(IPublisher<TileClickMessage> tileClick,
            TileRepository tileRepository)
        {
            _tileClick = tileClick;
            _tileRepository = tileRepository;
        }

        public override void OnStartServer()
        {
            var index = transform.GetSiblingIndex();

            var x = index / 3;
            var y = index % 3;

            TileModel.Value = new TileModel(x, y);
            
            _tileRepository.TileViews.Add(this);
        }

        public override void OnStartClient()
        {
            TileModel.OnChange += OnTileModelModelChange;
            MarkColor.OnChange += OnMarkColorChange;
        }

        public override void OnStopClient()
        {
            TileModel.OnChange -= OnTileModelModelChange;
            MarkColor.OnChange -= OnMarkColorChange;
        }

        private void OnTileModelModelChange(TileModel prev, TileModel next, bool asserver)
        {
            SetBaseColor();
            
            _debugText.SetText(next.Mark.ToString());
        }

        private void OnMarkColorChange(Color prevColor, Color nextColor, bool asServer)
        {
            _debugText.color = nextColor;
        }

        private void SetBaseColor()
        {
            var tile = TileModel.Value;
            _tileBackground.color = (tile.X % 2 == 0 && tile.Y % 2 != 0) || (tile.X % 2 != 0 && tile.Y % 2 == 0) ? _baseEvenColor : _baseOddColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _tileClick.Publish(new TileClickMessage() {Tile = this});
            
            SetBaseColor();
        }
    }
}
