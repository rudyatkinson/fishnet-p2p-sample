using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Script.Player.LobbyPlayer.Signal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Script.Player.LobbyPlayer.View
{
    public class LobbyPlayerView: NetworkBehaviour
    {
        private SignalBus _signalBus;
        
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Button _readyButton;

        public readonly SyncVar<bool> IsReady = new(false, new SyncTypeSettings(writePermissions: WritePermission.ServerOnly, readPermissions: ReadPermission.Observers));

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        private void OnEnable()
        {
            _readyButton.onClick.AddListener(ReadyButtonClicked);

            IsReady.OnChange += OnIsReadyChange;
        }

        private void OnDisable()
        {
            _readyButton.onClick.RemoveListener(ReadyButtonClicked);
            
            IsReady.OnChange -= OnIsReadyChange;
        }

        private void ReadyButtonClicked()
        {
            if (!IsOwner)
            {
                return;
            }
            
            _signalBus.Fire(new LobbyPlayerReady() { LobbyPlayer = this });
        }
        
        private void OnIsReadyChange(bool old, bool current, bool asServer)
        {
            _readyButton.image.color = current ? Color.green : Color.white;
        }
    }
}