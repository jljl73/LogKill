using System.Collections.Generic;
using LogKill.Core;
using LogKill.UI;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Mission
{
    public struct MissionProgress : INetworkSerializable
    {
        public ulong ClientId;
        public int MissionId;
        public bool IsImposter;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref MissionId);
            serializer.SerializeValue(ref IsImposter);
        }
    }

    public interface IMissionNetController
    {
        void OnGameStart();
        void ReportMissionClearServerRpc(ulong clientId, int missionId);
    }

    public class MissionNetController : NetworkBehaviour, IMissionNetController
    {
        private Dictionary<ulong, HashSet<int>> _clientMissionMap = new();
        private MissionService MissionService => ServiceLocator.Get<MissionService>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ServiceLocator.Register<IMissionNetController, MissionNetController>(this);
        }

        public void OnGameStart()
        {
            if (IsServer)
                BroadcastProgressToAllClients();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReportMissionClearServerRpc(ulong clientId, int missionId)
        {
            if (!_clientMissionMap.ContainsKey(clientId))
                _clientMissionMap[clientId] = new HashSet<int>();

            if (_clientMissionMap[clientId].Add(missionId))
            {
                BroadcastProgressToAllClients();
                CheckGameEndCondition();
            }
        }

        private void BroadcastProgressToAllClients()
        {
            int completedMissions = 0;
            int totalMissionCount = MissionService.MissionCount * MissionService.UserCount;

            foreach (var kvp in _clientMissionMap)
            {
                completedMissions += kvp.Value.Count;
            }

            BroadcastProgressToAllClientsClientRpc(completedMissions, totalMissionCount);
        }

        [ClientRpc]
        private void BroadcastProgressToAllClientsClientRpc(int progress, int allProgress)
        {
            MissionService.SendProgress(progress, allProgress);
        }

        [ClientRpc]
        private void EndGameClientRpc()
        {
            UIManager.Instance.ShowWindow<GameResultWindow>().ShowResult(false);
        }

        private void CheckGameEndCondition()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!_clientMissionMap.ContainsKey(client))
                    return;

                if (_clientMissionMap[client].Count < MissionService.MissionCount)
                    return;
            }

            EndGameClientRpc();
        }
    }
}
