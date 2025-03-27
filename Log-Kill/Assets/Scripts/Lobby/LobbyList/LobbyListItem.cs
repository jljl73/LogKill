using LogKill.Network;
using LogKill.UI;
using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace LogKill.LobbySystem
{
    public class LobbyListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _lobbyNameText;
        [SerializeField] private TMP_Text _imposterCountText;
        [SerializeField] private TMP_Text _playerCountText;

        private string _lobbyId;

        public void Initialize(Lobby lobby)
        {
            _lobbyNameText.text = lobby.Name;
            _imposterCountText.text = lobby.Data[NetworkConstants.IMPOSTER_COUNT_KEY].Value;
            _playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

            _lobbyId = lobby.Id;
        }

        public void RegisterJoinLobbyEvent(Action<Lobby> callback)
        {
            LobbyManager.Instance.JoinLobbyEvent -= callback;
            LobbyManager.Instance.JoinLobbyEvent += callback;
        }

        public async void OnClickJoin()
        {
            await LobbyManager.Instance.JoinLobbyByIdAsync(_lobbyId);
        }
    }
}
