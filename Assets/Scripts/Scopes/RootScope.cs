using FishNet.Managing;
using FishNet.Transporting.FishyEOSPlugin;
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
        }
    }
}
