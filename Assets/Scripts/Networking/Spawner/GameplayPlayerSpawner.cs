using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using RudyAtkinson.GameplayPlayer.Controller;
using VContainer;

namespace RudyAtkinson.Networking.Spawner
{
    public class GameplayPlayerSpawner : NetworkBehaviour
    {
        private NetworkManager _networkManager;
        private GameplayPlayerFactory _gameplayPlayerFactory;
        
        [Inject]
        private void Construct(NetworkManager networkManager,
            GameplayPlayerFactory gameplayPlayerFactory)
        {
            _networkManager = networkManager;
            _gameplayPlayerFactory = gameplayPlayerFactory;
        }
        
        public override void OnStartServer()
        {
            _networkManager.SceneManager.OnClientPresenceChangeEnd += OnClientPresenceChange;
        }

        public override void OnStopServer()
        {
            _networkManager.SceneManager.OnClientPresenceChangeEnd -= OnClientPresenceChange;
        }

        private void OnClientPresenceChange(ClientPresenceChangeEventArgs obj)
        {
            var isPlayerContainsScene = obj.Connection.Scenes.Contains(base.NetworkObject.gameObject.scene);
            if (!isPlayerContainsScene)
            {
                return;
            }

            var player = _gameplayPlayerFactory.Create();
            
            _networkManager.ServerManager.Spawn(player.gameObject, obj.Connection);
        }
    }
}
