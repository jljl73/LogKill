using LogKill.Character;
using LogKill.Core;
using LogKill.Log;
using System.Collections.Generic;
using UnityEngine;

namespace LogKill
{
    public class EncounterTrigger : MonoBehaviour
    {
        private Player _player;
        private Dictionary<ulong, float> _encounterTimes = new();
        private LogService LogService => ServiceLocator.Get<LogService>();

        public void Initalize(Player player)
        {
            _player = player;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != _player)
                {
                    var renderer = player.SpriteRenderer;

                    if (renderer.enabled)
                    {
                        if (player.IsDead == false && player.PlayerType == EPlayerType.Imposter)
                        {
                            _encounterTimes[player.ClientId] = Time.time;
                            player.OnDisableRender += OnDisableRender;
                        }
                    }
                    else
                    {
                        if (player.PlayerType == EPlayerType.Imposter)
                        {
                            player.OnDisableRender += OnDisableRender;
                        }
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != _player && _player.SpriteRenderer.enabled)
                {
                    var renderer = player.SpriteRenderer;
                    player.OnDisableRender -= OnDisableRender;
                    float encounterTime = 0;

                    if (player.IsDead)
                    {
                        LogService.Log(new IgnoredBodyLog());
                    }
                    else
                    {
                        LogService.Log(new LastEncounterLog(player.PlayerData.Name));

                        if (player.PlayerType == EPlayerType.Imposter)
                        {
                            if (_encounterTimes.TryGetValue(player.ClientId, out encounterTime))
                            {
                                LogService.Log(new ImposterEncounterTimeLog(Time.time - encounterTime));
                                _encounterTimes.Remove(player.ClientId);
                            }
                        }
                    }
                }

                _encounterTimes.Remove(player.ClientId);
            }
        }

        // private void StartImposterDetectPooling(Player player)
        // {
        //     _imposterDetectPollingToken?.Cancel();
        //     _imposterDetectPollingToken = new();

        //     ImposterDetectPooling(player).Forget();
        // }

        // private async UniTask ImposterDetectPooling(Player player)
        // {
        //     while (!_imposterDetectPollingToken.Token.IsCancellationRequested)
        //     {
        //         var renderer = player.GetComponentInChildren<SpriteRenderer>();

        //         if (renderer.enabled)
        //         {
        //             _encounterTimes[player.ClientId] = Time.time;
        //             _imposterDetectPollingToken?.Cancel();
        //             break;
        //         }

        //         await UniTask.Delay(1000, cancellationToken: _imposterDetectPollingToken.Token);
        //     }
        // }

        private void OnDisableRender(Player player, bool isDisable)
        {
            if (isDisable)
            {
                player.OnDisableRender -= OnDisableRender;
                if (player.IsDead)
                {
                    LogService.Log(new IgnoredBodyLog());
                }
                else
                {
                    LogService.Log(new LastEncounterLog(player.PlayerData.Name));

                    if (player.PlayerType == EPlayerType.Imposter)
                    {
                        if (_encounterTimes.TryGetValue(player.ClientId, out var encounterTime))
                        {
                            LogService.Log(new ImposterEncounterTimeLog(Time.time - encounterTime));
                            _encounterTimes.Remove(player.ClientId);
                        }
                    }
                }

                _encounterTimes.Remove(player.ClientId);
            }

            else
            {
                if (player.PlayerType == EPlayerType.Imposter)
                {
                    _encounterTimes[player.ClientId] = Time.time;
                }
            }
        }
    }
}
