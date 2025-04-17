using DG.Tweening;
using LogKill.Character;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.UI
{
    public class PlayerListPanel : MonoBehaviour
    {
        [SerializeField] private List<PlayerListPanelItem> _playerListPanelItems = new();

        private Tweener _tweener = null;

        public void Initialize()
        {
            foreach (var playerListItem in _playerListPanelItems)
            {
                playerListItem.gameObject.SetActive(false);
            }
        }

        public void UpdatePlayerList()
        {
            var players = PlayerDataManager.Instance.PlayerDicts
                .OrderBy(pair => pair.Key)
                .Select(pair => pair.Value)
                .ToList();

            for (int i = 0; i < _playerListPanelItems.Count; i++)
            {
                if (i < players.Count)
                {
                    ulong clientId = players[i].ClientId;

                    if (NetworkManager.Singleton.LocalClientId == clientId) continue;

                    _playerListPanelItems[i].InitPlayerPanelItem(players[i].PlayerData, OnClickKickPlayer);
                    _playerListPanelItems[i].gameObject.SetActive(true);
                }
                else
                {
                    _playerListPanelItems[i].gameObject.SetActive(false);
                }
            }
        }

        public void TogglePanel()
        {
            if (_tweener != null && _tweener.active && _tweener.IsPlaying()) return;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                _tweener = transform.DOScale(Vector3.one, 0.5f)
                    .From(Vector3.zero)
                    .SetEase(Ease.InOutCubic);
            }
            else
            {
                _tweener = transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InOutCubic)
                    .OnComplete(() => { gameObject.SetActive(false); });
            }
        }

        public void OnClickKickPlayer(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }
}
