using MessagePipe;
using RudyAtkinson.Gameplay.Controller;
using RudyAtkinson.Gameplay.Repository;
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
        [SerializeField] private GameplayNetworkController gameplayNetworkController;
        [SerializeField] private TileView _tileView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(gameplayNetworkController);
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