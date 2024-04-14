using System.Linq;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Tile.Controller
{
    public class TileServerController : NetworkBehaviour
    {
        private const int _height = 360;
        private const int _width = 360;

        private NetworkManager _networkManager;
        private TileViewFactory _tileViewFactory;
        
        [Inject]
        private void Construct(TileViewFactory tileViewFactory,
            NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _tileViewFactory = tileViewFactory;
        }

        [Server]
        public override void OnStartServer()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var tileModel = new TileModel(i, j, '.');
                    var pos = new Vector3(_width * i, _height * j);
                    
                    var obj = _tileViewFactory.Create(tileModel);
                    obj.NetworkObject.SetParent(this);

                    var objTransform = obj.transform;
                    objTransform.localScale = Vector3.one;
                    objTransform.localPosition = pos;
                    
                    _networkManager.ServerManager.Spawn(obj.gameObject, null);
                }
            }
        }
    }
}
