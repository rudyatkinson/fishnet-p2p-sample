using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using FishNet.Connection;
using FishNet.Object;
using MessagePipe;
using RudyAtkinson.Gameplay.Repository;
using RudyAtkinson.Tile.Model;
using RudyAtkinson.Tile.Repository;
using RudyAtkinson.Tile.View;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Tile.Controller
{
    public class TileNetworkController : NetworkBehaviour
    {
        private TileRepository _tileRepository;
        private GameplayRepository _gameplayRepository;
        
        private IPublisher<NewGameCountdown> _newGameCountdownPublisher;
        private IPublisher<NewGameStart> _newGameStartPublisher;

        private ISubscriber<TileClick> _tileClickSubscriber;

        private IDisposable _subscriberDisposables;
        
        [Inject]
        private void Construct(ISubscriber<TileClick> tileClickSubscriber,
            TileRepository tileRepository,
            GameplayRepository gameplayRepository,
            IPublisher<NewGameCountdown> newGameCountdownPublisher,
            IPublisher<NewGameStart> newGameStartPublisher)
        {
            _tileClickSubscriber = tileClickSubscriber;
            _tileRepository = tileRepository;
            _gameplayRepository = gameplayRepository;
            _newGameCountdownPublisher = newGameCountdownPublisher;
            _newGameStartPublisher = newGameStartPublisher;
        }

        private void OnEnable()
        {
            var tileClickDisposable = _tileClickSubscriber.Subscribe(tileClick =>
            {
                Server_OnTileClick(tileClick);
            });

            _subscriberDisposables = DisposableBag.Create(tileClickDisposable);
        }

        private void OnDisable()
        {
            _subscriberDisposables?.Dispose();
        }

        #region Server Logic
        [ServerRpc(RequireOwnership = false)]
        private void Server_OnTileClick(TileClick tileClick, NetworkConnection conn = null)
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

            var tile = tileClick.Tile;
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
            tile.Color.Value = Color.white;
            
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

            if (markQueue.Count >= 3)
            {
                var color = Color.white;
                color.a = .25f;
                markQueue.First().Color.Value = color;
            }

            var nextTurnMark = isHost ? opponentMark : hostMark;
            
            _gameplayRepository.SetMarkTurn(nextTurnMark);
            
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
            }
            else
            {
                Observers_TurnChanged(nextTurnMark);
            }
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
            _tileRepository.TileMarkQueueDictionary.Clear();
            
            foreach (var tileView in _tileRepository.TileViews)
            {
                var tileModel = tileView.TileModel.Value;
                tileModel.Mark = ' ';

                tileView.TileModel.Value = tileModel;
            }
        }

        private void LockPlayerInput(bool isLocked)
        {
            Debug.Log($"[Server] Tiles locked: {isLocked}");
            _gameplayRepository.SetPlayerInputLocked(isLocked);
        }
        #endregion

        #region TargetRPC
        [TargetRpc]
        private void Target_NotYourTurn(NetworkConnection conn)
        {
            // TODO: Inform the player
            Debug.Log("[Target] Not my turn yet!");
        }
        #endregion

        #region ObserversRPC
        [ObserversRpc]
        private void Observers_TurnChanged(char turnMark)
        {
            // TODO: Inform the player
            Debug.Log($"[Observer] Turn changed: {turnMark}");
        }

        [ObserversRpc]
        private void Observers_PlayerWin(bool isHostWin, Dictionary<char, int> winDict)
        {
            var playerMark = isHostWin ? 'X' : 'O';
            Debug.Log($"[Observer] Player {playerMark} Won!");
            
            foreach (var kvp in winDict)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }

        [ObserversRpc]
        private void Observers_NewGameCountdown(int countdown)
        {
            Debug.Log($"[Observer] Observers_NewGameCountdown Countdown: {countdown}");
            
            _newGameCountdownPublisher?.Publish(new NewGameCountdown() {Countdown = countdown});
        }

        [ObserversRpc]
        private void Observers_NewGameStart(char gameStarterMark)
        {
            Debug.Log($"[Observer] Observers_NewGameStart gameStarterMark: {gameStarterMark}");
            
            _newGameStartPublisher?.Publish(new NewGameStart() {GameStarterMark = gameStarterMark});
        }
        #endregion
    }
}
