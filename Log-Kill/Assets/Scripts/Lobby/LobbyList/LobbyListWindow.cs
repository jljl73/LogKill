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
        [SerializeField] private List<LobbyListItem> _lobbyListItems = new();
        [SerializeField] private Button _quickJoinButton;

        private CancellationTokenSource _lobbyListRefreshToken;

        public override void OnShow()
        {
            foreach (var lobbyListItem in _lobbyListItems)
            {
                lobbyListItem.gameObject.SetActive(false);
            }

            StartLobbyListRefresh().Forget();

            // LobbyManager.Instance.PlayerJoinedEvent += OnPlayerJoinedEvent;
        }

        public override void OnHide()
        {
            _lobbyListRefreshToken?.Cancel();
            _lobbyListRefreshToken?.Dispose();
            _lobbyListRefreshToken = null;

            // LobbyManager.Instance.PlayerJoinedEvent -= OnPlayerJoinedEvent;
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

                    _quickJoinButton.interactable = lobbyList.Count > 0;

                    await UniTask.Delay(NetworkConstants.LOBBY_LIST_UPDATE_MS, cancellationToken: _lobbyListRefreshToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("LobbyList loop safely cancelled");
            }
        }

        //private void OnPlayerJoinedEvent(Lobby lobby)
        //{
        //    if (lobby == null)
        //    {
        //        _quickJoinButton.interactable = true;
        //    }
        //    else
        //    {
        //        // TODO: Scene Move
        //        UIManager.Instance.CloseAllWindows();

        //        var lobbyHUD = UIManager.Instance.ShowHUD<InGameHud>();
        //        // lobbyHUD.Initialize();
        //    }
        //}

        private void UpdateLobbyList(List<Lobby> lobbies)
        {
            foreach (var lobbyListItem in _lobbyListItems)
            {
                lobbyListItem.gameObject.SetActive(false);
            }

            for (int index = 0; index < lobbies.Count; index++)
            {
                _lobbyListItems[index].Initialize(lobbies[index]);
                _lobbyListItems[index].gameObject.SetActive(true);
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