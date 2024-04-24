using FishNet.Managing;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.EOSLobby.Repository;
using RudyAtkinson.EOSLobby.Service;
using RudyAtkinson.Lobby.Repository;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RudyAtkinson.Scopes
{
    public class RootScope : LifetimeScope
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private FishyEOS _fishyEos;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_networkManager);
            builder.RegisterComponent(_fishyEos);
            
            builder.Register<EOSLobbyService>(Lifetime.Singleton);
            builder.Register<EOSLobbyRepository>(Lifetime.Singleton);
            builder.Register<LobbyRepository>(Lifetime.Singleton);
        }
    }
}
