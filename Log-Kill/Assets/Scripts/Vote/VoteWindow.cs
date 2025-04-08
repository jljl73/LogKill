using LogKill.Character;
using LogKill.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteWindow : WindowBase
    {
        [SerializeField] private VotePanel[] _votePanels;

        private Dictionary<ulong, VotePanel> _votePanelDict = new();

        public void InitVotePanel(VoteData[] voteDatas)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;

            voteDatas
                .OrderBy(data => data.PlayerData.ClientId == clientId ? 0 : 1)
                .ToArray();


            var voteData = voteDatas.FirstOrDefault(data => data.PlayerData.ClientId == clientId);
            bool isImposter = voteData.PlayerData.PlayerType == EPlayerType.Imposter;

            int panelIndex = 0;


        }




        public void InitVotePanel(Dictionary<ulong, string> playerLogDict)
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            var playerDatas = PlayerDataManager.Instance.PlayerDataDict;
            bool isImposter = playerDatas[localClientId].PlayerType == EPlayerType.Imposter;

            int panelIndex = 0;

            foreach (var playerData in playerDatas)
            {
                int index = (playerData.Key == localClientId) ? 0 : ++panelIndex;

                string logMessage = playerLogDict[playerData.Key];

                var votePanel = _votePanels[index];
                votePanel.Initialize(playerData.Value, logMessage, isImposter);
                votePanel.gameObject.SetActive(true);

                _votePanelDict[playerData.Key] = votePanel;
            }

            _votePanelDict[localClientId].OnDisabledPanelButton();

            for (int i = playerDatas.Count; i < _votePanels.Length; i++)
            {
                _votePanels[i].gameObject.SetActive(false);
            }
        }


        public override void OnShow()
        {
            _votePanelDict.Clear();
        }


        public void OnClickVotePanel(int index)
        {
            for (int i = 0; i < _votePanels.Length; i++)
            {
                if (!_votePanels[i].gameObject.activeSelf) continue;

                _votePanels[i].OnSelect(i == index);
            }
        }
    }
}
