using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Script.Networking
{
    public class NetworkView: MonoBehaviour
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;

        private SignalBus _signalBus;
        
        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        private void OnEnable()
        {
            _hostButton.onClick.AddListener(OnHostButtonClicked);
            _joinButton.onClick.AddListener(OnJoinButtonClicked);
        }

        private void OnDisable()
        {
            _hostButton.onClick.RemoveListener(OnHostButtonClicked);
            _joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        }

        private void OnHostButtonClicked()
        {
            _signalBus.Fire<HostButtonClickedSignal>();
        }

        private void OnJoinButtonClicked()
        {
            _signalBus.Fire<JoinButtonClickedSignal>();
        }
    }
}