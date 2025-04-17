using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.LobbySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class OnlineModeWindow : WindowBase
    {
        [SerializeField] private TMP_InputField _lobbyCodeInputField;
        [SerializeField] private Button _joinButton;

        [SerializeField] private MessageBoxWindow _messageBoxWindow;

        private LobbyManager LobbyManager => ServiceLocator.Get<LobbyManager>();

        public async override UniTask InitializeAsync()
        {
            _lobbyCodeInputField.onValueChanged.AddListener(value =>
            {
                _joinButton.interactable = !string.IsNullOrEmpty(value);
            });

            await UniTask.Yield();
        }

        public override void OnShow()
        {
            _joinButton.interactable = false;
            _lobbyCodeInputField.text = string.Empty;

            _messageBoxWindow.gameObject.SetActive(false);
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
            _joinButton.interactable = false;

            string lobbyCode = _lobbyCodeInputField.text;

            if (!await LobbyManager.JoinLobbyByCodeAsync(lobbyCode))
            {
                _messageBoxWindow.OnShow("존재하지 않는 코드입니다.");

                _lobbyCodeInputField.text = string.Empty;
                _joinButton.interactable = true;
            }
        }
    }
}
