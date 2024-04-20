using MessagePipe;
using RudyAtkinson.Gameplay.Controller;
using RudyAtkinson.Gameplay.Message;
using RudyAtkinson.Gameplay.Repository;
using RudyAtkinson.Gameplay.View;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.Repository;
using RudyAtkinson.Tile.View;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RudyAtkinson.Scopes
{
    public class PlayScope: LifetimeScope
    {
        [SerializeField] private TMP_Text _informerTextPrefab;
        [SerializeField] private GameplayView _gameplayView;
        [SerializeField] private GameplayNetworkController gameplayNetworkController;
        [SerializeField] private TileView _tileView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_informerTextPrefab);
            builder.RegisterComponent(gameplayNetworkController);
            builder.RegisterComponent(_tileView);
            builder.RegisterComponent(_gameplayView);

            builder.Register<TileRepository>(Lifetime.Singleton);
            builder.Register<GameplayRepository>(Lifetime.Singleton);
            builder.Register<GameplayController>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterMessagePipe();
            builder.RegisterEntryPoint<TileClick>();
            builder.RegisterEntryPoint<NewGameCountdown>();
            builder.RegisterEntryPoint<NewGameStart>();
            builder.RegisterEntryPoint<LeaveButtonClickMessage>();
        }
    }
}