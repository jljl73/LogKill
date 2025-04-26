using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Log;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LogKill
{
    public class EncounterTrigger : MonoBehaviour
    {
        private Player _player;
        private List<Player> _encountPlayers = new();
        private CancellationTokenSource _encounterPollingToken;

        private LogService LogService => ServiceLocator.Get<LogService>();

        public void Initalize(Player player)
        {
            _player = player;

            _encountPlayers.Clear();

            StartEncounterPooling();
        }

        private void OnDisable()
        {
            _encounterPollingToken?.Cancel();
            _encounterPollingToken?.Dispose();
            _encounterPollingToken = null;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != _player)
                {
                    var renderer = player.GetComponentInChildren<SpriteRenderer>();

                    if (renderer.enabled)
                        EncounterLog(player);
                    else
                        _encountPlayers.Add(player);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != _player)
                {
                    var renderer = player.GetComponentInChildren<SpriteRenderer>();

                    if (renderer.enabled)
                        LogService.Log(new LastEncounterLog(player.PlayerData.Name));

                    if (_encountPlayers.Contains(player))
                        _encountPlayers.Remove(player);
                }
            }
        }

        private void StartEncounterPooling()
        {
            _encounterPollingToken?.Cancel();
            _encounterPollingToken = new();

            EncounterPooling().Forget();
        }

        private async UniTask EncounterPooling()
        {
            while (!_encounterPollingToken.Token.IsCancellationRequested)
            {
                for (int i = _encountPlayers.Count - 1; i >= 0; i--)
                {
                    var player = _encountPlayers[i];
                    var renderer = player.GetComponentInChildren<SpriteRenderer>();

                    if (renderer.enabled)
                    {
                        EncounterLog(player);
                        _encountPlayers.RemoveAt(i);
                    }
                }

                await UniTask.Delay(1000, cancellationToken: _encounterPollingToken.Token);
            }
        }

        private void EncounterLog(Player player)
        {
            if (player.IsDead)
            {
                LogService.Log(new IgnoredBodyLog());
                return;
            }

            if (player.PlayerType == EPlayerType.Imposter)
                LogService.Log(new ImposterEncounterLog());
            else if (player.PlayerType == EPlayerType.Normal)
                LogService.Log(new CrewmateEncounterLog());
        }
    }
}
