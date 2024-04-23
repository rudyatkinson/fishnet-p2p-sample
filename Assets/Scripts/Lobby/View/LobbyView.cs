using System;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using MessagePipe;
using RudyAtkinson.EOSLobby.Repository;
using RudyAtkinson.Lobby.Message;
using RudyAtkinson.Lobby.Repository;
using UnityEngine;
using VContainer;

namespace RudyAtkinson.Lobby.View
{
    public class LobbyView : MonoBehaviour
    {
        private const int _width = 750;
        private const int _height = 300;
        private const int _heightForEachElementAtServerBrowser = 75;

        private NetworkManager _networkManager;
        private FishyEOS _fishyEos;
        private LobbyRepository _lobbyRepository;
        private EOSLobbyRepository _eosLobbyRepository;

        private ISubscriber<AllLobbyPlayersReadyCountdownMessage> _allLobbyPlayersReadyCountdownSubscriber;

        private IDisposable _messageDisposables;
        
        public event Action HostButtonClick;
        public event Action JoinButtonClick;
        public event Action ServerBrowserButtonClick;
        public event Action CloseServerBrowserButtonClick;
        public event Action LoginEOSTryAgainButtonClick;
        public event Action<LobbyDetails> JoinToLobbyButtonClick;
        
        
        [Inject]
        public void Construct(LobbyRepository lobbyRepository,
            NetworkManager networkManager,
            FishyEOS fishyEos,
            ISubscriber<AllLobbyPlayersReadyCountdownMessage> allLobbyPlayersReadyCountdownSubscriber,
            EOSLobbyRepository eosLobbyRepository)
        {
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _lobbyRepository = lobbyRepository;
            _eosLobbyRepository = eosLobbyRepository;

            _allLobbyPlayersReadyCountdownSubscriber = allLobbyPlayersReadyCountdownSubscriber;
        }

        private void Awake()
        {
            if (!PlayerPrefs.HasKey("rudyatkinson-player-name"))
            {
                PlayerPrefs.SetString("rudyatkinson-player-name", "Player");
            }
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

            if (_lobbyRepository.IsServerBrowserActive)
            {
                DrawServerBrowserUI();
                return;
            }

            if (_lobbyRepository.TriedToJoinLobbyViaServerBrowser)
            {
                DrawConnectionStartingUI();
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
                LoginEOSTryAgainButtonClick?.Invoke();
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
            
            if (GUILayout.Button("SERVER BROWSER", new GUIStyle("button"){fontSize = 42},GUILayout.Width(750), GUILayout.Height(75)))
            {
                ServerBrowserButtonClick?.Invoke();
            }
            
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
        
        private void DrawServerBrowserUI()
        {
            var lobbyDetails = _eosLobbyRepository.LobbyDetails;
            var areaHeight = (lobbyDetails.Count + 2) * _heightForEachElementAtServerBrowser;
            GUILayout.BeginArea(new Rect(Screen.width * .5f - _width * .5f, Screen.height * .5f - areaHeight * .5f, _width, areaHeight));
            GUILayout.BeginHorizontal();
            
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal(GUILayout.MaxHeight(75f));

            if (lobbyDetails.Any())
            {
                GUILayout.Label("Lobby Name", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold}, GUILayout.Width(475), GUILayout.Height(75));
                GUILayout.Label("Players", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold}, GUILayout.Width(275), GUILayout.Height(75));
            }
            else
            {
                GUILayout.Label("Cannot find any lobby, searching...", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold}, GUILayout.Width(_width), GUILayout.Height(75));
            }
            GUILayout.EndHorizontal();

            foreach (var lobbyDetail in lobbyDetails)
            {
                var option = new LobbyDetailsCopyAttributeByKeyOptions() { AttrKey = "LobbyName" };
                var lobbyNameAttribute = lobbyDetail.CopyAttributeByKey(ref option, out var lobbyNameResult);

                var getMemberCountOption = new LobbyDetailsGetMemberCountOptions();
                var lobbyMemberCount = lobbyDetail.GetMemberCount(ref getMemberCountOption);
                
                if (lobbyNameAttribute != Result.Success)
                {
                    Debug.Log($"[EOSLobby] DrawServerBrowserUI failed a lobby detail");
                    continue;
                }
                GUILayout.BeginHorizontal(GUILayout.MaxHeight(75f));
                GUILayout.Label(lobbyNameResult?.Data?.Value.AsUtf8, new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Normal}, GUILayout.Width(475), GUILayout.Height(75));
                GUILayout.Label($"{lobbyMemberCount}/2", new GUIStyle("label"){fontSize = 42, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Normal}, GUILayout.Width(75), GUILayout.Height(75));
                
                if (GUILayout.Button("JOIN", new GUIStyle("button"){fontSize = 42}, GUILayout.Width(200), GUILayout.Height(75)))
                {
                    JoinToLobbyButtonClick?.Invoke(lobbyDetail);
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("BACK", new GUIStyle("button"){fontSize = 42}, GUILayout.Width(_width), GUILayout.Height(75)))
            {
                CloseServerBrowserButtonClick?.Invoke();
            }
            GUILayout.EndVertical();
            
            GUILayout.EndArea();
        }
        
        private void OnClientConnectionStateChanged(ClientConnectionStateArgs obj)
        {
            _lobbyRepository.LocalConnectionState = obj.ConnectionState;
        }
    }
}
