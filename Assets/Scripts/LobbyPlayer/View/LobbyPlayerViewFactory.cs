using RudyAtkinson.Lobby.Repository;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.LobbyPlayer.View
{
    public class LobbyPlayerViewFactory
    {
        private LobbyPlayerView _prefab;
        private LobbyRepository _lobbyRepository;
        
        [Inject]
        public LobbyPlayerViewFactory(LobbyPlayerView prefab, 
            LobbyRepository lobbyRepository)
        {
            _prefab = prefab;
            _lobbyRepository = lobbyRepository;
        }

        public LobbyPlayerView Create()
        {
            var obj = Object.Instantiate(_prefab, null, true);
            obj.SetDependencies(_lobbyRepository);
            
            return obj;
        }
    }
}