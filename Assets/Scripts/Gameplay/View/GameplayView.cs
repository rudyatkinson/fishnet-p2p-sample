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
        [SerializeField] private Transform _informerTextParent;
        [SerializeField] private Button _leaveButton;

        private TMP_Text _informerTextPrefab;

        private IPublisher<LeaveButtonClickMessage> _leaveButtonClickPublisher;
        
        private ISubscriber<NewGameCountdown> _newGameCountdownSubscriber;
        private ISubscriber<NewGameStart> _newGameStartSubscriber;

        private IDisposable _subscriberDisposables;
        private CancellationTokenSource _newGameCountdownCancellationToken;
        
        [Inject]
        private void Construct(TMP_Text informerTextPrefab,
            ISubscriber<NewGameCountdown> newGameCountdownSubscriber,
            ISubscriber<NewGameStart> newGameStartSubscriber,
            IPublisher<LeaveButtonClickMessage> leaveButtonClickPublisher)
        {
            _informerTextPrefab = informerTextPrefab;
            _newGameCountdownSubscriber = newGameCountdownSubscriber;
            _newGameStartSubscriber = newGameStartSubscriber;
            _leaveButtonClickPublisher = leaveButtonClickPublisher;
        }

        private void OnEnable()
        {
            _leaveButton.onClick.AddListener(OnLeaveButtonClick);
            
            var newGameCountdownDisposable = _newGameCountdownSubscriber.Subscribe(OnNewGameCountdown);
            var newGameStartDisposable = _newGameStartSubscriber.Subscribe(OnNewGameStart);

            _subscriberDisposables = DisposableBag.Create(newGameCountdownDisposable, newGameStartDisposable);
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

        private void OnNewGameStart(NewGameStart newGameStart)
        {
            _newGameCountdownCancellationToken?.Cancel();
            
            var text = CreateInformerText();
            
            var starterPlayerText = newGameStart.isStarterPlayer ? "You" : "Opponent";
            text.SetText($"{starterPlayerText} will play the first turn.");

            UniTask.Delay(TimeSpan.FromSeconds(5)).AsAsyncUnitUniTask().ContinueWith(_ =>
            {
                Destroy(text.gameObject);
            });
        }
        
        private void OnNewGameCountdown(NewGameCountdown newGameCountdown)
        {
            _newGameCountdownCancellationToken?.Cancel();
            
            var text = CreateInformerText();
            
            text.SetText($"New Game starting in {newGameCountdown.Countdown}");

            _newGameCountdownCancellationToken = new CancellationTokenSource();
            _newGameCountdownCancellationToken.Token.Register(() =>
            {
                Destroy(text.gameObject);
            });

            UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: new CancellationToken())
                .WithCancellation(_newGameCountdownCancellationToken.Token)
                .AsAsyncUnitUniTask()
                .ContinueWith(_ =>
                {
                    Destroy(text.gameObject);
                });
        }

        private TMP_Text CreateInformerText()
        {
            var text = Instantiate(_informerTextPrefab, _informerTextParent);
            text.transform.localScale = Vector3.one;

            return text;
        }
    }
}
