using MessagePipe;
using RudyAtkinson.Gameplay.Repository;
using RudyAtkinson.Tile.Controller;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.Repository;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RudyAtkinson.Scopes
{
    public class PlayScope: LifetimeScope
    {
        [SerializeField] private TileNetworkController _tileNetworkController;
        [SerializeField] private TileView _tileView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_tileNetworkController);
            builder.RegisterComponent(_tileView);

            builder.Register<TileRepository>(Lifetime.Singleton);
            builder.Register<GameplayRepository>(Lifetime.Singleton);

            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<TileClick>();
            builder.RegisterEntryPoint<NewGameCountdown>();
            builder.RegisterEntryPoint<NewGameStart>();
        }
    }
}