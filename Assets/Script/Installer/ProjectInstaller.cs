using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using Script.Networking;
using UnityEngine;
using Zenject;

namespace Script.Installer
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private Tugboat _tugboat;
    
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<HostButtonClickedSignal>();
            Container.DeclareSignal<JoinButtonClickedSignal>();
            
            Container.Bind<NetworkManager>().FromInstance(_networkManager).AsSingle().NonLazy();
            Container.Bind<Tugboat>().FromInstance(_tugboat).AsSingle().NonLazy();
        
            Container.BindInterfacesTo<NetworkController>().AsSingle().NonLazy();
        }
    }
}