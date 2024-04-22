using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using FishNet.Plugins.FishyEOS.Util;
using RudyAtkinson.EOSLobby.Repository;
using RudyAtkinson.EOSLobby.Support;
using UnityEngine;

namespace RudyAtkinson.EOSLobby.Service
{
    public class EOSLobbyService
    {
        private readonly LobbyInterface _lobbyInterface = EOS.GetPlatformInterface().GetLobbyInterface();
        private readonly EOSLobbyRepository _eosLobbyRepository;
        
        private CreateLobbyCallbackInfo? _createLobbyCallbackInfo;
        private UpdateLobbyCallbackInfo? _updateLobbyCallbackInfo;
        private LobbySearchFindCallbackInfo? _lobbySearchFindCallbackInfo;
        private LeaveLobbyCallbackInfo? _leaveLobbyCallbackInfo;

        public EOSLobbyService(EOSLobbyRepository eosLobbyRepository)
        {
            _eosLobbyRepository = eosLobbyRepository;
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
            
            EOS.GetManager().StartCoroutine(UpdateLobbyAttributes(_createLobbyCallbackInfo?.LobbyId, "LobbyName", "Lobby's Name temp now."));
        }
        
        public IEnumerator UpdateLobbyAttributes(string lobbyId, Utf8String attributeKey, Utf8String attributeValue)
        {
            var updateLobbyModificationOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = lobbyId,
                LocalUserId = EOS.LocalProductUserId
            };
            
            _lobbyInterface.UpdateLobbyModification(ref updateLobbyModificationOptions, out var lobbyModification);
            
            var addAttributeOptions = new LobbyModificationAddAttributeOptions
            {
                Attribute = new AttributeData
                {
                    Key = attributeKey,
                    Value = new AttributeDataValue { AsUtf8 = attributeValue },
                },
                Visibility = LobbyAttributeVisibility.Public
            };
            
            lobbyModification.AddAttribute(ref addAttributeOptions);
            
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
            
            Debug.Log($"[EOSLobby] Result: {_updateLobbyCallbackInfo?.ResultCode}, Id: {_updateLobbyCallbackInfo?.LobbyId}");

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

                        _eosLobbyRepository.LobbyDetails = lobbyDetails;

                        Debug.Log($"[EOSLobby] Lobby Count: {lobbyDetails.Length}");
                        foreach (var lobbyDetail in lobbyDetails)
                        {
                            var lobbyCopyAttributeByKeyOptions = new LobbyDetailsCopyAttributeByKeyOptions
                                { AttrKey = "LobbyName" };
                            lobbyDetail.CopyAttributeByKey(ref lobbyCopyAttributeByKeyOptions, out var lobbyAttribute);

                            if (!lobbyAttribute.HasValue)
                            {
                                continue;
                            }

                            Debug.Log($"[EOSLobby] LobbyName Attribute: {lobbyAttribute.Value.Data?.Value.AsUtf8}");
                        }

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