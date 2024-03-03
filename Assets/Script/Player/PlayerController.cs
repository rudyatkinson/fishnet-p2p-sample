using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Script.Installer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script.Player
{
    public class PlayerController: NetworkBehaviour
    {
        private PlayerInput _playerInput;
        
        private readonly SyncVar<int> _health = new(initialValue: 100, new SyncTypeSettings(writePermissions: WritePermission.ServerOnly, readPermissions: ReadPermission.Observers));

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
            {
                return;
            }
            
            Inject();
            Subscribe();
        }

        // TODO: This injection method should be applied into both lobby player and player classes.
        private void Inject()
        {
            // TODO: Injection does not work on client side as it should be.
            _playerInput = PlaySceneInstaller.ContainerInstance.Resolve<PlayerInput>();
        }
        
        private void Subscribe()
        {
            _health.OnChange += OnHealthChanged;
        }

        private void OnDestroy()
        {
            _health.OnChange -= OnHealthChanged;
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }
            
            var moveValue = _playerInput.actions["Move"].ReadValue<Vector2>();
            
            // TODO: Movement speed must be configurized and authorized to server.
            transform.position += new Vector3(moveValue.x, moveValue.y, 0) * Time.deltaTime * 2f;
        }

        [Server]
        private void Hit()
        {
            _health.Value -= 10;
            Debug.Log($"[Player] Hit Health: {_health.Value}");
            RpcTest();
        }

        [Client]
        private void ClientRpcHitReceived()
        {
            Debug.Log($"[Player] ClientRpcHitReceived Health: {_health.Value}");
        }

        private void OnHealthChanged(int prev, int next, bool asServer)
        {
            
        }
        
        [ServerRpc(RunLocally = true)]
        private void RpcTest(NetworkConnection conn = null)
        {
            Debug.Log($"[Player] {conn?.ClientId} ServerRpcHitReceived Health: {_health.Value}");
        }
    }
}