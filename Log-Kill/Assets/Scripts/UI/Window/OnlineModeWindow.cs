using Cysharp.Threading.Tasks;
using LogKill.LobbySystem;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class OnlineModeWindow : WindowBase
    {
        [SerializeField] private TMP_InputField _lobbyCodeInputField;
        [SerializeField] private Button _joinButton;

        public override UniTask InitializeAsync()
        {
            _lobbyCodeInputField.onValueChanged.AddListener(value =>
            {
                _joinButton.interactable = !string.IsNullOrEmpty(value);
            });

            return base.InitializeAsync();
        }

        public override void OnShow()
        {
            _joinButton.interactable = false;
            _lobbyCodeInputField.text = string.Empty;

            LobbyManager.Instance.JoinLobbyEvent += OnJoinComplete;
        }

        public override void OnHide()
        {
            LobbyManager.Instance.JoinLobbyEvent -= OnJoinComplete;
        }

        private void OnJoinComplete(Lobby lobby)
        {
            UIManager.Instance.CloseAllWindows();

            var lobbyHUD = UIManager.Instance.ShowHUD<LobbyHUD>();
            lobbyHUD.Initialize();
        }

        public void OnClickCreateLobby()
        {
            var createLobbyWindow = UIManager.Instance.ShowWindow<CreateLobbyWindow>();
            createLobbyWindow.Initialize();
        }

        public void OnClickLobbyList()
        {
            var lobbyListWindow = UIManager.Instance.ShowWindow<LobbyListWindow>();
            lobbyListWindow.Initialize();
        }

        public async void OnClickLobbyJoin()
        {
            string lobbyCode = _lobbyCodeInputField.text;
            await LobbyManager.Instance.JoinLobbyByCodeAsync(lobbyCode);
        }
    }
}
