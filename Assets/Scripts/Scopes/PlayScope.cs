using MessagePipe;
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
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_tileServerController);
            builder.RegisterComponent(_tileView);

            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<TileClick>();
        }
    }
}