using System.Collections.Generic;
using LogKill.Core;
using LogKill.Event;
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

    public class MissionService : IService
    {
        private int _userCount;
        private int _missionCount;

        private Dictionary<ulong, HashSet<int>> _clientMissionMap = new();
        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public void Initialize()
        {
            EventBus.Subscribe<GameStartEvent>(OnGameStart);
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

        [ClientRpc]
        private void BroadcastProgressToAllClientsClientRpc(int progress, int allProgress)
        {
            EventBus.Publish<MissionProgressEvent>(new MissionProgressEvent
            {
                Progress = progress,
                AllProgress = allProgress
            });
        }

        private void BroadcastProgressToAllClients()
        {
            int completedMissions = 0;
            int totalMissionCount = _missionCount * _userCount;

            foreach (var kvp in _clientMissionMap)
            {
                completedMissions += kvp.Value.Count;
            }

            BroadcastProgressToAllClientsClientRpc(completedMissions, totalMissionCount);
        }

        private void CheckGameEndCondition()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!_clientMissionMap.ContainsKey(client))
                    return;

                if (_clientMissionMap[client].Count < _missionCount)
                    return;
            }

            EndGameClientRpc();
        }

        [ClientRpc]
        private void EndGameClientRpc()
        {
            UIManager.Instance.ShowWindow<GameResultWindow>();
        }

        public void OnGameStart(GameStartEvent context)
        {
            _userCount = context.UserCount;
            _missionCount = context.MissionCount;
            BroadcastProgressToAllClients();
        }
    }
}
