using Cysharp.Threading.Tasks;
using LogKill.Network;
using LogKill.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.LobbySystem
{
    public class LobbyListWindow : WindowBase
    {
        [SerializeField] private GameObject _lobbyListContent;
        [SerializeField] private GameObject _lobbyListItemPrefab;

        [SerializeField] private Button _quickJoinButton;

        private CancellationTokenSource _lobbyListRefreshToken;

        private List<GameObject> _lobbyListItems = new List<GameObject>();

        public override void OnShow()
        {
            StartLobbyListRefresh();

            LobbyManager.Instance.PlayerJoinedEvent += OnPlayerJoinedEvent;
        }

        public override void OnHide()
        {
            _lobbyListRefreshToken?.Cancel();
            _lobbyListRefreshToken?.Dispose();
            _lobbyListRefreshToken = null;

            LobbyManager.Instance.PlayerJoinedEvent -= OnPlayerJoinedEvent;
        }

        private async UniTask StartLobbyListRefresh()
        {
            _lobbyListRefreshToken = new CancellationTokenSource();

            try
            {
                while (!_lobbyListRefreshToken.Token.IsCancellationRequested)
                {
                    var lobbyList = await LobbyManager.Instance.GetLobbyListAsync();
                    UpdateLobbyList(lobbyList);

                    await UniTask.Delay(NetworkConstants.LOBBY_LIST_UPDATE_MS, cancellationToken: _lobbyListRefreshToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("LobbyList loop safely cancelled");
            }
        }

        private void OnPlayerJoinedEvent(Lobby lobby)
        {
            if (lobby == null)
            {
                _quickJoinButton.interactable = true;
            }
            else
            {
                // TODO: Scene Move
                UIManager.Instance.CloseAllWindows();

                var lobbyHUD = UIManager.Instance.ShowHUD<InGameHud>();
                // lobbyHUD.Initialize();
            }
        }

        private void UpdateLobbyList(List<Lobby> lobbies)
        {
            // TODO Object Polling
            foreach (GameObject lobbyItem in _lobbyListItems)
            {
                Destroy(lobbyItem);
            }

            foreach (Lobby lobby in lobbies)
            {
                LobbyListItem lobbyItem = Instantiate(_lobbyListItemPrefab, _lobbyListContent.transform).GetComponent<LobbyListItem>();
                lobbyItem.Initialize(lobby);

                _lobbyListItems.Add(lobbyItem.gameObject);
            }
        }

        public async void OnClickQuickJoin()
        {
            _quickJoinButton.interactable = false;

            await LobbyManager.Instance.JoinQuickMatch();
        }

        public void OnClickBack()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}