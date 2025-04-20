using LogKill.Core;
using LogKill.Event;
using LogKill.UI;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerDataManager : NetworkSingleton<PlayerDataManager>
    {
        public Dictionary<ulong, Player> PlayerDicts { get; private set; } = new();
        public Player Me { get; private set; }

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ClearAllPlayers();
        }

        public void AddPlayer(Player player)
        {
            if (PlayerDicts.ContainsKey(player.ClientId))
            {
                PlayerDicts[player.ClientId] = player;
            }
            else
            {
                PlayerDicts.Add(player.ClientId, player);
            }

            if (player.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                Me = player;
            }
        }

        public void RemovePlayer(ulong clientId)
        {
            if (PlayerDicts.ContainsKey(clientId))
            {
                PlayerDicts.Remove(clientId);
            }
        }

        public void ClearAllPlayers()
        {
            PlayerDicts.Clear();
        }

        public void StartWave()
        {
            SetDeactiveDeadPlayers();
            MoveOriginPostiona();
        }

        private void SetDeactiveDeadPlayers()
        {
            foreach (var player in PlayerDicts.Values)
            {
                if (player.IsDead)
                {
                    player.gameObject.SetActive(false);
                }
            }
        }

        private void MoveOriginPostiona()
        {
            Me.transform.position = Vector3.zero;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPlayerKillServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
        {
            // TODO : Check if the killer is impostor or not
            ulong killerId = rpcParams.Receive.SenderClientId;
            if (PlayerDicts.ContainsKey(killerId))
            {
                if (PlayerDicts[killerId].PlayerType != EPlayerType.Imposter)
                {
                    Debug.Log("Killer is Not Imposter");
                    return;
                }
            }

            if (PlayerDicts.ContainsKey(targetClientId))
            {
                BroadcastPlayerKillClientRpc(targetClientId);
            }
        }

        [ClientRpc]
        private void BroadcastPlayerKillClientRpc(ulong targetClientId)
        {
            EventBus.Publish(new PlayerKillEvent()
            {
                VictimId = targetClientId,
                IsBreak = true,
            });
        }

        [ClientRpc]
        private void BroadcastSettingImposterClientRpc(ulong clientId)
        {
            EventBus.Publish(new SettingImposterEvent() { ClientId = clientId });
        }

        public Player GetPlayer(ulong clientId)
        {
            if (PlayerDicts.TryGetValue(clientId, out Player player))
            {
                return player;
            }

            return null;
        }

        public int GetAlivePlayerCount()
        {
            return PlayerDicts.Values.Count(player => !player.IsDead);
        }

        public Player GetRandomAlivePlayer()
        {
            var alivePlayers = PlayerDicts.Values.Where(player => !player.IsDead).ToList();
            if (alivePlayers.Count == 0)
                return null;

            int randomIndex = Random.Range(0, alivePlayers.Count);
            ulong randomClientId = alivePlayers[randomIndex].ClientId;
            return NetworkManager.Singleton.ConnectedClients[randomClientId].PlayerObject.GetComponent<Player>();
        }

        public EColorType CheckPlayerAvailableColor()
        {
            var networkClients = NetworkManager.Singleton.ConnectedClients.Values;
            EColorType color = EColorType.White;

            foreach (EColorType colorType in System.Enum.GetValues(typeof(EColorType)))
            {
                bool isUsed = false;

                foreach (var client in networkClients)
                {
                    if (client.PlayerObject == null)
                        continue;

                    var player = client.PlayerObject.GetComponent<Player>();
                    if (player.ColorType == colorType)
                    {
                        isUsed = true;
                        break;
                    }
                }

                if (!isUsed)
                {
                    color = colorType;
                    break;
                }
            }

            return color;
        }

        public void SettingImposters(int imposterCount)
        {
            var suffleClientKeys = PlayerDicts.Keys.OrderBy(x => Random.value).ToList();

            for (int i = 0; i < imposterCount; i++)
            {
                var randomId = suffleClientKeys[i];
                BroadcastSettingImposterClientRpc(randomId);
            }
        }

        public bool CheckGameOver(out bool isImposterWin)
        {
            isImposterWin = false;
            int imposterCount = 0;
            int crewCount = 0;

            foreach (var player in PlayerDicts.Values)
            {
                if (!player.IsDead)
                {
                    if (player.PlayerType == EPlayerType.Imposter)
                    {
                        imposterCount++;
                    }
                    else
                    {
                        crewCount++;
                    }
                }
            }

            if (imposterCount == 0)
            {
                isImposterWin = false;
                return true;
            }
            else if (crewCount <= imposterCount)
            {
                isImposterWin = true;
                return true;
            }

            return false;
        }

    }
}
