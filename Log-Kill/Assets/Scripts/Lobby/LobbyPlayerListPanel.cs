using DG.Tweening;
using LogKill.Character;
using LogKill.Room;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class LobbyPlayerListPanel : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _playerListItems = new List<GameObject>();

        private Tweener _tweener = null;

        private Dictionary<ulong, GameObject> _playerListDicts = new Dictionary<ulong, GameObject>();

        public void Initialize()
        {
            foreach (var playerListItem in _playerListItems)
            {
                playerListItem.SetActive(false);
            }
        }

        public void OnShow()
        {
            if (_tweener != null && _tweener.active && _tweener.IsPlaying()) return;

            gameObject.SetActive(true);
            _tweener = transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.InOutCubic);
        }

        public void OnHide()
        {
            if (_tweener != null && _tweener.active && _tweener.IsPlaying()) return;

            _tweener = transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic)
                .OnComplete(() => { gameObject.SetActive(false); });
        }

        public void UpdatePlayerList(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId) return;

            if (_playerListDicts.ContainsKey(clientId))
            {
                _playerListDicts[clientId].SetActive(false);
                _playerListDicts.Remove(clientId);
            }
            else
            {
                var playerListItem = _playerListItems[(int)clientId];

                var playerData = PlayerDataManager.Instance.GetPlayerData(clientId);

                if (playerData.HasValue)
                {
                    playerListItem.GetComponentInChildren<Image>().color = playerData.Value.GetColor();
                    playerListItem.SetActive(true);

                    _playerListDicts.Add(clientId, playerListItem);
                }
            }
        }
    }
}
