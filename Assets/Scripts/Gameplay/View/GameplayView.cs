using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using RudyAtkinson.Gameplay.Message;
using RudyAtkinson.Tile.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace RudyAtkinson.Gameplay.View
{
    public class GameplayView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playerScoreText;
        [SerializeField] private TMP_Text _opponentScoreText;
        [SerializeField] private Transform _informerTextParent;
        [SerializeField] private Button _leaveButton;

        private TMP_Text _informerTextPrefab;

        private IPublisher<LeaveButtonClickMessage> _leaveButtonClickPublisher;
        
        private ISubscriber<NewGameCountdownMessage> _newGameCountdownSubscriber;
        private ISubscriber<NewGameStartMessage> _newGameStartSubscriber;
        private ISubscriber<ShowWinConditionMessage> _showWinConditionSubscriber;
        private ISubscriber<UpdateTurnInfoMessage> _updateTurnInfoSubscriber;
        private ISubscriber<UpdateWinScoresMessage> _updateWinScoresSubscriber;
        private ISubscriber<NotYourTurnMessage> _notYourTurnSubscriber;

        private IDisposable _subscriberDisposables;
        private CancellationTokenSource _newGameCountdownCTS;
        private CancellationTokenSource _newGameStartCTS;
        private CancellationTokenSource _updateTurnInfoCTS;
        private CancellationTokenSource _showWinConditionCTS;
        private CancellationTokenSource _notYourTurnCTS;
        
        [Inject]
        private void Construct(TMP_Text informerTextPrefab,
            ISubscriber<NewGameCountdownMessage> newGameCountdownSubscriber,
            ISubscriber<NewGameStartMessage> newGameStartSubscriber,
            IPublisher<LeaveButtonClickMessage> leaveButtonClickPublisher,
            ISubscriber<ShowWinConditionMessage> showWinConditionSubscriber,
            ISubscriber<UpdateTurnInfoMessage> updateTurnInfoSubscriber,
            ISubscriber<UpdateWinScoresMessage> updateWinScoresSubscriber,
            ISubscriber<NotYourTurnMessage> notYourTurnSubscriber)
        {
            _informerTextPrefab = informerTextPrefab;
            _newGameCountdownSubscriber = newGameCountdownSubscriber;
            _newGameStartSubscriber = newGameStartSubscriber;
            _leaveButtonClickPublisher = leaveButtonClickPublisher;
            _showWinConditionSubscriber = showWinConditionSubscriber;
            _updateTurnInfoSubscriber = updateTurnInfoSubscriber;
            _updateWinScoresSubscriber = updateWinScoresSubscriber;
            _notYourTurnSubscriber = notYourTurnSubscriber;
        }

        private void OnEnable()
        {
            _leaveButton.onClick.AddListener(OnLeaveButtonClick);
            
            var newGameCountdownDisposable = _newGameCountdownSubscriber.Subscribe(OnNewGameCountdown);
            var newGameStartDisposable = _newGameStartSubscriber.Subscribe(OnNewGameStart);
            var showWinConditionDisposable = _showWinConditionSubscriber.Subscribe(OnShowWinCondition);
            var updateTurnInfoDisposable = _updateTurnInfoSubscriber.Subscribe(OnUpdateTurnInfo);
            var updateWinScoresDisposable = _updateWinScoresSubscriber.Subscribe(OnUpdateWinScores);
            var notYourTurnDisposable = _notYourTurnSubscriber.Subscribe(OnNotYourTurn);

            _subscriberDisposables = DisposableBag.Create(
                newGameCountdownDisposable, 
                newGameStartDisposable,
                showWinConditionDisposable,
                updateTurnInfoDisposable,
                updateWinScoresDisposable,
                notYourTurnDisposable);
        }

        private void OnDisable()
        {
            _leaveButton.onClick.RemoveListener(OnLeaveButtonClick);

            _subscriberDisposables?.Dispose();
        }

        private void OnLeaveButtonClick()
        {
            _leaveButtonClickPublisher?.Publish(new LeaveButtonClickMessage());
        }

        private void OnNewGameStart(NewGameStartMessage newGameStartMessage)
        {
            _newGameCountdownCTS?.Cancel();
            _newGameStartCTS?.Cancel();
            
            var text = CreateInformerText();
            
            var starterPlayerText = newGameStartMessage.IsPlayerStart ? "You" : "Opponent";
            text.SetText($"{starterPlayerText} will play the first turn.");
            
            _newGameStartCTS = new CancellationTokenSource();
            _newGameStartCTS.Token.Register(() =>
            {
                Destroy(text.gameObject);
            });

            UniTask.Delay(TimeSpan.FromSeconds(5))
                .WithCancellation(_newGameStartCTS.Token)
                .AsAsyncUnitUniTask().ContinueWith(_ =>
                {
                    _newGameStartCTS?.Dispose();
                    _newGameStartCTS = null;
                    
                    Destroy(text.gameObject);
                });
        }
        
        private void OnNewGameCountdown(NewGameCountdownMessage newGameCountdownMessage)
        {
            _newGameCountdownCTS?.Cancel();
            
            var text = CreateInformerText();
            
            text.SetText($"New Game starting in {newGameCountdownMessage.Countdown}");

            _newGameCountdownCTS = new CancellationTokenSource();
            _newGameCountdownCTS.Token.Register(() =>
            {
                Destroy(text.gameObject);
            });

            UniTask.Delay(TimeSpan.FromSeconds(5))
                .WithCancellation(_newGameCountdownCTS.Token)
                .AsAsyncUnitUniTask()
                .ContinueWith(_ =>
                {
                    _newGameCountdownCTS?.Dispose();
                    _newGameCountdownCTS = null;
                    
                    Destroy(text.gameObject);
                });
        }
        
        private void OnShowWinCondition(ShowWinConditionMessage args)
        {
            _updateTurnInfoCTS?.Cancel();
            _showWinConditionCTS?.Cancel();
            
            var text = CreateInformerText();
            var message = args.HasPlayerWon ? "You Win!" : "You Lose!";
            text.SetText(message);
            
            _showWinConditionCTS = new CancellationTokenSource();
            _showWinConditionCTS.Token.Register(() =>
            {
                Destroy(text.gameObject);
            });
            
            UniTask.Delay(TimeSpan.FromSeconds(5))
                .WithCancellation(_showWinConditionCTS.Token)
                .AsAsyncUnitUniTask()
                .ContinueWith(_ =>
                {
                    _showWinConditionCTS?.Dispose();
                    _showWinConditionCTS = null;
                    
                    Destroy(text.gameObject);
                });
        }

        private void OnUpdateTurnInfo(UpdateTurnInfoMessage args)
        {
            _newGameStartCTS?.Cancel();
            _updateTurnInfoCTS?.Cancel();
            
            var text = CreateInformerText();
            var message = args.IsPlayersTurn ? "Your Turn" : "Opponent's Turn";
            text.SetText(message);
            
            _updateTurnInfoCTS = new CancellationTokenSource();
            _updateTurnInfoCTS.Token.Register(() =>
            {
                Destroy(text.gameObject);
            });
            
            UniTask.Delay(TimeSpan.FromSeconds(5))
                .WithCancellation(_updateTurnInfoCTS.Token)
                .AsAsyncUnitUniTask()
                .ContinueWith(_ =>
                {
                    _updateTurnInfoCTS?.Dispose();
                    _updateTurnInfoCTS = null;
                    
                    Destroy(text.gameObject);
                });
        }

        private void OnUpdateWinScores(UpdateWinScoresMessage args)
        {
            _playerScoreText.SetText(args.PlayerWinCount.ToString());
            _opponentScoreText.SetText(args.OpponentWinCount.ToString());
        }

        private void OnNotYourTurn(NotYourTurnMessage args)
        {
            _notYourTurnCTS?.Cancel();
            
            var text = CreateInformerText();
            text.SetText($"Not your turn.");
            
            _notYourTurnCTS = new CancellationTokenSource();
            _notYourTurnCTS.Token.Register(() =>
            {
                Destroy(text.gameObject);
            });

            UniTask.Delay(TimeSpan.FromSeconds(5))
                .WithCancellation(_notYourTurnCTS.Token)
                .AsAsyncUnitUniTask()
                .ContinueWith(_ =>
                {
                    _notYourTurnCTS?.Dispose();
                    _notYourTurnCTS = null;
                    
                    Destroy(text.gameObject);
                });
        }

        private TMP_Text CreateInformerText()
        {
            var text = Instantiate(_informerTextPrefab, _informerTextParent);
            text.transform.localScale = Vector3.one;

            return text;
        }

        private void InitTextTask(CancellationTokenSource cts, TMP_Text text, int delaySeconds = 5)
        {
            UniTask.Delay(TimeSpan.FromSeconds(delaySeconds))
                .WithCancellation(cts.Token)
                .AsAsyncUnitUniTask()
                .ContinueWith(_ =>
                {
                    cts?.Dispose();
                    cts = null;
                    
                    Destroy(text.gameObject);
                });
        }
    }
}
