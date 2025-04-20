using LogKill.Core;
using LogKill.LobbySystem;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.UI
{
    public class GameResultWindow : WindowBase
    {
        [SerializeField] private TMP_Text _resultText;

        public void ShowResult(bool isImposterWin)
        {
            if (isImposterWin)
            {
                _resultText.text = "Imposter Win!";
            }
            else
            {
                _resultText.text = "Crew Win!";
            }
        }

        public void OnClickCloseButton()
        {
            UIManager.Instance.CloseCurrentWindow();
            NetworkManager.Singleton.Shutdown();
            // TODO : 인게임 모두 초기화 Clinet Shutdown
        }
    }
}
