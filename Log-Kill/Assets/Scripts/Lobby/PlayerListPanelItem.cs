using LogKill.Character;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class PlayerListPanelItem : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _kickButton;

        public void InitPlayerPanelItem(PlayerData playerData, Action<ulong> callback)
        {
            _iconImage.color = playerData.GetColor();

            _kickButton.onClick.RemoveAllListeners();
            _kickButton.onClick.AddListener(() => callback?.Invoke(playerData.ClientId));
        }
    }
}
