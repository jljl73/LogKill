using LogKill.LobbySystem;
using TMPro;
using UnityEngine;

namespace LogKill.UI
{
    public class CreateLobbyWindow : WindowBase
    {
        [SerializeField] private TMP_InputField _lobbyNameInputField;

        [SerializeField] private TMP_Text _imposterCountText;
        [SerializeField] private TMP_Text _maxPlayerCountText;

        [SerializeField] private int _imposterCount = 1;
        [SerializeField] private int _maxPlayerCount = 10;


        public override void Initialize()
        {
            base.Initialize();

            _lobbyNameInputField.text = string.Empty;
            _imposterCountText.text = $"ImposterCount : {_imposterCount}";
            _maxPlayerCountText.text = $"MaxPlayerCount : {_maxPlayerCount}";
        }

        public async void OnClickCreateLobby()
        {
            string lobbyName = _lobbyNameInputField.text;
            int imposterCount = _imposterCount;
            int maxPlayerCount = _maxPlayerCount;

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
