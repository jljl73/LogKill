using LogKill.Core;
using LogKill.Log;
using LogKill.UI;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteService : NetworkBehaviour, IService
    {
        private LogService LogService => ServiceLocator.Get<LogService>();


        public void Initialize()
        {

        }

        [ServerRpc(RequireOwnership = false)]
        public void StartVotingServerRpc()
        {
            StartVotingClientRpc();
        }

        [ClientRpc]
        public void StartVotingClientRpc()
        {
            Debug.Log("StartVotingClientRpc");

            var logList = LogService.GetRandomLogList();

            var selectLogWindow = UIManager.Instance.ShowWindow<SelectLogWindow>();
            selectLogWindow.InitLogList(logList);
        }
    }
}
