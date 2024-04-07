using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;

namespace RudyAtkinson.LobbyPlayer.View
{
    public class LobbyPlayerView : NetworkBehaviour
    {
        private readonly SyncVar<bool> _ready = new (new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        
        [SerializeField] private Toggle _readyToggle;
        [SerializeField] private Button _readyButton;

        public override void OnStartClient()
        {
            _ready.OnChange += OnReadyChange;
        }

        public override void OnStopClient()
        {
            _ready.OnChange -= OnReadyChange;
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            SetActiveReadyButton(IsOwner);

            if (IsOwner)
            {
                _readyButton.onClick.AddListener(OnReadyButtonClick);
            }
            else
            {
                _readyButton.onClick.RemoveListener(OnReadyButtonClick);
            }
        }

        private void OnReadyButtonClick()
        {
            ServerRPCPlayerReady();
        }

        public void SetActiveReadyButton(bool isActive)
        {
            _readyButton.gameObject.SetActive(isActive);
        }
        
        [ServerRpc(RequireOwnership = true)]
        private void ServerRPCPlayerReady(NetworkConnection conn = null)
        {
            ChangeReady();
        }

        [Server]
        private void ChangeReady()
        {
            _ready.Value = !_ready.Value;
            _readyToggle.isOn = _ready.Value;
        }
        
        [ObserversRpc]
        private void OnReadyChange(bool prev, bool next, bool asServer)
        {
            if (!asServer)
            {
                return;
            }

            _readyToggle.isOn = next;
        }
    }
}
