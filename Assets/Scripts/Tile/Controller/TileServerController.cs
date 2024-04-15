using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Object;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.View;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace RudyAtkinson.Tile.Controller
{
    public class TileServerController : NetworkBehaviour
    {
        private NetworkManager _networkManager;
        private TileViewFactory _tileViewFactory;
        
        [SerializeField] private TileView[] _tileViews;
        
        [Inject]
        private void Construct(TileViewFactory tileViewFactory,
            NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _tileViewFactory = tileViewFactory;
        }
    }
}
