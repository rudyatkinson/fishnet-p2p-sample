using FishNet.Managing;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.Lobby.Controller;
using RudyAtkinson.Lobby.Repository;
using RudyAtkinson.Lobby.View;
using RudyAtkinson.LobbyPlayer.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RudyAtkinson.Scopes
{
    public class LobbyScope : LifetimeScope
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private FishyEOS _fishyEos;
        [SerializeField] private LobbyView _lobbyView;
        [SerializeField] private LobbyServerController _lobbyServerController;
        [SerializeField] private LobbyPlayerView _lobbyPlayerView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_networkManager);
            builder.RegisterComponent(_fishyEos);
            builder.RegisterComponent(_lobbyView);
            builder.RegisterComponent(_lobbyServerController);
            builder.RegisterComponent(_lobbyPlayerView);
            
            builder.Register<LobbyController>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            builder.Register<LobbyRepository>(Lifetime.Singleton);
            builder.Register<LobbyPlayerViewFactory>(Lifetime.Singleton);
        }
    }
}
