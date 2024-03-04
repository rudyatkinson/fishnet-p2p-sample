using System;
using System.Collections.Generic;
using System.Threading;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using R3;
using Script.Networking.Lobby.Model;
using Script.Networking.Lobby.View;
using Script.Player.LobbyPlayer.View;
using Zenject;

namespace Script.Networking.Lobby
{
    // TODO: Organise all players ready logic and fix host's lobby player obj was not create sometimes bug.
    public class LobbyServerController: NetworkBehaviour
    {
        private NetworkManager _networkManager;
        private LobbyView _lobbyView;

        private List<LobbyPlayerView> _lobbyPlayers = new();
        private int readyPlayerCount;

        private CancellationTokenSource _startCountdownIntervalToken;

        [Inject]
        public void Construct(NetworkManager networkManager,
            LobbyView lobbyView)
        {
            _networkManager = networkManager;
            _lobbyView = lobbyView;
        }

        public void AddLobbyPlayer(LobbyPlayerView lobbyPlayerView)
        {
            _lobbyPlayers.Add(lobbyPlayerView);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void CmdLobbyPlayerReady(LobbyPlayerView lobbyPlayer, NetworkConnection conn = null)
        {
            lobbyPlayer.IsReady.Value = !lobbyPlayer.IsReady.Value;

            if (lobbyPlayer.IsReady.Value)
            {
                readyPlayerCount++;
            }
            else
            {
                readyPlayerCount--;
            }
            
            var totalPlayerCount = _lobbyPlayers.Count;

            if (readyPlayerCount < totalPlayerCount)
            {
                _startCountdownIntervalToken?.Cancel();
                
                RpcDisableLobbyStartCountdown();
                
                return;
            }

            _startCountdownIntervalToken = new CancellationTokenSource();
            var countdown = 5;
            
            Observable.Interval(TimeSpan.FromSeconds(1), _startCountdownIntervalToken.Token)
                .TakeWhile(_ => countdown >= 0)
                .Subscribe(_ =>
                {
                    if (countdown <= 0)
                    {
                        RpcLoadPlayScene();
                    }
                    
                    RpcLobbyStartCountdownNotifier(countdown--);
                });

        }

        [ServerRpc(RequireOwnership = false)]
        private void RpcLoadPlayScene()
        {
            _networkManager.SceneManager.UnloadGlobalScenes(new SceneUnloadData(sceneName: Scenes.LobbyScene.ToString()));
            _networkManager.SceneManager.LoadGlobalScenes(new SceneLoadData(sceneName: Scenes.PlayScene.ToString())
            {
                PreferredActiveScene = new PreferredScene(new SceneLookupData(name:Scenes.PlayScene.ToString()))
            });
        }

        [ObserversRpc]
        private void RpcLobbyStartCountdownNotifier(int countDown)
        {
            _lobbyView.RefreshLobbyStartCountdown(countDown);
        }

        [ObserversRpc] 
        private void RpcDisableLobbyStartCountdown()
        {
            _lobbyView.DisableLobbyStartCountdown();
        }
    }
}