using FishNet.Managing;
using FishNet.Transporting.FishyEOSPlugin;
using MessagePipe;
using RudyAtkinson.Lobby.Controller;
using RudyAtkinson.Lobby.Model;
using RudyAtkinson.Lobby.Repository;
using RudyAtkinson.Lobby.View;
using RudyAtkinson.LobbyPlayer.Model;
using RudyAtkinson.LobbyPlayer.View;
using RudyAtkinson.Tile.Model;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RudyAtkinson.Scopes
{
    public class LobbyScope : LifetimeScope
    {
        [SerializeField] private LobbyView _lobbyView;
        [SerializeField] private LobbyServerController _lobbyServerController;
        [SerializeField] private LobbyPlayerView _lobbyPlayerView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_lobbyView);
            builder.RegisterComponent(_lobbyServerController);
            builder.RegisterComponent(_lobbyPlayerView);
            
            builder.Register<LobbyController>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            builder.Register<LobbyRepository>(Lifetime.Singleton);
            builder.Register<LobbyPlayerViewFactory>(Lifetime.Singleton);

            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<LobbyPlayerReady>();
            builder.RegisterEntryPoint<AllLobbyPlayersReadyCountdown>();
        }
    }
}
