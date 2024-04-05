using FishNet.Managing;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.Lobby.Controller;
using RudyAtkinson.Lobby.Repository;
using RudyAtkinson.Lobby.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RudyAtkinson.Scopes
{
    public class LobbyScope : LifetimeScope
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private FishyEOS _fishyEos;
        [SerializeField] private LobbyView lobbyView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_networkManager);
            builder.RegisterComponent(_fishyEos);
            builder.RegisterComponent(lobbyView);
            
            builder.Register<LobbyController>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            builder.Register<LobbyRepository>(Lifetime.Singleton);
        }
    }
}
