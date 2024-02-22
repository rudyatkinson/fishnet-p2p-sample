using System;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using Zenject;

namespace Script.Networking
{
    public class NetworkController: IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly NetworkManager _networkManager;
        private readonly Tugboat _tugboat;

        public NetworkController(SignalBus signalBus, 
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
        }

        private void OnHostButtonClicked()
        {
            _tugboat.StartConnection(true);
            _tugboat.StartConnection(false);
        }

        private void OnJoinButtonClicked()
        {
            _tugboat.StartConnection(false);
        }
    }
}