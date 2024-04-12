using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
                ServerRPCSetName(PlayerPrefs.GetString("rudyatkinson-player-name"));
                
                _readyToggle.onValueChanged.AddListener(ServerRPCPlayerReady);
            }
            else
            {
                _readyToggle.onValueChanged.RemoveListener(ServerRPCPlayerReady);
            }
        }

        private void SetActiveReadyButton(bool isActive)
        {
            _readyToggle.interactable = isActive;
            _readyToggle.targetGraphic.enabled = isActive;
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
        private void ServerRPCSetName(string playerName)
        {
            _name.Value = playerName;
        }
        
        [ServerRpc(RequireOwnership = true)]
        private void ServerRPCPlayerReady(bool isReady)
        {
            ChangeReady(isReady);
        }

        [Server]
        private void ChangeReady(bool isReady)
        {
            _ready.Value = isReady;
            _readyToggle.isOn = isReady;
        }

        #endregion
    }
}
