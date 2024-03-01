using Script.Player.LobbyPlayer.Network;
using UnityEngine;
using Zenject;

namespace Script.Installer
{
    public class PlaySceneInstaller: MonoInstaller
    {
        [SerializeField] private PlayScenePlayerSpawner _playerSpawner;
        
        public override void InstallBindings()
        {
            Container.Bind<PlayScenePlayerSpawner>().FromInstance(_playerSpawner).AsSingle().NonLazy();
        }
    }
}