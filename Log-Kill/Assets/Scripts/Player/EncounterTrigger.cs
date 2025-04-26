using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Log;
using System.Threading;
using UnityEngine;

namespace LogKill
{
    public class EncounterTrigger : MonoBehaviour
    {
        private Player _player;
        private Player _detectImposter;

        private CancellationTokenSource _imposterDetectPollingToken;

        private bool _isDeadPlayerEncounter;
        private float _imposterEncounterTime;

        private LogService LogService => ServiceLocator.Get<LogService>();

        public void Initalize(Player player)
        {
            _player = player;
        }

        private void OnDisable()
        {
            _imposterDetectPollingToken?.Cancel();
            _imposterDetectPollingToken?.Dispose();
            _imposterDetectPollingToken = null;
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
                            _detectImposter = null;
                            _imposterEncounterTime = Time.time;

                            _imposterDetectPollingToken?.Cancel();
                            _imposterDetectPollingToken = null;
                        }

                        if (player.IsDead)
                            _isDeadPlayerEncounter = true;
                    }
                    else
                    {
                        if (player.PlayerType == EPlayerType.Imposter && _detectImposter == null)
                        {
                            _detectImposter = player;
                            StartImposterDetectPooling();
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
                        _imposterEncounterTime = 0f;
                    }

                    if (player == _detectImposter)
                    {
                        _detectImposter = null;
                        _imposterDetectPollingToken?.Cancel();
                    }
                }
            }
        }

        private void StartImposterDetectPooling()
        {
            _imposterDetectPollingToken?.Cancel();
            _imposterDetectPollingToken = new();

            ImposterDetectPooling().Forget();
        }

        private async UniTask ImposterDetectPooling()
        {
            while (!_imposterDetectPollingToken.Token.IsCancellationRequested)
            {
                var renderer = _detectImposter.GetComponentInChildren<SpriteRenderer>();

                if (renderer.enabled)
                {
                    _detectImposter = null;
                    _imposterEncounterTime = Time.time;

                    _imposterDetectPollingToken?.Cancel();
                }

                await UniTask.Delay(1000, cancellationToken: _imposterDetectPollingToken.Token);
            }
        }
    }
}
