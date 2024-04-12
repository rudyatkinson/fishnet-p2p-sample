﻿using MessagePipe;
using RudyAtkinson.LobbyPlayer.Model;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.LobbyPlayer.View
{
    public class LobbyPlayerViewFactory
    {
        private LobbyPlayerView _prefab;
        private IPublisher<LobbyPlayerReady> _lobbyPlayerReady;
        
        [Inject]
        public LobbyPlayerViewFactory(LobbyPlayerView prefab, 
            IPublisher<LobbyPlayerReady> lobbyPlayerReady)
        {
            _prefab = prefab;
            _lobbyPlayerReady = lobbyPlayerReady;
        }

        public LobbyPlayerView Create()
        {
            var obj = Object.Instantiate(_prefab, null, true);
            obj.SetDependencies(_lobbyPlayerReady);
            
            return obj;
        }
    }
}