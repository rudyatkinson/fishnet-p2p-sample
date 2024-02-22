using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Script.Player
{
    public class PlayerController: NetworkBehaviour
    {
        private readonly SyncVar<int> _health = new(initialValue: 100, new SyncTypeSettings(writePermissions: WritePermission.ServerOnly, readPermissions: ReadPermission.Observers));

        public override void OnStartClient()
        {
            Debug.Log("OnStartClient");
            base.OnStartClient();
        }

        public override void OnStartServer()
        {
            Debug.Log("OnStartServer");
            base.OnStartServer();
        }

        private void OnEnable()
        {
            _health.OnChange += OnHealthChanged;
        }

        private void OnDisable()
        {
            _health.OnChange -= OnHealthChanged;
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
               Hit();
            }
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