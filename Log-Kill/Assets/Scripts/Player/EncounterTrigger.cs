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
        private Player _encounterImpsoter;
        private List<Player> _encountImposters = new();

        private CancellationTokenSource _encounterPollingToken;

        private bool _isDeadPlayerEncounter;
        private float _imposterEncounterTime;

        private LogService LogService => ServiceLocator.Get<LogService>();

        public void Initalize(Player player)
        {
            _player = player;

            _encountImposters.Clear();

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
                    {
                        if (player.PlayerType == EPlayerType.Imposter)
                        {
                            _encounterImpsoter = null;
                            _imposterEncounterTime = Time.time;
                        }
                        else
                            _isDeadPlayerEncounter = true;
                    }
                    else
                    {
                        if (player.PlayerType == EPlayerType.Imposter && _encounterImpsoter == null)
                        {
                            _encounterImpsoter = player;
                        }
                    }
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

                    if (player.IsDead && _isDeadPlayerEncounter)
                    {
                        LogService.Log(new IgnoredBodyLog());
                        _isDeadPlayerEncounter = false;
                    }

                    if (player.PlayerType == EPlayerType.Imposter && _imposterEncounterTime > 0f)
                    {
                        LogService.Log(new ImposterEncounterTimeLog(Time.time - _imposterEncounterTime));
                        _encounterImpsoter = null;
                        _imposterEncounterTime = 0f;
                    }
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
                if (_encounterImpsoter != null)
                {
                    var renderer = _encounterImpsoter.GetComponentInChildren<SpriteRenderer>();

                    if (renderer.enabled)
                    {
                        _imposterEncounterTime = Time.time;
                        _encounterImpsoter = null;
                    }
                }

                await UniTask.Delay(1000, cancellationToken: _encounterPollingToken.Token);
            }
        }
    }
}
