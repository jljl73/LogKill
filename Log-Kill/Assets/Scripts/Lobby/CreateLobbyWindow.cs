using Cysharp.Threading.Tasks;
using LogKill.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.LobbySystem
{
    public class CreateLobbyWindow : WindowBase
    {
        [SerializeField] private TMP_InputField _lobbyNameInputField;

        [SerializeField] private TMP_InputField _imposterCountInputField;
        [SerializeField] private TMP_InputField _maxPlayerCountInputField;

        [SerializeField] private Button _createButton;

        private int _imposterCount = 1;
        private int _maxPlayerCount = 10;

        public override UniTask InitializeAsync()
        {
            _lobbyNameInputField.onValueChanged.AddListener(value =>
            {
                _createButton.interactable = !string.IsNullOrEmpty(value);
            });

            return base.InitializeAsync();
        }

        public override void OnShow()
        {
            _createButton.interactable = false;

            _lobbyNameInputField.text = string.Empty;

            _imposterCountInputField.text = _imposterCount.ToString();
            _maxPlayerCountInputField.text = _maxPlayerCount.ToString();

            LobbyManager.Instance.JoinLobbyEvent += OnCreateLobbyComplete;
        }

        public override void OnHide()
        {
            LobbyManager.Instance.JoinLobbyEvent -= OnCreateLobbyComplete;
        }

        private void OnCreateLobbyComplete(Lobby lobby)
        {
            if (lobby == null)
            {
                _createButton.interactable = true;
            }
            else
            {
                // TODO: Scene Move
                UIManager.Instance.CloseAllWindows();

                var lobbyHUD = UIManager.Instance.ShowHUD<LobbyHUD>();
                lobbyHUD.Initialize();
            }
        }

        public async void OnClickCreateLobby()
        {
            _createButton.interactable = false;

            string lobbyName = _lobbyNameInputField.text;
            int imposterCount = int.Parse(_imposterCountInputField.text);
            int maxPlayerCount = int.Parse(_maxPlayerCountInputField.text);

            await LobbyManager.Instance.CreateLobbyAsync(lobbyName, maxPlayerCount, imposterCount);
        }

        public void OnClickBack()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}
