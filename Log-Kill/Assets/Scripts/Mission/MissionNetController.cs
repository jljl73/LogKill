using LogKill.Character;
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
        private int _completeMissionCount;
        private int _totalMissionCount;

        private MissionService MissionService => ServiceLocator.Get<MissionService>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ServiceLocator.Register<IMissionNetController, MissionNetController>(this);
        }

        public void OnGameStart()
        {
            if (IsServer)
            {
                _completeMissionCount = 0;
                _totalMissionCount = MissionService.MissionCount * MissionService.UserCount;
                BroadcastProgressToAllClientsClientRpc(_completeMissionCount, _totalMissionCount);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReportMissionClearServerRpc(ulong clientId, int missionId)
        {
            var playerType = PlayerDataManager.Instance.GetPlayer(clientId).PlayerType;

            if (playerType == EPlayerType.Imposter)
            {
                _completeMissionCount = Mathf.Max(0, _completeMissionCount - 1);
            }
            else
            {
                _completeMissionCount++;
            }

            BroadcastProgressToAllClientsClientRpc(_completeMissionCount, _totalMissionCount);
            CheckGameEndCondition();
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
            if (_completeMissionCount < _totalMissionCount)
                return;

            EndGameClientRpc();
        }
    }
}
