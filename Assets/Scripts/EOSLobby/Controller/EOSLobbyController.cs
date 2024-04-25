using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using FishNet.Managing;
using FishNet.Plugins.FishyEOS.Util;
using FishNet.Transporting;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.EOSLobby.Repository;
using RudyAtkinson.EOSLobby.Service;
using RudyAtkinson.Lobby.Repository;
using RudyAtkinson.Lobby.View;
using UnityEngine;
using VContainer.Unity;

namespace RudyAtkinson.EOSLobby.Controller
{
    public class EOSLobbyController: IStartable, IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly FishyEOS _fishyEos; 
        private readonly EOSLobbyService _eosLobbyService;
        private readonly EOSLobbyRepository _eosLobbyRepository;
        private readonly LobbyView _lobbyView;
        
        public EOSLobbyController(
            NetworkManager networkManager,
            FishyEOS fishyEos,
            EOSLobbyService eosLobbyService,
            EOSLobbyRepository eosLobbyRepository,
            LobbyView lobbyView)
        {
            _networkManager = networkManager;
            _fishyEos = fishyEos;
            _eosLobbyService = eosLobbyService;
            _eosLobbyRepository = eosLobbyRepository;
            _lobbyView = lobbyView;
            
            _fishyEos.OnClientConnectionState += OnClientConnectionState;
            _fishyEos.OnServerConnectionState += OnServerConnectionState;
            _lobbyView.LoginEOSTryAgainButtonClick += OnLoginEOSTryAgainButtonClick;
            _lobbyView.JoinToLobbyButtonClick += OnJoinToLobbyButtonClick;
            _lobbyView.JoinButtonClick += OnJoinButtonClick;
        }

        public void Dispose()
        {
            _fishyEos.OnClientConnectionState -= OnClientConnectionState;
            _fishyEos.OnServerConnectionState -= OnServerConnectionState;
            _lobbyView.LoginEOSTryAgainButtonClick -= OnLoginEOSTryAgainButtonClick;
            _lobbyView.JoinToLobbyButtonClick -= OnJoinToLobbyButtonClick;
            _lobbyView.JoinButtonClick -= OnJoinButtonClick;
        }
        
        public void Start()
        {
            _fishyEos.StartCoroutine(EOSLoginCoroutine());
        }
        
        private void OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            if (obj.ConnectionState == LocalConnectionState.Started)
            {
                _eosLobbyRepository.TriedToJoinLobbyViaServerBrowser = false;
                _eosLobbyRepository.IsServerBrowserActive = false;
            }
            else if (obj.ConnectionState == LocalConnectionState.Stopped && Application.isPlaying)
            {
                _fishyEos.StartCoroutine(_eosLobbyService.LeaveLobby(_eosLobbyRepository.LobbyId, EOS.LocalProductUserId));
            }
        }

        private void OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            if (obj.ConnectionState == LocalConnectionState.Started)
            {
                var guid = Guid.NewGuid();
                var encodedLocalProductUserId = Encoding.ASCII.GetBytes(guid.ToString());
                var convertedLocalProductUserId = Convert.ToBase64String(encodedLocalProductUserId);
                
                var regex = new Regex("[^a-zA-Z0-9 -]");
                var replacedLocalProductUserId = regex.Replace(convertedLocalProductUserId, "");

                var shortLobbyCode = replacedLocalProductUserId[..5];
                
                _fishyEos.StartCoroutine(_eosLobbyService.CreateLobby(shortLobbyCode, EOS.LocalProductUserId));
            }
        }

        private void OnJoinButtonClick()
        {
            _fishyEos.StartCoroutine(JoinLobbyById());
        }
        
        private IEnumerator JoinLobbyById()
        {
            if (string.IsNullOrEmpty(_eosLobbyRepository.LobbyId))
            {
                yield break;
            }

            yield return _fishyEos.StartCoroutine(_eosLobbyService.JoinLobbyById(EOS.LocalProductUserId, _eosLobbyRepository.LobbyId));

            var copyLobbyDetailsHandleOptions = new CopyLobbyDetailsHandleOptions() { LobbyId = _eosLobbyRepository.LobbyId, LocalUserId = EOS.LocalProductUserId }; 
            var copyLobbyDetailsHandleResult = EOS.GetPlatformInterface().GetLobbyInterface().CopyLobbyDetailsHandle(ref copyLobbyDetailsHandleOptions, out var lobbyDetails);

            yield return copyLobbyDetailsHandleResult;
            
            if (copyLobbyDetailsHandleResult != Result.Success)
            {
                lobbyDetails.Release();
                yield break;
            }
            
            var lobbyCopyAttributeByKeyOptions = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = "RemoteProductUserId" };
            var hostIdAttributeResult = lobbyDetails.CopyAttributeByKey(ref lobbyCopyAttributeByKeyOptions, out var hostIdAttribute);

            yield return hostIdAttributeResult;
            
            if (hostIdAttributeResult != Result.Success)
            {
                lobbyDetails.Release();
                yield break;
            }
            
            var lobbyId = hostIdAttribute?.Data?.Value.AsUtf8;
            
            _fishyEos.RemoteProductUserId = lobbyId;
            _networkManager.ClientManager.StartConnection();
            
            lobbyDetails.Release();
        }

        private void OnLoginEOSTryAgainButtonClick()
        {
            _fishyEos.StartCoroutine(EOSLoginCoroutine());
        }

        private void OnJoinToLobbyButtonClick(LobbyDetails lobbyDetails)
        {
            _fishyEos.StartCoroutine(StartEOSLobbyJoin(lobbyDetails));
        }

        private IEnumerator StartEOSLobbyJoin(LobbyDetails lobbyDetails)
        {
            _eosLobbyRepository.IsServerBrowserActive = false;
            _eosLobbyRepository.TriedToJoinLobbyViaServerBrowser = true;
            
            yield return _fishyEos.StartCoroutine(_eosLobbyService.JoinLobby(EOS.LocalProductUserId, lobbyDetails));

            var lobbyCopyAttributeByKeyOptions = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = "RemoteProductUserId" };
            var hostIdAttributeResult = lobbyDetails.CopyAttributeByKey(ref lobbyCopyAttributeByKeyOptions, out var hostIdAttribute);

            if (hostIdAttributeResult != Result.Success)
            {
                _eosLobbyRepository.IsServerBrowserActive = true;
                _eosLobbyRepository.TriedToJoinLobbyViaServerBrowser = false;

                yield break;
            }
            
            var lobbyId = hostIdAttribute?.Data?.Value.AsUtf8;
            
            _fishyEos.RemoteProductUserId = lobbyId;
            _networkManager.ClientManager.StartConnection();
        }
        
        private IEnumerator EOSLoginCoroutine()
        {
            yield return new AuthData().Connect(out var authDataLogin);
            
            var nullableLoginCallback = authDataLogin.loginCallbackInfo;
            if (!nullableLoginCallback.HasValue)
            {
                _eosLobbyRepository.TriedToLoginEOSAtInitialTime = true;

                yield break;
            }
            
            var loginCallback = nullableLoginCallback.Value;
            
            Debug.Log($"EOS Login Result: {loginCallback.ResultCode}");

            _eosLobbyRepository.EOSLoginResult = loginCallback.ResultCode;

            _eosLobbyRepository.TriedToLoginEOSAtInitialTime = true;
        }
    }
}