using System;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
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

        private NetworkManager _networkManager;
        private FishyEOS _fishyEos;
        private LobbyRepository _lobbyRepository;

        private ISubscriber<AllLobbyPlayersReadyCountdown> _allLobbyPlayersReadyCountdownSubscriber;

        private IDisposable _messageDisposables;
        
        public event Action HostButtonClick;
        public event Action JoinButtonClick;
        
        
        [Inject]
        public void Construct(LobbyRepository lobbyRepository,
            NetworkManager networkManager,
            FishyEOS fishyEos,
            ISubscriber<AllLobbyPlayersReadyCountdown> allLobbyPlayersReadyCountdownSubscriber)
        {
            _networkManager = networkManager;
            _fishyEos = fishyEos;
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
                    _lobbyRepository.AllLobbyPlayersReadyCountdownData = obj;
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
            if (_lobbyRepository.LocalConnectionState is LocalConnectionState.Stopped)
            {
                DrawHostAndJoinUI();
            }
            else if (_lobbyRepository.LocalConnectionState is LocalConnectionState.Starting)
            {
                DrawConnectionStartingUI();
            }
            else if (_lobbyRepository.LocalConnectionState is LocalConnectionState.Stopping)
            {
                DrawConnectionStoppingUI();
            }
            else if (_lobbyRepository.LocalConnectionState is LocalConnectionState.Started)
            {
                DrawConnectionStartedUI();
            }

            if (_lobbyRepository.AllLobbyPlayersReadyCountdownData.Enabled)
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
        
        private void DrawConnectionStartedUI()
        {
            var address = _networkManager.IsHostStarted ? _fishyEos.LocalProductUserId : _fishyEos.RemoteProductUserId;
            
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .1f - _height * .5f, _width, _height));
            if (GUILayout.Button(address, new GUIStyle("label") { fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold }, GUILayout.Width(750), GUILayout.Height(75)))
            {
                GUIUtility.systemCopyBuffer = address;
            }
            GUILayout.EndArea();
        }

        private void DrawAllLobbyPlayersReadyCountdownUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .9f - _height * .5f, _width, _height));
            
            GUILayout.Label($"Starting in {_lobbyRepository.AllLobbyPlayersReadyCountdownData.Countdown}", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75));

            GUILayout.EndArea();
        }
        
        private void OnClientConnectionStateChanged(ClientConnectionStateArgs obj)
        {
            _lobbyRepository.LocalConnectionState = obj.ConnectionState;
        }
    }
}
