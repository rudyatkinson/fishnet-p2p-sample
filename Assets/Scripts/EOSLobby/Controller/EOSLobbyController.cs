using System;
using System.Collections;
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
        private readonly FishyEOS _fishyEos; 
        private readonly LobbyRepository _lobbyRepository;
        private readonly EOSLobbyService _eosLobbyService;
        private readonly EOSLobbyRepository _eosLobbyRepository;
        private readonly LobbyView _lobbyView;
        
        public EOSLobbyController(
            FishyEOS fishyEos,
            LobbyRepository lobbyRepository,
            EOSLobbyService eosLobbyService,
            EOSLobbyRepository eosLobbyRepository,
            LobbyView lobbyView)
        {
            _fishyEos = fishyEos;
            _lobbyRepository = lobbyRepository;
            _eosLobbyService = eosLobbyService;
            _eosLobbyRepository = eosLobbyRepository;
            _lobbyView = lobbyView;
            
            _fishyEos.OnClientConnectionState += OnClientConnectionState;
            _lobbyView.LoginEOSTryAgainButtonClick += OnLoginEOSTryAgainButtonClick;
        }

        public void Dispose()
        {
            _fishyEos.OnClientConnectionState -= OnClientConnectionState;
            _lobbyView.LoginEOSTryAgainButtonClick -= OnLoginEOSTryAgainButtonClick;
        }
        
        public void Start()
        {
            _fishyEos.StartCoroutine(EOSLoginCoroutine());
        }
        
        private void OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            if (obj.ConnectionState == LocalConnectionState.Started)
            {
                _fishyEos.StartCoroutine(_eosLobbyService.CreateLobby(EOS.LocalProductUserId));
            }
            else if (obj.ConnectionState == LocalConnectionState.Stopped && Application.isPlaying)
            {
                _fishyEos.StartCoroutine(_eosLobbyService.LeaveLobby(_eosLobbyRepository.LobbyId, EOS.LocalProductUserId));
            }
        }

        private void OnLoginEOSTryAgainButtonClick()
        {
            _fishyEos.StartCoroutine(EOSLoginCoroutine());
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
    }
}