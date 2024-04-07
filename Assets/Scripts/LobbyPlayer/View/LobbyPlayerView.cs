using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using RudyAtkinson.Lobby.Repository;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RudyAtkinson.LobbyPlayer.View
{
    public class LobbyPlayerView : NetworkBehaviour
    {
        private readonly SyncVar<string> _name = new (new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));
        private readonly SyncVar<bool> _ready = new (new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private Toggle _readyToggle;
        [SerializeField] private Button _readyButton;

        #region Client

        public override void OnStartClient()
        {
            _name.OnChange += OnNameChange;
            _ready.OnChange += OnReadyChange;
        }

        public override void OnStopClient()
        {
            _name.OnChange -= OnNameChange;
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

        private void SetActiveReadyButton(bool isActive)
        {
            _readyButton.gameObject.SetActive(isActive);
        }
        
        private void OnNameChange(string prev, string next, bool asServer)
        {
            _playerNameText.SetText(next);
        }
        
        private void OnReadyChange(bool prev, bool next, bool asServer)
        {
            _readyToggle.isOn = next;
        }

        #endregion

        #region Server

        [ServerRpc(RequireOwnership = true)]
        public void ServerRPCSetName(string playerName)
        {
            _name.Value = playerName;
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

        #endregion
    }
}
