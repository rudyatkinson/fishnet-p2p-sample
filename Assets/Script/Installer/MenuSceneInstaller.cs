using UnityEngine;
using Zenject;
using NetworkView = Script.Networking.NetworkView;

public class MenuSceneInstaller : MonoInstaller
{
    [SerializeField] private NetworkView _networkView;
    
    public override void InstallBindings()
    {
        Container.Bind<NetworkView>().FromInstance(_networkView).AsSingle().NonLazy();
    }
}