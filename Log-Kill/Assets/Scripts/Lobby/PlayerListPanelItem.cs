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
            _iconImage.sprite = GetColorSprite(playerData.ColorType);

            _kickButton.onClick.RemoveAllListeners();
            _kickButton.onClick.AddListener(() => callback?.Invoke(playerData.ClientId));
        }

        private Sprite GetColorSprite(EColorType colorType)
        {
            return SpriteResourceManager.Instance.GetPlayerSprite(colorType);
        }
    }
}
