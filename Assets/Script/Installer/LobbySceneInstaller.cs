using Script.Networking.Lobby;
using Script.Networking.Lobby.View;
using Script.Player.LobbyPlayer.Network;
using Script.Player.LobbyPlayer.Player;
using UnityEngine;
using Zenject;

namespace Script.Installer
{
    public class LobbySceneInstaller: MonoInstaller
    {
        [SerializeField] private LobbyServerController _lobbyServerController;
        [SerializeField] private LobbyPlayerSpawner _lobbyPlayerSpawner;
        [SerializeField] private LobbyView _lobbyView;
    
        public override void InstallBindings()
        {
            Container.Bind<LobbyServerController>().FromInstance(_lobbyServerController).AsSingle().NonLazy();
            Container.Bind<LobbyPlayerSpawner>().FromInstance(_lobbyPlayerSpawner).AsSingle().NonLazy();
            Container.Bind<LobbyView>().FromInstance(_lobbyView).AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<LobbyPlayerController>().AsSingle().NonLazy();
        }
    }
}