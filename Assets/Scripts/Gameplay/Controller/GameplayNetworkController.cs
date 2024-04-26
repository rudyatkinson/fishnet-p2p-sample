using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Plugins.FishyEOS.Util;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using MessagePipe;
using RudyAtkinson.EOSLobby.Repository;
using RudyAtkinson.EOSLobby.Service;
using RudyAtkinson.Gameplay.Message;
using RudyAtkinson.Gameplay.Repository;
using RudyAtkinson.Tile.Message;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.Repository;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Gameplay.Controller
{
    public class GameplayNetworkController : NetworkBehaviour
    {
        private TileRepository _tileRepository;
        private GameplayRepository _gameplayRepository;
        private FishyEOS _fishyEos;
        private EOSLobbyService _eosLobbyService;
        private EOSLobbyRepository _eosLobbyRepository;
        
        private IPublisher<NewGameCountdownMessage> _newGameCountdownPublisher;
        private IPublisher<NewGameStartMessage> _newGameStartPublisher;
        private IPublisher<UpdateWinScoresMessage> _updateWinScoresPublisher;
        private IPublisher<ShowWinConditionMessage> _showWinConditionPublisher;
        private IPublisher<UpdateTurnInfoMessage> _updateTurnInfoPublisher;
        private IPublisher<NotYourTurnMessage> _notYourTurnPublisher;

        private ISubscriber<TileClickMessage> _tileClickSubscriber;

        private IDisposable _subscriberDisposables;
        
        [Inject]
        private void Construct(ISubscriber<TileClickMessage> tileClickSubscriber,
            TileRepository tileRepository,
            GameplayRepository gameplayRepository,
            IPublisher<NewGameCountdownMessage> newGameCountdownPublisher,
            IPublisher<NewGameStartMessage> newGameStartPublisher,
            FishyEOS fishyEos,
            IPublisher<UpdateWinScoresMessage> updateWinScoresPublisher,
            IPublisher<ShowWinConditionMessage> showWinConditionPublisher,
            IPublisher<UpdateTurnInfoMessage> updateTurnInfoPublisher,
            IPublisher<NotYourTurnMessage> notYourTurnPublisher,
            EOSLobbyService eosLobbyService,
            EOSLobbyRepository eosLobbyRepository)
        {
            _tileClickSubscriber = tileClickSubscriber;
            _tileRepository = tileRepository;
            _gameplayRepository = gameplayRepository;
            _newGameCountdownPublisher = newGameCountdownPublisher;
            _newGameStartPublisher = newGameStartPublisher;
            _fishyEos = fishyEos;
            _updateWinScoresPublisher = updateWinScoresPublisher;
            _showWinConditionPublisher = showWinConditionPublisher;
            _updateTurnInfoPublisher = updateTurnInfoPublisher;
            _notYourTurnPublisher = notYourTurnPublisher;
            _eosLobbyService = eosLobbyService;
            _eosLobbyRepository = eosLobbyRepository;
        }

        private void OnEnable()
        {
            var tileClickDisposable = _tileClickSubscriber.Subscribe(tileClick =>
            {
                Server_OnTileClick(tileClick);
            });

            _fishyEos.OnClientConnectionState += OnClientConnectionStateChange;

            _subscriberDisposables = DisposableBag.Create(tileClickDisposable);
        }

        private void OnDisable()
        {
            _fishyEos.OnClientConnectionState -= OnClientConnectionStateChange;

            _subscriberDisposables?.Dispose();
        }

        #region Server Logic
        [ServerRpc(RequireOwnership = false)]
        private void Server_OnTileClick(TileClickMessage tileClickMessage, NetworkConnection conn = null)
        {
            if (_gameplayRepository.GetPlayerInputLocked())
            {
                Debug.Log($"[Server] Tried to click a tile during input locked.");
                return;
            }
            
            var hostMark = 'X';
            var opponentMark = 'O';
            var isHost = conn.IsHost;
            var mark = isHost ? hostMark : opponentMark;

            var tile = tileClickMessage.Tile;
            var tileModel = tile.TileModel.Value;
            var tileMark = tileModel.Mark;

            if (mark != _gameplayRepository.GetMarkTurn())
            {
                Target_NotYourTurn(conn);
                return;
            }

            if (tileMark != ' ') 
            {
                Debug.Log($"[Server] Clicked Tile owned already!");
                return;
            }
            
            tileModel.Mark = mark;

            tile.TileModel.Value = tileModel;
            tile.MarkColor.Value = Color.white;
            
            var tileDict = _tileRepository.TileMarkQueueDictionary;
            tileDict.TryAdd(mark, new Queue<TileView>());
            
            var markQueue = tileDict[mark];
            markQueue.Enqueue(tile);

            if (markQueue.Count > 3)
            {
                var obsoleteTile = markQueue.Dequeue();
                var obsoleteTileModel = obsoleteTile.TileModel.Value;

                obsoleteTileModel.Mark = ' ';
                
                obsoleteTile.TileModel.Value = obsoleteTileModel;
            }
            
            var hasWon = CheckForWin(_tileRepository.MarkMatrix(), mark);
            if (hasWon)
            {
                LockPlayerInput(true);
                
                var winDict = _gameplayRepository.GetWinDict();
                winDict[mark]++;
                
                Observers_PlayerWin(isHost, winDict);

                StartNewGameCountdown().AsAsyncUnitUniTask().ContinueWith(_ =>
                {
                    ClearTiles();

                    LockPlayerInput(false);
                });

                return;
            }

            var nextTurnMark = isHost ? opponentMark : hostMark;
            
            var nextMarkQueue = tileDict[nextTurnMark];
            if (nextMarkQueue.Count >= 3)
            {
                var color = Color.white;
                color.a = .25f;
                nextMarkQueue.First().MarkColor.Value = color;
            }
            
            _gameplayRepository.SetMarkTurn(nextTurnMark);
            
            Observers_TurnChanged(!isHost);
        }
        
        private bool CheckForWin(char[,] grid, char playerMark)
        {
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                if (grid[row, 0] == playerMark && grid[row, 1] == playerMark && grid[row, 2] == playerMark)
                {
                    return true;
                }
            }
            
            for (int col = 0; col < grid.GetLength(1); col++)
            {
                if (grid[0, col] == playerMark && grid[1, col] == playerMark && grid[2, col] == playerMark)
                {
                    return true;
                }
            }
            
            if (grid[0, 0] == playerMark && grid[1, 1] == playerMark && grid[2, 2] == playerMark)
            {
                return true;
            }
            if (grid[0, 2] == playerMark && grid[1, 1] == playerMark && grid[2, 0] == playerMark)
            {
                return true;
            }

            return false;
        }
        
        private async UniTask StartNewGameCountdown()
        {
            for (int countdown = 5; countdown >= 0; countdown--)
            {
                Observers_NewGameCountdown(countdown);

                await UniTask.Delay(TimeSpan.FromSeconds(1));
                
                if (countdown <= 0)
                {
                    Observers_NewGameStart(_gameplayRepository.GetMarkTurn());
                }
            }
        }

        private void ClearTiles()
        {
            Debug.Log($"[Server] Tiles Cleared!");
            foreach (var kvp in _tileRepository.TileMarkQueueDictionary)
            {
                kvp.Value.Clear();
            }
            
            foreach (var tileView in _tileRepository.TileViews)
            {
                var tileModel = tileView.TileModel.Value;
                tileModel.Mark = ' ';

                tileView.TileModel.Value = tileModel;
                tileView.MarkColor.Value = Color.white;
            }
        }

        private void LockPlayerInput(bool isLocked)
        {
            Debug.Log($"[Server] Tiles locked: {isLocked}");
            _gameplayRepository.SetPlayerInputLocked(isLocked);
        }
        #endregion
        
        #region Client Logic
        private void OnClientConnectionStateChange(ClientConnectionStateArgs obj)
        {
            if (obj.ConnectionState == LocalConnectionState.Stopping)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
                _eosLobbyService.LeaveLobby(_eosLobbyRepository.LobbyId, EOS.LocalProductUserId).ToUniTask();
            }
        }
        #endregion

        #region TargetRPC
        [TargetRpc]
        private void Target_NotYourTurn(NetworkConnection conn)
        {
            _notYourTurnPublisher?.Publish(new NotYourTurnMessage());
        }
        #endregion

        #region ObserversRPC
        [ObserversRpc]
        private void Observers_TurnChanged(bool isHostTurn)
        {
            _updateTurnInfoPublisher?.Publish(new UpdateTurnInfoMessage() {IsPlayersTurn = isHostTurn == IsHostInitialized});
        }

        [ObserversRpc]
        private void Observers_PlayerWin(bool hasHostWon, Dictionary<char, int> winDict)
        {
            _showWinConditionPublisher?.Publish(new ShowWinConditionMessage() {HasPlayerWon = hasHostWon == IsHostInitialized});

            var playerMark = IsHostInitialized ? 'X' : 'O';
            var opponentMark = IsHostInitialized ? 'O' : 'X';
            var playerWinCount = winDict[playerMark];
            var opponentWinCount = winDict[opponentMark];
            
            _updateWinScoresPublisher?.Publish(new UpdateWinScoresMessage()
            {
                PlayerWinCount = playerWinCount, 
                OpponentWinCount = opponentWinCount
            });
        }

        [ObserversRpc]
        private void Observers_NewGameCountdown(int countdown)
        {
            _newGameCountdownPublisher?.Publish(new NewGameCountdownMessage() {Countdown = countdown});
            
            Debug.Log($"[Observer] Observers_NewGameCountdown Countdown: {countdown}");
        }

        [ObserversRpc]
        private void Observers_NewGameStart(char gameStarterMark)
        {
            var isMarkHost = gameStarterMark == 'X';
            _newGameStartPublisher?.Publish(new NewGameStartMessage() {IsPlayerStart = isMarkHost == IsHostInitialized});
            
            Debug.Log($"[Observer] Observers_NewGameStart gameStarterMark: {gameStarterMark}");
        }
        #endregion
    }
}
