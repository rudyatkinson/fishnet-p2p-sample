using FishNet.Managing;
using FishNet.Object;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Tile.Controller
{
    public class TileServerController : NetworkBehaviour
    {
        private NetworkManager _networkManager;
        
        [SerializeField] private TileView[] _tileViews;
        
        [Inject]
        private void Construct(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }
    }
}
