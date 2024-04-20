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
        
        private readonly ISubscriber<LeaveButtonClick> _leaveButtonClickSubscriber;

        private IDisposable _subscriberDisposables;
        
        public GameplayController(ISubscriber<LeaveButtonClick> leaveButtonClickSubscriber,
            NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _leaveButtonClickSubscriber = leaveButtonClickSubscriber;
        }
        
        public void Start()
        {
            var leaveButtonClickSubscriberDisposable = _leaveButtonClickSubscriber.Subscribe(OnLeaveButtonClick);

            _subscriberDisposables = DisposableBag.Create(leaveButtonClickSubscriberDisposable);
        }

        ~GameplayController()
        {
            _subscriberDisposables?.Dispose();
        }

        private void OnLeaveButtonClick(LeaveButtonClick leaveButtonClick)
        {
            if (_networkManager.IsHostStarted)
            {
                _networkManager.ServerManager.StopConnection(true);
            }
            else
            {
                _networkManager.ClientManager.StopConnection();
            }
        }
    }
}