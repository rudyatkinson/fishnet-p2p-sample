using FishNet.Managing;
using MessagePipe;
using RudyAtkinson.Tile.Model;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.GameplayPlayer.Controller
{
    public class GameplayPlayerFactory
    {
        private GameplayPlayer _prefab;
        private NetworkManager _networkManager;
        private ISubscriber<TileClick> _tileClickSubscriber;
        
        [Inject]
        public GameplayPlayerFactory(GameplayPlayer prefab,
            NetworkManager networkManager,
            ISubscriber<TileClick> tileClickSubscriber)
        {
            _prefab = prefab;
            _networkManager = networkManager;
            _tileClickSubscriber = tileClickSubscriber;
        }

        public GameplayPlayer Create()
        {
            var obj = Object.Instantiate(_prefab, Vector3.one, Quaternion.identity);
            obj.SetDependencies(_networkManager, _tileClickSubscriber);
            
            return obj;
        }
    }
}