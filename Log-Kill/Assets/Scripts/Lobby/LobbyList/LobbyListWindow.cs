using Cysharp.Threading.Tasks;
using LogKill.Core;
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
        [SerializeField] private List<LobbyListItem> _lobbyListItems = new();
        [SerializeField] private Button _quickJoinButton;

        [SerializeField] private MessageBoxWindow _messageBoxWindow;

        private CancellationTokenSource _lobbyListRefreshToken;

        private LobbyManager LobbyManager => ServiceLocator.Get<LobbyManager>();

        public async override UniTask InitializeAsync()
        {
            foreach (var lobbyListItem in _lobbyListItems)
            {
                lobbyListItem.RegisterJoinEvent(OnJoinEvent);
            }

            await UniTask.Yield();
        }

        public override void OnShow()
        {
            foreach (var lobbyListItem in _lobbyListItems)
            {
                if (lobbyListItem.gameObject.activeSelf)
                    lobbyListItem.gameObject.SetActive(false);
            }

            _messageBoxWindow.gameObject.SetActive(false);

            StartLobbyListRefresh().Forget();
        }

        public override void OnHide()
        {
            _lobbyListRefreshToken?.Cancel();
            _lobbyListRefreshToken?.Dispose();
            _lobbyListRefreshToken = null;
        }

        private async UniTask StartLobbyListRefresh()
        {
            _lobbyListRefreshToken = new CancellationTokenSource();

            try
            {
                while (!_lobbyListRefreshToken.Token.IsCancellationRequested)
                {
                    var lobbyList = await LobbyManager.GetLobbyListAsync();

                    if (lobbyList == null)
                    {
                        await UniTask.Delay(NetworkConstants.LOBBY_LIST_UPDATE_MS, cancellationToken: _lobbyListRefreshToken.Token);
                        continue;
                    }

                    _quickJoinButton.interactable = lobbyList.Count > 0;
                    UpdateLobbyList(lobbyList);

                    await UniTask.Delay(NetworkConstants.LOBBY_LIST_UPDATE_MS, cancellationToken: _lobbyListRefreshToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("LobbyList loop safely cancelled");
            }
        }

        private void UpdateLobbyList(List<Lobby> lobbies)
        {
            for (int index = 0; index < _lobbyListItems.Count; index++)
            {
                if (index < lobbies.Count)
                {
                    _lobbyListItems[index].Initialize(lobbies[index]);

                    if (!_lobbyListItems[index].gameObject.activeSelf)
                        _lobbyListItems[index].gameObject.SetActive(true);
                }
                else
                {
                    if (_lobbyListItems[index].gameObject.activeSelf)
                        _lobbyListItems[index].gameObject.SetActive(false);
                }
            }
        }

        private async void OnJoinEvent(string lobbyId)
        {
            if (!await LobbyManager.JoinLobbyByIdAsync(lobbyId))
            {
                _messageBoxWindow.OnShow("존재하지 않는 방입니다.");
            }
        }

        public async void OnClickQuickJoin()
        {
            _quickJoinButton.interactable = false;

            if (!await LobbyManager.JoinQuickMatch())
            {
                _messageBoxWindow.OnShow("방이 존재하지 않습니다");
                _quickJoinButton.interactable = true;
            }
        }

        public void OnClickBack()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}