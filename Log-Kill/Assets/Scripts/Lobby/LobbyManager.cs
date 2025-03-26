using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace LogKill.LobbySystem
{
    public class LobbyManager : MonoSingleton<LobbyManager>
    {
        private RelayManager RelayManager => ServiceLocator.Get<RelayManager>();

        private CancellationTokenSource _lobbyHeartbeatToken;
        private CancellationTokenSource _lobbyRefreshInfomationToken;

        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        public Lobby CurrentLobby { get; private set; }

        public event Action<List<Lobby>> LobbyListChangedEvent;
        public event Action<Lobby> JoinLobbyEvent;
        public event Action LeaveLobbyEvent;
        public event Action KickedFromLobbyEvent;
        public event Action GameStartEvent;

        public async UniTask InitializeAsync()
        {
            await UnityServicesInitializeAsync();
            await SignInAnonymouslyAsync();
        }

        public async UniTask CreateLobbyAsync(string lobbyName, int maxPlayers, int imposterCount, bool isPrivate = false)
        {
            try
            {
                var lobbyOptions = new CreateLobbyOptions
                {
                    Player = GetPlayer(),
                    IsPrivate = isPrivate,
                    Data = new Dictionary<string, DataObject>
                    {
                        { NetworkConstants.GAMESTART_KEY, new DataObject(DataObject.VisibilityOptions.Member, "false") },
                        { NetworkConstants.IMPOSTER_COUNT_KEY, new DataObject(DataObject.VisibilityOptions.Member, imposterCount.ToString()) }
                    }
                };
                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

                string joinCode = await RelayManager.CreateRelay(CurrentLobby.MaxPlayers);

                await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {NetworkConstants.JOINCODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                    }
                });

                await StartHeartbeatLobbyAlive();

                NetworkManager.Singleton.StartHost();

                Debug.Log($"Success Create Lobby : {CurrentLobby.LobbyCode}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed CreateLobby : {e.Message}");
            }

            JoinLobbyEvent?.Invoke(CurrentLobby);
        }

        public async UniTask JoinLobbyByIdAsync(string lobbyId)
        {
            try
            {
                var joinOptions = new JoinLobbyByIdOptions { Player = GetPlayer() };
                CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);

                string joinCode = CurrentLobby.Data[NetworkConstants.JOINCODE_KEY].Value;
                await RelayManager.JoinRelay(joinCode);

                NetworkManager.Singleton.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed JoinLobbyById : {e.Message}");
            }

            JoinLobbyEvent?.Invoke(CurrentLobby);
        }

        public async UniTask JoinLobbyByCodeAsync(string lobbyCode)
        {
            try
            {
                var joinOptions = new JoinLobbyByCodeOptions { Player = GetPlayer() };
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);

                string joinCode = CurrentLobby.Data[NetworkConstants.JOINCODE_KEY].Value;
                await RelayManager.JoinRelay(joinCode);

                NetworkManager.Singleton.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed JoinLobbyByCode : {e.Message}");
            }

            JoinLobbyEvent?.Invoke(CurrentLobby);
        }

        public async UniTask JoinQuickMatch()
        {
            try
            {
                var joinOptions = new QuickJoinLobbyOptions { Player = GetPlayer() };
                CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinOptions);

                string joinCode = CurrentLobby.Data[NetworkConstants.JOINCODE_KEY].Value;
                await RelayManager.JoinRelay(joinCode);

                NetworkManager.Singleton.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed JoinQuickMatch : {e.Message}");
            }

            JoinLobbyEvent?.Invoke(CurrentLobby);
        }


        public async UniTask StartHeartbeatLobbyAlive()
        {
            _lobbyHeartbeatToken = new CancellationTokenSource();

            try
            {
                while (!_lobbyHeartbeatToken.Token.IsCancellationRequested)
                {
                    await UniTask.DelayFrame(NetworkConstants.LOBBY_HEARTBEAT_MS);

                    await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Heartbeat loop safely cancelled");
            }
        }

        public async UniTask StartRefreshLobbyInfomation()
        {
            _lobbyRefreshInfomationToken = new CancellationTokenSource();

            try
            {
                while (!_lobbyRefreshInfomationToken.Token.IsCancellationRequested)
                {
                    await UniTask.DelayFrame(NetworkConstants.LOBBY_INFOMATION_UPDATE_MS);

                    var lobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                    CurrentLobby = lobby;
                }
            }
            catch
            {

            }
        }

        public async UniTask GetLobbyListAsync()
        {
            try
            {
                #region Filter
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>{
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"
                    )
                },
                    Order = new List<QueryOrder>{
                    new QueryOrder(
                        asc: false,
                        field: QueryOrder.FieldOptions.Created
                    )
                }
                };
                #endregion
                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();

                LobbyListChangedEvent?.Invoke(response.Results);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Failed GetLobbyList : {e.Message}");
            }
        }

        public async UniTask UpdateIsPrivate(bool isPrivate)
        {
            try
            {
                var lobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
                {
                    IsPrivate = isPrivate
                });

                CurrentLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed UpdateIsPrivate : {e.Message}");
            }
        }

        public bool GetIsHost()
        {
            return CurrentLobby != null && CurrentLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        public bool GetIsPrivate()
        {
            return CurrentLobby.IsPrivate;
        }

        public int GetMaxPlayers()
        {
            return CurrentLobby.MaxPlayers;
        }

        public int GetPlayerCount()
        {
            return CurrentLobby.Players.Count;
        }

        public string GetLobbyCode()
        {
            return CurrentLobby.LobbyCode;
        }

        private async UniTask UnityServicesInitializeAsync()
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("Initialized Unity Services");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Failed Initialized Unity Services : {e.Message}");
            }
        }

        private async UniTask SignInAnonymouslyAsync()
        {
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync();
                    PlayerId = AuthenticationService.Instance.PlayerId;

                    Debug.Log($"Signed in anonymously. Name: {PlayerName}. ID: {PlayerId}");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Failed Signed : {e.Message}");
            }
        }

        private Player GetPlayer()
        {
            return new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { NetworkConstants.PLAYERNAME_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
                }
            };
        }

        private bool IsPlayerInLobby()
        {
            if (CurrentLobby == null || CurrentLobby.Players == null)
                return false;

            return CurrentLobby.Players.Any(player => player.Id == AuthenticationService.Instance.PlayerId);
        }
    }
}
