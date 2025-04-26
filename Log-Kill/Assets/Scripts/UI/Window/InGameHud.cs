using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Entity;
using LogKill.Event;
using LogKill.Log;
using LogKill.Vote;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class InGameHud : HUDBase
    {
        private readonly float _breakCooldown = 20;

        [SerializeField] private Slider _missionProgressBar;
        [SerializeField] private TMP_Text _batteryCountText;

        [SerializeField] private GameObject _playerContainter;
        [SerializeField] private Button _interactButton;
        [SerializeField] private Button _reportButton;
        [SerializeField] private Button _breakButton;
        [SerializeField] private TMP_Text _breakTimerText;
        [SerializeField] private GameObject _deathContainer;
        [SerializeField] private TMP_Text _toastMessageText;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LogService LogService => ServiceLocator.Get<LogService>();
        private VoteService VoteService => ServiceLocator.Get<VoteService>();
        private PlayerDataManager PlayerDataManager => PlayerDataManager.Instance;

        private IInteractable _interactableEntity;
        private List<Player> _nearbyPlayers = new List<Player>();
        private List<Player> _deadPlayers = new List<Player>();
        private bool IsImposter => PlayerDataManager.Me.PlayerType == EPlayerType.Imposter;
        private bool _isKillTimerFinished = false;
        private CancellationTokenSource _killTimerCts = new CancellationTokenSource();

        public override async UniTask InitializeAsync()
        {
            _interactButton?.onClick.AddListener(OnClickInteract);
            _reportButton?.onClick.AddListener(OnClickReport);
            _breakButton?.onClick.AddListener(OnClickBreak);

            _interactButton.interactable = _interactableEntity != null;

            EventBus.Subscribe<InteractEvent>(OnInteractEvent);
            EventBus.Subscribe<MissionProgressEvent>(OnMissionProgressEvent);
            EventBus.Subscribe<PlayerRangeChagnedEvent>(OnPlayerRangeEvent);
            EventBus.Subscribe<PlayerRangeChagnedEvent>(OnPlayerRangeEvent);
            EventBus.Subscribe<ItemChangedEvent>(OnItemChangedEvent);
            EventBus.Subscribe<VoteEndEvent>(OnVoteEndEvent);
            EventBus.Subscribe<PlayerKillEvent>(OnDead);
            EventBus.Subscribe<ToastMessageEvent>(OnToastMessageEvent);

            await UniTask.Yield();
        }

        public override void OnShow()
        {
            base.OnShow();
            _deathContainer.SetActive(false);
            _playerContainter.SetActive(true);

            _breakButton.gameObject.SetActive(IsImposter);
            _interactButton.interactable = false;
            _reportButton.interactable = GameManager.Instance.IsDebugMode || false;
            _breakButton.interactable = false;
            _batteryCountText.text = "0";

            StartKillTimer();
        }

        private void OnInteractEvent(InteractEvent context)
        {
            if (context.Enable)
            {
                _interactableEntity = context.InteractableEntity;
                _interactButton.interactable = true;
            }
            else
            {
                if (context.InteractableEntity == _interactableEntity)
                    _interactableEntity = null;
                _interactButton.interactable = false;
            }
        }

        private void OnPlayerRangeEvent(PlayerRangeChagnedEvent context)
        {
            if (GameManager.Instance.GameState != EGameState.InGame)
                return;

            if (context.IsNearby)
            {
                if (context.TargetPlayer.IsDead)
                {
                    _deadPlayers.Add(context.TargetPlayer);
                }
                else
                {
                    _nearbyPlayers.Add(context.TargetPlayer);
                }
            }
            else
            {
                if (context.TargetPlayer.IsDead)
                {
                    _deadPlayers.Remove(context.TargetPlayer);
                }
                else
                {
                    _nearbyPlayers.Remove(context.TargetPlayer);
                }
            }

            ValidateReportButton();
            ValidateBreakButton();
        }

        private void OnDead(PlayerKillEvent context)
        {
            if (context.VictimId == PlayerDataManager.Instance.Me.ClientId)
            {
                _deathContainer.SetActive(true);
                _playerContainter.SetActive(false);
                return;
            }
        }

        private void ValidateReportButton()
        {
            if (_deadPlayers.Count > 0)
            {
                _reportButton.interactable = true;
            }
            else
            {
                _reportButton.interactable = GameManager.Instance.IsDebugMode || false;
            }
        }

        private void ValidateBreakButton()
        {
            if (IsImposter == false)
                return;

            if (_nearbyPlayers.Count > 0)
            {
                _breakButton.interactable = _isKillTimerFinished;
            }
            else
            {
                _breakButton.interactable = false;
            }
        }

        private void OnMissionProgressEvent(MissionProgressEvent context)
        {
            Debug.Log($"Mission Progress: {context.Progress}/{context.AllProgress}");
            _missionProgressBar.value = (float)context.Progress / context.AllProgress;
        }

        private void OnItemChangedEvent(ItemChangedEvent context)
        {
            if (context.ItemType == EItemType.Battery)
            {
                UpdateBatteryCount(context.ItemCount);
            }
        }

        private void OnVoteEndEvent(VoteEndEvent context)
        {
            StartKillTimer();
        }

        private void OnToastMessageEvent(ToastMessageEvent context)
        {
            ShowToastMessage(context.Message).Forget();
        }

        private async UniTask ShowToastMessage(string message)
        {
            _toastMessageText.text = message;
            _toastMessageText.gameObject.SetActive(true);
            var timer = 0.0f;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                _toastMessageText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, timer * 2.0f));
                await UniTask.NextFrame();
            }
            await UniTask.Delay(2000);

            timer = 0.0f;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                _toastMessageText.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, timer * 2.0f));
                await UniTask.NextFrame();
            }
            _toastMessageText.gameObject.SetActive(false);
        }

        private void UpdateBatteryCount(int amount)
        {
            _batteryCountText.text = amount.ToString();
        }


        private void StartKillTimer()
        {
            if (IsImposter == false)
                return;

            _killTimerCts?.Cancel();
            _killTimerCts = new();
            KillTimer().Forget();
        }

        private async UniTask KillTimer()
        {
            _isKillTimerFinished = false;
            ValidateBreakButton();

            if (IsImposter == false)
                return;

            var timer = _breakCooldown;
            while (timer > 0)
            {
                _breakTimerText.text = Mathf.CeilToInt(timer).ToString();
                await UniTask.NextFrame(cancellationToken: _killTimerCts.Token);
                timer -= Time.deltaTime;
            }

            _isKillTimerFinished = true;
            _breakTimerText.text = string.Empty;
            ValidateBreakButton();
        }

        public void OnClickInteract()
        {
            _interactableEntity?.Interact();
        }

        public void OnClickReport()
        {
            VoteService.OnVoteStart(new VoteStartEvent()
            {
                ReportClientId = NetworkManager.Singleton.LocalClientId
            });
        }

        public void OnClickBreak()
        {
            if (_nearbyPlayers.Count == 0)
                return;

            LogService.Log(new KillLog());
            PlayerDataManager.Instance.RequestPlayerKillServerRpc(_nearbyPlayers[0].ClientId);
            _nearbyPlayers.RemoveAt(0);
            StartKillTimer();
        }

        public void OnClickChangeTarget()
        {
            PlayerDataManager.Instance.WatchRandomAlivePlayer();
        }
    }
}
