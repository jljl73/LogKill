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

        public override void Initialize()
        {
            StartLobbyListRefresh();
            LobbyManager.Instance.JoinLobbyEvent += OnJoinLobbyEvent;
            LobbyManager.Instance.LobbyListChangedEvent += OnLobbyListChangedEvent;
        }

        public override void OnHide()
        {
            _lobbyListRefreshToken?.Cancel();
            _lobbyListRefreshToken?.Dispose();
            _lobbyListRefreshToken = null;

            LobbyManager.Instance.JoinLobbyEvent -= OnJoinLobbyEvent;
            LobbyManager.Instance.LobbyListChangedEvent -= OnLobbyListChangedEvent;
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

        private void OnLobbyListChangedEvent(List<Lobby> lobbies)
        {
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
                lobbyItem.RegisterJoinLobbyEvent(OnJoinLobbyEvent);

                _lobbyListItems.Add(lobbyItem.gameObject);
            }
        }
        private void OnJoinLobbyEvent(Lobby lobby)
        {
            if (lobby == null)
            {
                _quickJoinButton.interactable = true;
            }
            else
            {
                // TODO: Scene Move
                UIManager.Instance.CloseAllWindows();

                var lobbyHUD = UIManager.Instance.ShowHUD<LobbyHUD>();
                lobbyHUD.Initialize();
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
