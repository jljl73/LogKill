using LogKill.UI;
using TMPro;
using UnityEngine;

namespace LogKill.LobbySystem
{
    public class CreateLobbyWindow : WindowBase
    {
        [SerializeField] private TMP_InputField _lobbyNameInputField;

        [SerializeField] private TMP_InputField _imposterCountInputField;
        [SerializeField] private TMP_InputField _maxPlayerCountInputField;

        private int _imposterCount = 1;
        private int _maxPlayerCount = 10;

        public override void Initialize()
        {
            _lobbyNameInputField.text = string.Empty;

            _imposterCountInputField.text = _imposterCount.ToString();
            _maxPlayerCountInputField.text = _maxPlayerCount.ToString();
        }

        public async void OnClickCreateLobby()
        {
            string lobbyName = _lobbyNameInputField.text;
            int imposterCount = int.Parse(_imposterCountInputField.text);
            int maxPlayerCount = int.Parse(_maxPlayerCountInputField.text);

            await LobbyManager.Instance.CreateLobbyAsync(lobbyName, maxPlayerCount, imposterCount);

            // TODO: Scene Move
            UIManager.Instance.CloseAllWindows();

            var lobbyHUD = UIManager.Instance.ShowHUD<LobbyHUD>();
            lobbyHUD.Initialize();
        }

        public void OnClickBack()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}
