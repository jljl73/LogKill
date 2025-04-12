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

            //LobbyManager.Instance.PlayerJoinedEvent += OnPlayerJoinedEvent;
        }

        public override void OnHide()
        {
            //LobbyManager.Instance.PlayerJoinedEvent -= OnPlayerJoinedEvent;
        }

        //private void OnPlayerJoinedEvent(Lobby lobby)
        //{
        //    Debug.Log("OnlineModeWindow OnPlayerJoinedEvent");

        //    if (lobby == null)
        //    {
        //        _joinButton.interactable = true;
        //    }
        //    else
        //    {
        //        // TODO: Scene Move
        //        UIManager.Instance.CloseAllWindows();

        //        var lobbyHUD = UIManager.Instance.ShowHUD<InGameHud>();
        //        lobbyHUD.Initialize();
        //    }
        //}

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
                Debug.Log("입장 실패");
            }
        }
    }
}
