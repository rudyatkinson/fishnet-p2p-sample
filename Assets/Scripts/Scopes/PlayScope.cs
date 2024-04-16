using MessagePipe;
using RudyAtkinson.GameplayPlayer.Controller;
using RudyAtkinson.Tile.Controller;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RudyAtkinson.Scopes
{
    public class PlayScope: LifetimeScope
    {
        [SerializeField] private TileServerController _tileServerController;
        [SerializeField] private TileView _tileView;
        [SerializeField] private GameplayPlayer.Controller.GameplayPlayer _gameplayPlayer;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_tileServerController);
            builder.RegisterComponent(_tileView);
            builder.RegisterComponent(_gameplayPlayer);
            
            builder.Register<GameplayPlayerFactory>(Lifetime.Singleton);

            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<TileClick>();
        }
    }
}