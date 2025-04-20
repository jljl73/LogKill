using TMPro;
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
    }
}
