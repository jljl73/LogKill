using System.Collections.Generic;
using System.Linq;
using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.UI;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Log
{
    public interface ILogNetController
    {
        void RequestRandomPlayerLogServerRpc(ulong clientId);
    }

    [RequireComponent(typeof(NetworkBehaviour))]
    public class LogNetController : NetworkBehaviour, ILogNetController
    {
        override public void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ServiceLocator.Register<ILogNetController, LogNetController>(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestRandomPlayerLogServerRpc(ulong requestClientId)
        {
            if (!IsServer)
                return;

            if (SelectRandomPlayer(requestClientId, out var randomPlayerId))
            {
                ClientRpcParams rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { randomPlayerId }
                    }
                };
                GetCurrentRandomLogClientRpc(requestClientId, randomPlayerId, rpcParams);
            }
        }

        [ClientRpc(RequireOwnership = false)]
        private void GetCurrentRandomLogClientRpc(ulong reportClientId, ulong clientId, ClientRpcParams rpcParams = default)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
                return;

            ServiceLocator.Get<EventBus>().Publish(new ToastMessageEvent()
            {
                Message = "[LOG] 외부에서 당신의 활동 기록이 조회되었습니다.",
            });

            var logService = ServiceLocator.Get<LogService>();
            var logList = logService.GetCreminalScoreWeightedRandomLogList(3);

            if (logList.Count > 0)
            {
                var log = string.Empty;
                for (int i = 0; i < logList.Count; i++)
                {
                    log += $"{i + 1}. {logList[i]}\n";
                }
                SendMyLogServerRpc(reportClientId, $"{PlayerDataManager.Instance.Me.PlayerData.Name}\n{log}");
                return;
            }

            SendMyLogServerRpc(reportClientId, string.Empty);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendMyLogServerRpc(ulong requestClientId, string log)
        {
            if (!IsServer)
                return;

            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new List<ulong> { requestClientId }
                }
            };
            SendRandomPlayerLogClientRpc(log, rpcParams);
        }

        [ClientRpc(RequireOwnership = false)]
        public void SendRandomPlayerLogClientRpc(string log, ClientRpcParams rpcParams = default)
        {
            UIManager.Instance.ShowWindow<SDcardWindow>().SetLog(log);
        }

        private bool SelectRandomPlayer(ulong exceptClientId, out ulong targetClientId)
        {
            var players = PlayerDataManager.Instance.PlayerDicts;
            var randomPlayer = players.Values.Where(player => player.ClientId != exceptClientId && player.IsDead == false).ToList();

            if (randomPlayer.Count == 0)
            {
                targetClientId = 0;
                return false;
            }

            int randomIndex = Random.Range(0, randomPlayer.Count);
            targetClientId = randomPlayer[randomIndex].ClientId;
            return true;
        }
    }
}
