using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using FishNet.Plugins.FishyEOS.Util;
using FishNet.Transporting.FishyEOSPlugin;
using RudyAtkinson.EOSLobby.Repository;
using RudyAtkinson.EOSLobby.Yield;
using UnityEngine;

namespace RudyAtkinson.EOSLobby.Service
{
    public class EOSLobbyService
    {
        private readonly FishyEOS _fishyEos;
        private readonly LobbyInterface _lobbyInterface = EOS.GetPlatformInterface().GetLobbyInterface();
        private readonly EOSLobbyRepository _eosLobbyRepository;
        
        private CreateLobbyCallbackInfo? _createLobbyCallbackInfo;
        private UpdateLobbyCallbackInfo? _updateLobbyCallbackInfo;
        private LobbySearchFindCallbackInfo? _lobbySearchFindCallbackInfo;
        private LeaveLobbyCallbackInfo? _leaveLobbyCallbackInfo;
        private JoinLobbyCallbackInfo? _joinLobbyCallbackInfo;

        public EOSLobbyService(EOSLobbyRepository eosLobbyRepository,
            FishyEOS fishyEos)
        {
            _eosLobbyRepository = eosLobbyRepository;
            _fishyEos = fishyEos;
        }

        public IEnumerator CreateLobby(ProductUserId localUserId)
        {
            var createLobbyOptions = new CreateLobbyOptions
            {
                LocalUserId = localUserId,
                MaxLobbyMembers = 2,
                PermissionLevel = LobbyPermissionLevel.Publicadvertised,
                BucketId = "fishnet-p2p-sample"
            };
            
            _lobbyInterface.CreateLobby(ref createLobbyOptions, null,
                (ref CreateLobbyCallbackInfo callbackInfo) =>
                {
                    _createLobbyCallbackInfo = callbackInfo;
                });

            yield return new WaitUntilOrTimeout(() => _createLobbyCallbackInfo.HasValue, 30f,
                () => _createLobbyCallbackInfo = new CreateLobbyCallbackInfo { ResultCode = Result.TimedOut });

            _eosLobbyRepository.LobbyId = _createLobbyCallbackInfo?.LobbyId;
            
            Debug.Log($"[EOSLobby] Result: {_createLobbyCallbackInfo?.ResultCode}, Id: {_createLobbyCallbackInfo?.LobbyId}");
            
            EOS.GetManager().StartCoroutine(UpdateLobbyNameAndIDAttributes());
        }
        
        private IEnumerator UpdateLobbyNameAndIDAttributes()
        {
            var updateLobbyModificationOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = _eosLobbyRepository.LobbyId,
                LocalUserId = EOS.LocalProductUserId
            };
            
            _lobbyInterface.UpdateLobbyModification(ref updateLobbyModificationOptions, out var lobbyModification);
            
            var addLobbyNameAttributeOptions = new LobbyModificationAddAttributeOptions
            {
                Attribute = new AttributeData
                {
                    Key = "LobbyName",
                    Value = new AttributeDataValue { AsUtf8 = $"{PlayerPrefs.GetString("rudyatkinson-player-name")}'s Lobby" }
                },
                Visibility = LobbyAttributeVisibility.Public
            };
            lobbyModification.AddAttribute(ref addLobbyNameAttributeOptions);

            var addConnectionIdAttributeOption = new LobbyModificationAddAttributeOptions()
            {
                Attribute = new AttributeData()
                {
                    Key = "RemoteProductUserId",
                    Value = new AttributeDataValue() { AsUtf8 = _fishyEos.LocalProductUserId }
                },
                Visibility = LobbyAttributeVisibility.Public
            };
            lobbyModification.AddAttribute(ref addConnectionIdAttributeOption);
            
            var updateLobbyOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = lobbyModification,
            };
            
            _lobbyInterface.UpdateLobby(ref updateLobbyOptions, null,
                (ref UpdateLobbyCallbackInfo callbackInfo) =>
                {
                    _updateLobbyCallbackInfo = callbackInfo;
                });

            yield return new WaitUntilOrTimeout(() => _updateLobbyCallbackInfo.HasValue, 30f,
                () => _updateLobbyCallbackInfo = new UpdateLobbyCallbackInfo { ResultCode = Result.TimedOut });
        }

        public IEnumerator UpdateLobbyPermissionLevelAttribute(LobbyPermissionLevel permissionLevel)
        {
            var updateLobbyModificationOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = _eosLobbyRepository.LobbyId,
                LocalUserId = EOS.LocalProductUserId
            };
            
            _lobbyInterface.UpdateLobbyModification(ref updateLobbyModificationOptions, out var lobbyModification);

            var setPermissionLevelAttributeOption = new LobbyModificationSetPermissionLevelOptions()
            {
                PermissionLevel = permissionLevel
            };
            lobbyModification.SetPermissionLevel(ref setPermissionLevelAttributeOption);
            
            var updateLobbyOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = lobbyModification,
            };
            
            _lobbyInterface.UpdateLobby(ref updateLobbyOptions, null,
                (ref UpdateLobbyCallbackInfo callbackInfo) =>
                {
                    _updateLobbyCallbackInfo = callbackInfo;
                });

            yield return new WaitUntilOrTimeout(() => _updateLobbyCallbackInfo.HasValue, 30f,
                () => _updateLobbyCallbackInfo = new UpdateLobbyCallbackInfo { ResultCode = Result.TimedOut });
        }
        
        public IEnumerator JoinLobby(ProductUserId localUserId, LobbyDetails lobbyDetails)
        {
            var joinLobbyOptions = new JoinLobbyOptions
            {
                LobbyDetailsHandle = lobbyDetails,
                LocalUserId = localUserId,
            };
            var lobbyInterface = EOS.GetPlatformInterface().GetLobbyInterface();
            lobbyInterface.JoinLobby(ref joinLobbyOptions, null,
                (ref JoinLobbyCallbackInfo callbackInfo) => { _joinLobbyCallbackInfo = callbackInfo; });

            yield return new WaitUntilOrTimeout(() => _joinLobbyCallbackInfo.HasValue, 30f,
                () => _joinLobbyCallbackInfo = new JoinLobbyCallbackInfo { ResultCode = Result.TimedOut });
            
            _eosLobbyRepository.LobbyId = _joinLobbyCallbackInfo?.LobbyId;
        }
        
        public IEnumerator LeaveLobby(string lobbyId, ProductUserId localUserId)
        {
            var leaveLobbyOptions = new LeaveLobbyOptions
            {
                LobbyId = lobbyId,
                LocalUserId = localUserId,
            };
            
            _lobbyInterface.LeaveLobby(ref leaveLobbyOptions, null,
                (ref LeaveLobbyCallbackInfo callbackInfo) => { _leaveLobbyCallbackInfo = callbackInfo; });

            yield return new WaitUntilOrTimeout(() => _leaveLobbyCallbackInfo.HasValue, 30f,
                () => _leaveLobbyCallbackInfo = new LeaveLobbyCallbackInfo { ResultCode = Result.TimedOut });
        }

        public IEnumerator SearchLobbiesCoroutine()
        {
            while (true)
            {
                var createLobbySearchOptions = new CreateLobbySearchOptions { MaxResults = 10, };
                
                _lobbyInterface.CreateLobbySearch(ref createLobbySearchOptions, out var lobbySearch);
                
                var lobbySearchFindOptions = new LobbySearchFindOptions { LocalUserId = EOS.LocalProductUserId };
                var lobbySearchParameterOptions = new LobbySearchSetParameterOptions
                {
                    ComparisonOp = ComparisonOp.Notequal,
                    Parameter = new AttributeData
                    {
                        Key = "LobbyName",
                        Value = new AttributeDataValue { AsUtf8 = "" },
                    },
                };
                lobbySearch.SetParameter(ref lobbySearchParameterOptions);

                lobbySearch.Find(ref lobbySearchFindOptions, null,
                    (ref LobbySearchFindCallbackInfo data) =>
                    {
                        var getSearchResultCountOptions = new LobbySearchGetSearchResultCountOptions();
                        var numberOfResults = lobbySearch.GetSearchResultCount(ref getSearchResultCountOptions);

                        var lobbyDetails = new LobbyDetails[numberOfResults];
                        for (uint i = 0; i < numberOfResults; i++)
                        {
                            var copySearchResultByIndexOptions = new LobbySearchCopySearchResultByIndexOptions
                                { LobbyIndex = i };
                            var result =
                                lobbySearch.CopySearchResultByIndex(ref copySearchResultByIndexOptions,
                                    out var lobbyDetailsHandle);
                            lobbyDetails[i] = lobbyDetailsHandle;
                        }

                        lobbySearch.Release();

                        var lobbyDetailsDict = new Dictionary<LobbyDetails, LobbyDetailsInfo>();
                        foreach (var lobbyDetail in lobbyDetails)
                        {
                            var lobbyDetailsCopyInfoOptions = new LobbyDetailsCopyInfoOptions();
                            var lobbyDetailsResult = lobbyDetail.CopyInfo(ref lobbyDetailsCopyInfoOptions, out var lobbyDetailsInfo);

                            var getMemberCountOption = new LobbyDetailsGetMemberCountOptions();
                            var lobbyMemberCount = lobbyDetail.GetMemberCount(ref getMemberCountOption);
                
                            if (lobbyDetailsResult != Result.Success || 
                                !lobbyDetailsInfo.HasValue || 
                                lobbyMemberCount != 1)
                            {
                                continue;
                            }
                
                            lobbyDetailsDict.Add(lobbyDetail, lobbyDetailsInfo.Value);
                        }
                        
                        _eosLobbyRepository.LobbyDetails = lobbyDetailsDict;

                        Debug.Log($"[EOSLobby] Lobby Count: {lobbyDetails.Length}");

                        _lobbySearchFindCallbackInfo = data;
                    });

                yield return new WaitUntilOrTimeout(() => _lobbySearchFindCallbackInfo.HasValue, 10f,
                    () => _lobbySearchFindCallbackInfo = new LobbySearchFindCallbackInfo
                        { ResultCode = Result.TimedOut });

                if (_lobbySearchFindCallbackInfo?.ResultCode != Result.Success)
                {
                    lobbySearch.Release();
                }

                yield return new WaitForSeconds(5f);
            }
        }
    }
}