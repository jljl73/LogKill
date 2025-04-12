using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.Network;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace LogKill.LobbySystem
{
    public class LobbyManager : Core.IService
    {
        private RelayManager RelayManager => ServiceLocator.Get<RelayManager>();

        private CancellationTokenSource _lobbyHeartbeatToken;

        #region Filter
        private QueryLobbiesOptions _lobbyListFilter = new QueryLobbiesOptions
        {
            Count = 30,
            Filters = new List<QueryFilter>{

                        // Lobby Status is Ready
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.S1,
                            op: QueryFilter.OpOptions.EQ,
                            value: ELobbyStatus.Ready.ToString()
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

        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        public Lobby CurrentLobby { get; private set; }


        public async void Initialize()
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
                        { NetworkConstants.LOBBY_STATUS_KEY, new DataObject(
                            DataObject.VisibilityOptions.Public,
                            ELobbyStatus.Initialize.ToString(),
                            DataObject.IndexOptions.S1
                            )
                        },
                        { NetworkConstants.IMPOSTER_COUNT_KEY, new DataObject(
                            DataObject.VisibilityOptions.Public,
                            imposterCount.ToString(),
                            DataObject.IndexOptions.N1
                            )
                        }
                    }
                };
                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

                StartHeartbeatLobbyAlive().Forget();

                await StartRelayWithHost();

                Debug.Log($"Success Create Lobby : {CurrentLobby.LobbyCode}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed CreateLobby : {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed CreateLobby : {e.Message}");
            }
        }

        public async UniTask<bool> JoinLobbyByIdAsync(string lobbyId)
        {
            try
            {
                var joinOptions = new JoinLobbyByIdOptions { Player = GetPlayer() };
                CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);

                await StartRelayWithClient();

                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed JoinLobbyById : {e.Message}");
            }

            return false;
        }

        public async UniTask<bool> JoinLobbyByCodeAsync(string lobbyCode)
        {
            try
            {
                var joinOptions = new JoinLobbyByCodeOptions { Player = GetPlayer() };
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);

                await StartRelayWithClient();

                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed JoinLobbyByCode : {e.Message}");

            }

            return false;
        }

        public async UniTask<bool> JoinQuickMatch()
        {
            try
            {
                var joinOptions = new QuickJoinLobbyOptions { Player = GetPlayer() };
                CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinOptions);

                await StartRelayWithClient();

                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed JoinQuickMatch : {e.Message}");
            }

            return false;
        }

        public async UniTask LeaveLobbyAsync()
        {
            if (CurrentLobby == null)
            {
                Debug.Log("Lobby is Null");
                return;
            }

            try
            {
                if (GetIsHost())
                {
                    StopHeartbeatLobbyAlive();
                    await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                }
                else
                {
                    await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, PlayerId);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Failed LeaveLobby : {e.Message}");
            }
            finally
            {
                CurrentLobby = null;
            }
        }

        public async UniTask StartHeartbeatLobbyAlive()
        {
            if (!GetIsHost())
            {
                Debug.LogWarning("Not Host");
                return;
            }

            _lobbyHeartbeatToken = new CancellationTokenSource();

            try
            {
                while (!_lobbyHeartbeatToken.Token.IsCancellationRequested)
                {
                    await UniTask.Delay(NetworkConstants.LOBBY_HEARTBEAT_MS, cancellationToken: _lobbyHeartbeatToken.Token);

                    await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Heartbeat loop safely cancelled");
            }
        }

        public async UniTask<Lobby> GetLobbyAsync(string lobbyId)
        {
            try
            {
                var lobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
                return lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Failed GetLobby : {e.Message}");
                return null;
            }
        }

        public async UniTask<List<Lobby>> GetLobbyListAsync()
        {
            try
            {
                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(_lobbyListFilter);
                return response.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Failed GetLobbyList : {e.Message}");
                return null;
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

        public int GetImposterCount()
        {
            return int.Parse(CurrentLobby.Data[NetworkConstants.IMPOSTER_COUNT_KEY].Value);
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

        private async UniTask StartRelayWithHost()
        {
            string joinCode = await RelayManager.CreateRelay(CurrentLobby.MaxPlayers);

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                    {
                        {NetworkConstants.LOBBY_STATUS_KEY, new DataObject(
                            DataObject.VisibilityOptions.Public,
                            ELobbyStatus.Ready.ToString(),
                            DataObject.IndexOptions.S1
                            )
                        },
                        {NetworkConstants.JOINCODE_KEY, new DataObject(
                            DataObject.VisibilityOptions.Member,
                            joinCode,
                            DataObject.IndexOptions.S2
                            )
                        }
                    }
            });

            NetworkManager.Singleton.StartHost();
        }

        private async UniTask StartRelayWithClient()
        {
            string joinCode = CurrentLobby.Data[NetworkConstants.JOINCODE_KEY].Value;
            await RelayManager.JoinRelay(joinCode);

            NetworkManager.Singleton.StartClient();
        }

        private void StopHeartbeatLobbyAlive()
        {
            _lobbyHeartbeatToken?.Cancel();
            _lobbyHeartbeatToken?.Dispose();
            _lobbyHeartbeatToken = null;
        }
    }
}