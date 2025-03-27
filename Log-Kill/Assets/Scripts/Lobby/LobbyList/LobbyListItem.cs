using LogKill.Network;
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

        public void Initialize(Lobby lobby)
        {
            _lobbyNameText.text = lobby.Name;
            _imposterCountText.text = lobby.Data[NetworkConstants.IMPOSTER_COUNT_KEY].Value;
            _playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }
    }
}
