using System;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using FishNet.Utility.Extension;
using Script.Networking.Signal;
using Zenject;
using Scenes = Script.Networking.Lobby.Model.Scenes;

namespace Script.Networking
{
    public class NetworkServerController: IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly NetworkManager _networkManager;
        private readonly Tugboat _tugboat;

        public NetworkServerController(SignalBus signalBus, 
            NetworkManager networkManager,
            Tugboat tugboat)
        {
            _signalBus = signalBus;
            _networkManager = networkManager;
            _tugboat = tugboat;

            _signalBus.Subscribe<HostButtonClickedSignal>(OnHostButtonClicked);
            _signalBus.Subscribe<JoinButtonClickedSignal>(OnJoinButtonClicked);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<HostButtonClickedSignal>(OnHostButtonClicked);
            _signalBus.Unsubscribe<JoinButtonClickedSignal>(OnJoinButtonClicked);
            
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionStateChanged;
        }

        private void OnHostButtonClicked()
        {
            _tugboat.StartConnection(true);
            _tugboat.StartConnection(false);
            
            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionStateChanged;
        }

        private void OnJoinButtonClicked()
        {
            _tugboat.StartConnection(false);
        }

        private void OnServerConnectionStateChanged(ServerConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case LocalConnectionState.Started:
                    
                    _networkManager.SceneManager.UnloadGlobalScenes(new SceneUnloadData());
                    _networkManager.SceneManager.LoadGlobalScenes(new SceneLoadData(sceneName: Scenes.LobbyScene.ToString()){ ReplaceScenes = ReplaceOption.All});
                    
                    break;
                case LocalConnectionState.Stopped:
                    
                    _networkManager.SceneManager.UnloadGlobalScenes(new SceneUnloadData());
                    _networkManager.SceneManager.LoadGlobalScenes(new SceneLoadData(sceneName: Scenes.OfflineScene.ToString()));

                    break;
            }
        }
    }
}