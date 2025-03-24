using Cysharp.Threading.Tasks;
using LogKill.Core;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace LogKill.LobbySystem
{
    public class LobbyManager : MonoSingleton<LobbyManager>
    {
        public string PlayerId { get; private set; }
        public string JoinCode { get; private set; }
        public Lobby CurrentLobby { get; private set; }

        public async UniTask InitializeAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                PlayerId = AuthenticationService.Instance.PlayerId;
                Debug.Log($"Sign in anonymously succeeded! PlayerID : {PlayerId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async UniTask CreateLobbyAsync(string lobbyName, int maxPlayerCount, int imposterCount, bool isPrivate = false)
        {
            try
            {
                var options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Player
                    (
                        id: PlayerId,
                        //profile: new PlayerProfile("profileName"),
                        data: new Dictionary<string, PlayerDataObject>
                        {
                            { "isHost", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                        }
                    ),
                    Data = new Dictionary<string, DataObject>
                    {
                        {"imposterCount", new DataObject(DataObject.VisibilityOptions.Public, imposterCount.ToString()) }
                    }
                };

                CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayerCount, options);

                var allocation = await CreateRelaySessionAsync(maxPlayerCount);
                JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                await UpdateLobbyWithJoinCodeAsync(JoinCode);

                StartHostWithRelay(allocation);

                Debug.Log($"Success Create Lobby : {CurrentLobby.LobbyCode}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Lobby creation failed: {e.Message}");
            }
        }

        public async UniTask<bool> JoinLobbyAsync(string lobbyCode)
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
                {
                    Player = new Player
                    (
                        id: PlayerId, 
                        //profile: new PlayerProfile("profileName"),
                        data: new Dictionary<string, PlayerDataObject>
                        {
                            { "isHost", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                        }
                    )
                });

                JoinCode = CurrentLobby.Data["joinCode"].Value;
                var joinAllocation = await JoinRelaySessionAsync(JoinCode);

                StartClientWithRelay(joinAllocation);

                Debug.Log($"Success Join Lobby: {lobbyCode}");
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Lobby join failed: {e.Message}");
                return false;
            }
        }

        public async UniTask<Allocation> CreateRelaySessionAsync(int maxPlayers)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                return allocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Relay creation failed: {e.Message}");
                return null;
            }
        }

        public async UniTask<JoinAllocation> JoinRelaySessionAsync(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                return joinAllocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Relay join failed: {e.Message}");
                return null;
            }
        }

        public bool GetIsHost()
        {
            var player = CurrentLobby.Players.Find(player => player.Id == PlayerId);

            return player != null && player.Data.TryGetValue("isHost", out var isHostData) && isHostData.Value == "true";
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

        public async UniTask UpdateLobbyWithIsPrivate(bool isPrivate)
        {
            var options = new UpdateLobbyOptions { IsPrivate = isPrivate };

            await UpdateLobbyOptionsAsync(options);
        }

        private async UniTask UpdateLobbyWithJoinCodeAsync(string joinCode)
        {
            var options = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };

            await UpdateLobbyOptionsAsync(options);
        }

        private async UniTask UpdateLobbyOptionsAsync(UpdateLobbyOptions options)
        {
            if (CurrentLobby == null)
            {
                Debug.LogWarning("Update failed: CurrentLobby is null.");
                return;
            }

            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log($"Lobby options update failed: {e.Message}");
            }
        }

        private void StartHostWithRelay(Allocation allocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
        }

        private void StartClientWithRelay(JoinAllocation joinAllocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }
    }
}
