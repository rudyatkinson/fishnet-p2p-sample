using System;
using Script.Player.LobbyPlayer.Network;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Script.Installer
{
    public class PlaySceneInstaller: MonoInstaller
    {
        public static DiContainer ContainerInstance;

        public static event Action InstallComplete;
        
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private PlayScenePlayerSpawner _playerSpawner;

        private void Awake()
        {
            ContainerInstance = Container;
        }

        public override void InstallBindings()
        {
            Container.Bind<PlayerInput>().FromInstance(_playerInput).AsSingle().NonLazy();
            Container.Bind<PlayScenePlayerSpawner>().FromInstance(_playerSpawner).AsSingle().NonLazy();
            
            InstallComplete?.Invoke();
        }
    }
}