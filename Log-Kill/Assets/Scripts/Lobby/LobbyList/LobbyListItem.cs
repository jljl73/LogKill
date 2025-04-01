using LogKill.Network;
using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.LobbySystem
{
    public class LobbyListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _lobbyNameText;
        [SerializeField] private TMP_Text _imposterCountText;
        [SerializeField] private TMP_Text _playerCountText;

        [SerializeField] private Button _joinButton;

        private Lobby _lobby;

        public void Initialize(Lobby lobby)
        {
            _lobby = lobby;

            _joinButton.interactable = lobby.MaxPlayers != lobby.Players.Count;

            _lobbyNameText.text = lobby.Name;
            _imposterCountText.text = lobby.Data[NetworkConstants.IMPOSTER_COUNT_KEY].Value;
            _playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }

        public async void OnClickJoin()
        {
            await LobbyManager.Instance.JoinLobbyByIdAsync(_lobby.Id);
        }
    }
}
