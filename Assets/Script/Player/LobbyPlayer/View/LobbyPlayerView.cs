using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Script.Networking.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Script.Player.LobbyPlayer.View
{
    public class LobbyPlayerView: NetworkBehaviour
    {
        private LobbyServerController _lobbyServerController;
        
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _readyButtonText;
        [SerializeField] private Button _readyButton;

        public readonly SyncVar<bool> IsReady = new(false, new SyncTypeSettings(writePermissions: WritePermission.ServerOnly, readPermissions: ReadPermission.Observers));

        [Inject]
        private void Construct(LobbyServerController lobbyServerController)
        {
            _lobbyServerController = lobbyServerController;
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            _readyButton.image.color = IsOwner ? Color.white : Color.clear;

            base.OnOwnershipClient(prevOwner);
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

            _lobbyServerController.CmdLobbyPlayerReady(this);
        }
        
        private void OnIsReadyChange(bool old, bool current, bool asServer)
        {
            _readyButtonText.color = current ? Color.green : Color.red;
        }
    }
}