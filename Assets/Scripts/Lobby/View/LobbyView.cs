using System;
using System.Collections;
using Epic.OnlineServices;
using FishNet.Managing;
using FishNet.Plugins.FishyEOS.Util;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using MessagePipe;
using RudyAtkinson.Lobby.Message;
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

        private ISubscriber<AllLobbyPlayersReadyCountdownMessage> _allLobbyPlayersReadyCountdownSubscriber;

        private IDisposable _messageDisposables;
        
        public event Action HostButtonClick;
        public event Action JoinButtonClick;
        
        
        [Inject]
        public void Construct(LobbyRepository lobbyRepository,
            NetworkManager networkManager,
            FishyEOS fishyEos,
            ISubscriber<AllLobbyPlayersReadyCountdownMessage> allLobbyPlayersReadyCountdownSubscriber)
        {
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _lobbyRepository = lobbyRepository;

            _allLobbyPlayersReadyCountdownSubscriber = allLobbyPlayersReadyCountdownSubscriber;
        }

        private void Awake()
        {
            if (!PlayerPrefs.HasKey("rudyatkinson-player-name"))
            {
                PlayerPrefs.SetString("rudyatkinson-player-name", "Player");
            }

            StartCoroutine(EOSLoginCoroutine());
            
        }

        private void OnEnable()
        {
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;

            var allLobbyPlayersReadyCountdownDisposable = _allLobbyPlayersReadyCountdownSubscriber.Subscribe(
                obj =>
                {
                    _lobbyRepository.AllLobbyPlayersReadyCountdownMessageData = obj;
                });
            
            _messageDisposables = DisposableBag.Create(allLobbyPlayersReadyCountdownDisposable);
        }

        private void OnDisable()
        {
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionStateChanged;
            
            _messageDisposables?.Dispose();
        }
        
        private IEnumerator EOSLoginCoroutine()
        {
            yield return new AuthData().Connect(out var authDataLogin);
            
            var nullableLoginCallback = authDataLogin.loginCallbackInfo;
            if (!nullableLoginCallback.HasValue)
            {
                _lobbyRepository.TriedToLoginEOSAtInitialTime = true;

                yield break;
            }
            
            var loginCallback = nullableLoginCallback.Value;
            
            Debug.Log($"EOS Login Result: {loginCallback.ResultCode}");

            _lobbyRepository.EOSLoginResult = loginCallback.ResultCode;

            _lobbyRepository.TriedToLoginEOSAtInitialTime = true;
        }

        private void OnGUI()
        {
            if (!_lobbyRepository.TriedToLoginEOSAtInitialTime)
            {
                DrawLoggingInEOSUI();
                return;
            }
            
            if (_lobbyRepository.EOSLoginResult != Result.Success)
            {
                DrawLoginErrorUI();
                return;
            }
            
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

            if (_lobbyRepository.AllLobbyPlayersReadyCountdownMessageData.Enabled)
            {
                DrawAllLobbyPlayersReadyCountdownUI();
            }
        }

        private void DrawLoggingInEOSUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .5f - _height * .5f, _width, _height));
            GUILayout.Label("Logging in EOS...", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75));
            GUILayout.EndArea();
        }
        
        private void DrawLoginErrorUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .5f - _height * .5f, _width, _height));
            GUILayout.Label($"Encountered an error during login, {_lobbyRepository.EOSLoginResult}", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75));
            GUILayout.Space(50);
            if (GUILayout.Button("Try Again", new GUIStyle("button") { fontSize = 42 }, GUILayout.Width(750), GUILayout.Height(75)))
            {
                _lobbyRepository.TriedToLoginEOSAtInitialTime = false;
                StartCoroutine(EOSLoginCoroutine());
            }
            GUILayout.EndArea();
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
            
            GUILayout.Label($"Starting in {_lobbyRepository.AllLobbyPlayersReadyCountdownMessageData.Countdown}", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold}, GUILayout.Width(750), GUILayout.Height(75));

            GUILayout.EndArea();
        }
        
        private void OnClientConnectionStateChanged(ClientConnectionStateArgs obj)
        {
            _lobbyRepository.LocalConnectionState = obj.ConnectionState;
        }
    }
}
