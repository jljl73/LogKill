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
        [SerializeField] private LoadingWindow _loadingWindow;

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

            _messageBoxWindow.OnHide();
            _loadingWindow.OnHide();

            StartLobbyListRefresh().Forget();
        }

        public override void OnHide()
        {
            _lobbyListRefreshToken?.Cancel();
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

                    UpdateLobbyList(lobbyList);
                    _quickJoinButton.interactable = lobbyList.Count > 0;

                    await UniTask.Delay(NetworkConstants.LOBBY_LIST_UPDATE_MS, cancellationToken: _lobbyListRefreshToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("LobbyList loop safely cancelled");
            }
            finally
            {
                _lobbyListRefreshToken?.Dispose();
                _lobbyListRefreshToken = null;
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
            _loadingWindow.OnShow("방에 입장 중입니다...");

            if (!await LobbyManager.JoinLobbyByIdAsync(lobbyId))
            {
                _loadingWindow.OnHide();
                _messageBoxWindow.OnShow("존재하지 않는 방입니다.");
            }
        }

        public async void OnClickQuickJoin()
        {
            _loadingWindow.OnShow("방에 입장 중입니다...");

            if (!await LobbyManager.JoinQuickMatch())
            {
                _loadingWindow.OnHide();
                _messageBoxWindow.OnShow("방이 존재하지 않습니다");
            }
        }

        public void OnClickBack()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}