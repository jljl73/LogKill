using Cysharp.Threading.Tasks;
using LogKill.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace LogKill.Network
{
    public class RelayManager : IService
    {
        public void Initialize()
        {
        }

        public async UniTask<string> CreateRelay(int maxPlayers)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed CreatRelay : {e.Message}");
                return null;
            }
        }

        public async UniTask JoinRelay(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"Failed JoinRelay : {e.Message}");
            }
        }
    }
}
