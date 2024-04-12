using System;
using FishNet.Managing;
using FishNet.Transporting;
using MessagePipe;
using RudyAtkinson.Lobby.Model;
using RudyAtkinson.Lobby.Repository;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Lobby.View
{
    public class LobbyView : MonoBehaviour
    {
        private const int _width = 750;
        private const int _height = 225;

        private AllLobbyPlayersReadyCountdown _allPlayersReady = new (false);
        private LocalConnectionState _localConnectionState;
        
        private NetworkManager _networkManager;
        private LobbyRepository _lobbyRepository;

        private ISubscriber<AllLobbyPlayersReadyCountdown> _allLobbyPlayersReadyCountdownSubscriber;

        private IDisposable _messageDisposables;
        
        public event Action HostButtonClick;
        public event Action JoinButtonClick;
        
        
        [Inject]
        public void Construct(LobbyRepository lobbyRepository,
            NetworkManager networkManager,
            ISubscriber<AllLobbyPlayersReadyCountdown> allLobbyPlayersReadyCountdownSubscriber)
        {
            _networkManager = networkManager;
            _lobbyRepository = lobbyRepository;

            _allLobbyPlayersReadyCountdownSubscriber = allLobbyPlayersReadyCountdownSubscriber;
            
        }

        private void Awake()
        {
            PlayerPrefs.SetString("rudyatkinson-player-name", "Player");
        }

        private void OnEnable()
        {
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;

            var allLobbyPlayersReadyCountdownDisposable = _allLobbyPlayersReadyCountdownSubscriber.Subscribe(
                obj =>
                {
                    _allPlayersReady = obj;
                });
            
            _messageDisposables = DisposableBag.Create(allLobbyPlayersReadyCountdownDisposable);
        }

        private void OnDisable()
        {
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;
            
            _messageDisposables?.Dispose();
        }

        private void OnGUI()
        {
            if (_localConnectionState is LocalConnectionState.Stopped)
            {
                DrawHostAndJoinUI();
            }
            else if (_localConnectionState is LocalConnectionState.Starting)
            {
                DrawConnectionStartingUI();
            }
            else if (_localConnectionState is LocalConnectionState.Stopping)
            {
                DrawConnectionStoppingUI();
            }

            if (_allPlayersReady.Enabled)
            {
                DrawAllLobbyPlayersReadyCountdownUI();
            }
        }

        private void DrawHostAndJoinUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .5f - _height * .5f, _width, _height));
            
            var playerName = GUILayout.TextField(PlayerPrefs.GetString("rudyatkinson-player-name"), new GUIStyle("textfield"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75))
                .Trim()
                .Replace(" ", string.Empty);

            if (playerName.Length > 18)
            {
                playerName = playerName[..18];
            }

            PlayerPrefs.SetString("rudyatkinson-player-name", playerName);

            if (GUILayout.Button("HOST", new GUIStyle("button"){fontSize = 42},GUILayout.Width(750), GUILayout.Height(75)))
            {
                HostButtonClick?.Invoke();
            }

            GUILayout.BeginHorizontal();
            
            _lobbyRepository.Address = GUILayout.TextField(_lobbyRepository.Address, new GUIStyle("textfield"){fontSize = 42, alignment = TextAnchor.MiddleLeft}, GUILayout.Width(550), GUILayout.Height(75));

            if (GUILayout.Button("JOIN", new GUIStyle("button"){fontSize = 42}, GUILayout.Width(200), GUILayout.Height(75)))
            {
                JoinButtonClick?.Invoke();
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawConnectionStartingUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .5f - _height * .5f, _width, _height));
            GUILayout.Label("Connecting...", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75));
            GUILayout.EndArea();
        }

        private void DrawConnectionStoppingUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .5f - _height * .5f, _width, _height));
            GUILayout.Label("Leaving...", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75));
            GUILayout.EndArea();
        }

        private void DrawAllLobbyPlayersReadyCountdownUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .9f - _height * .5f, _width, _height));
            
            GUILayout.Label($"Starting in {_allPlayersReady.Countdown}", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75));

            GUILayout.EndArea();
        }
        
        private void OnClientConnectionStateChanged(ClientConnectionStateArgs obj)
        {
            _localConnectionState = obj.ConnectionState;
        }
    }
}
