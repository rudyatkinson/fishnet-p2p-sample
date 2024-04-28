using System;
using FishNet.Managing;
using MessagePipe;
using RudyAtkinson.Gameplay.Message;
using VContainer.Unity;

namespace RudyAtkinson.Gameplay.Controller
{
    public class GameplayController: IStartable
    {
        private readonly NetworkManager _networkManager;
        
        private readonly ISubscriber<DisconnectButtonClickMessage> _disconnectButtonClickSubscriber;

        private IDisposable _subscriberDisposables;
        
        public GameplayController(ISubscriber<DisconnectButtonClickMessage> disconnectButtonClickSubscriber,
            NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _disconnectButtonClickSubscriber = disconnectButtonClickSubscriber;
        }
        
        public void Start()
        {
            var disconnectButtonClickSubscriberDisposable = _disconnectButtonClickSubscriber.Subscribe(OnDisconnectButtonClick);

            _subscriberDisposables = DisposableBag.Create(disconnectButtonClickSubscriberDisposable);
        }

        ~GameplayController()
        {
            _subscriberDisposables?.Dispose();
        }

        private void OnDisconnectButtonClick(DisconnectButtonClickMessage args)
        {
            if (_networkManager.IsHostStarted)
            {
                _networkManager.ServerManager.StopConnection(true);
            }
            
            _networkManager.ClientManager.StopConnection();
        }
    }
}