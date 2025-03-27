using Cysharp.Threading.Tasks;
using LogKill.Network;
using LogKill.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace LogKill.LobbySystem
{
    public class LobbyListWindow : WindowBase
    {
        [SerializeField] private GameObject _lobbyListContent;
        [SerializeField] private GameObject _lobbyListItemPrefab;

        private CancellationTokenSource _lobbyListRefreshToken;

        private List<GameObject> _lobbyListItems = new List<GameObject>();

        public override void Initialize()
        {

        }

        public override void OnShow()
        {
            StartLobbyListRefresh();
            LobbyManager.Instance.JoinLobbyEvent += OnJoinLobbyComplete;
            LobbyManager.Instance.LobbyListChangedEvent += OnLobbyListChangedComplete;
        }

        public override void OnHide()
        {
            _lobbyListRefreshToken?.Cancel();
            _lobbyListRefreshToken?.Dispose();
            _lobbyListRefreshToken = null;

            LobbyManager.Instance.JoinLobbyEvent -= OnJoinLobbyComplete;
            LobbyManager.Instance.LobbyListChangedEvent -= OnLobbyListChangedComplete;
        }

        private async UniTask StartLobbyListRefresh()
        {
            _lobbyListRefreshToken = new CancellationTokenSource();

            try
            {
                while (!_lobbyListRefreshToken.Token.IsCancellationRequested)
                {
                    await LobbyManager.Instance.GetLobbyListAsync();

                    await UniTask.Delay(NetworkConstants.LOBBY_LIST_UPDATE_MS, cancellationToken: _lobbyListRefreshToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("LobbyList loop safely cancelled");
            }
        }

        private void OnLobbyListChangedComplete(List<Lobby> lobbies)
        {
            Debug.Log("OnLobbyListChangedComplete");
            UpdateLobbyList(lobbies);
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
                lobbyItem.RegisterJoinLobbyEvent(OnJoinLobbyComplete);

                _lobbyListItems.Add(lobbyItem.gameObject);
            }
        }
        private void OnJoinLobbyComplete(Lobby lobby)
        {
            if (lobby != null)
            {
                // TODO: Scene Move
                UIManager.Instance.CloseAllWindows();
                UIManager.Instance.ShowHUD<LobbyHUD>();
            }
        }

        public async void OnClickQuickJoin()
        {
            await LobbyManager.Instance.JoinQuickMatch();
        }

        public void OnClickBack()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}
