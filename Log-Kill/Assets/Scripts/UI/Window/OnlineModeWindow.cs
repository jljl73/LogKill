using LogKill.LobbySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class OnlineModeWindow : WindowBase
    {
        [SerializeField] private TMP_InputField _lobbyCodeField;
        [SerializeField] private Button _joinButton;

        public override void Initialize()
        {
            base.Initialize();

            _joinButton.interactable = false;

            _lobbyCodeField.onValueChanged.AddListener(value =>
            {
                _joinButton.interactable = !string.IsNullOrEmpty(value);
            });
        }

        public void OnClickCreateLobby()
        {
            var createLobbyWindow = UIManager.Instance.ShowWindow<CreateLobbyWindow>();
            createLobbyWindow.Initialize();
        }

        public void OnClickLobbyList()
        {
            // TODO ShowWIndow LobbyListWindow
            Debug.Log("TODO: ShowWIndow LobbyListWindow");
        }

        public async void OnClickLobbyJoin()
        {
            string lobbyCode = _lobbyCodeField.text;

            if (await LobbyManager.Instance.JoinLobbyAsync(lobbyCode))
            {
                // TODO: Scene Move
                UIManager.Instance.CloseAllWindows();

                var lobbyHUD = UIManager.Instance.ShowHUD<LobbyHUD>();
                lobbyHUD.Initialize();
            }
            else
            {
                Debug.Log("Lobby Join Failed");
            }
        }
    }
}
