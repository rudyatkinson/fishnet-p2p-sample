using RudyAtkinson.Lobby.Repository;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.LobbyPlayer.View
{
    public class LobbyPlayerViewFactory
    {
        private LobbyPlayerView _prefab;
        
        [Inject]
        public LobbyPlayerViewFactory(LobbyPlayerView prefab)
        {
            _prefab = prefab;
        }

        public LobbyPlayerView Create()
        {
            var obj = Object.Instantiate(_prefab, null, true);
            
            return obj;
        }
    }
}