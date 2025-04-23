using LogKill.Core;
using LogKill.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.LobbySystem
{
    public class CreateLobbyWindow : WindowBase
    {
        [SerializeField] private TMP_InputField _lobbyNameInputField;

        [SerializeField] private List<CountButton> _imposterCountButtons;
        [SerializeField] private List<CountButton> _maxPlayerCountButtons;

        [SerializeField] private MessageBoxWindow _messageBoxWindow;
        [SerializeField] private LoadingWindow _loadingWindow;

        private readonly int MIN_IMPOSTER_COUNT = 1;

        private readonly int MIN_PLAYER_COUNT = 4;
        private readonly int MAX_PLAYER_COUNT = 10;

        private readonly Dictionary<int, int> IMPOSTER_TO_LIMIT_PLAYER = new()
        {
            { 1, 4 },
            { 2, 7 },
            { 3, 9 }
        };

        private int _imposterCount;
        private int _maxPlayerCount;

        private LobbyManager LobbyManager => ServiceLocator.Get<LobbyManager>();

        public override void OnShow()
        {
            _lobbyNameInputField.text = string.Empty;

            _messageBoxWindow.OnHide();
            _loadingWindow.OnHide();

            UpdateImposterCount(MIN_IMPOSTER_COUNT);
            UpdateMaxPlayerCount(MAX_PLAYER_COUNT);
        }

        public void UpdateImposterCount(int count)
        {
            _imposterCount = count;

            for (int index = 0; index < _imposterCountButtons.Count; index++)
            {
                bool isSelect = index == count - MIN_IMPOSTER_COUNT;
                _imposterCountButtons[index].OnSelect(isSelect);
            }

            int limitMaxPlayerCount = GetLimitMaxPlayerCount(_imposterCount);
            if (_maxPlayerCount < limitMaxPlayerCount)
            {
                UpdateMaxPlayerCount(limitMaxPlayerCount);
            }

            for (int index = 0; index < _maxPlayerCountButtons.Count; index++)
            {
                bool isDisabled = index < limitMaxPlayerCount - MIN_PLAYER_COUNT;
                _maxPlayerCountButtons[index].OnDisabled(isDisabled);
            }
        }

        public void UpdateMaxPlayerCount(int count)
        {
            _maxPlayerCount = count;

            for (int index = 0; index < _maxPlayerCountButtons.Count; index++)
            {
                bool isSelect = index == count - MIN_PLAYER_COUNT;
                _maxPlayerCountButtons[index].OnSelect(isSelect);
            }
        }

        private int GetLimitMaxPlayerCount(int imposterCount)
        {
            return IMPOSTER_TO_LIMIT_PLAYER.TryGetValue(imposterCount, out int limitMaxPlayer) ? limitMaxPlayer : MIN_PLAYER_COUNT;
        }

        private string GetValidLobbyName(string input, string fallback = "Default")
        {
            return string.IsNullOrWhiteSpace(input) ? fallback : input;
        }

        public async void OnClickCreateLobby()
        {
            _loadingWindow.OnShow("방을 생성중입니다...");

            string lobbyName = GetValidLobbyName(_lobbyNameInputField.text);
            if (!await LobbyManager.CreateLobbyAsync(lobbyName, _maxPlayerCount, _imposterCount))
            {
                _loadingWindow.OnHide();
                _messageBoxWindow.OnShow("방 생성에 실패하였습니다.");
            }
        }

        public void OnClickBack()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}
