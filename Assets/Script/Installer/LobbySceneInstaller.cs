using Script.Player.LobbyPlayer.Player;
using UnityEngine;
using Zenject;

namespace Script.Installer
{
    public class LobbySceneInstaller: MonoInstaller
    {
        [SerializeField] private LobbyPlayerServerController _lobbyPlayerServerController;
    
        public override void InstallBindings()
        {
            Container.Bind<LobbyPlayerServerController>().FromInstance(_lobbyPlayerServerController).AsSingle().NonLazy();
        }
    }
}