using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Entity;
using LogKill.Event;
using LogKill.Log;
using LogKill.UI;
using LogKill.Vote;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class InGameHud : HUDBase
    {
        [SerializeField] private Slider _missionProgressBar;
        [SerializeField] private Button _interactButton;
        [SerializeField] private Button _reportButton;
        [SerializeField] private Button _breakButton;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LogService LogService => ServiceLocator.Get<LogService>();
        private VoteService VoteService => ServiceLocator.Get<VoteService>();
        private PlayerDataManager PlayerDataManager => PlayerDataManager.Instance;

        private IInteractable _interactableEntity;
        private List<Player> _nearbyPlayers = new List<Player>();
        private List<Player> _deadPlayers = new List<Player>();
        private bool IsImposter => PlayerDataManager.Me.PlayerType == EPlayerType.Imposter;

        public override async UniTask InitializeAsync()
        {
            _interactButton?.onClick.AddListener(OnClickInteract);
            _reportButton?.onClick.AddListener(OnClickReport);
            _breakButton?.onClick.AddListener(OnClickBreak);

            _interactButton.interactable = _interactableEntity != null;

            EventBus.Subscribe<InteractEvent>(OnInteractEvent);
            EventBus.Subscribe<MissionProgressEvent>(OnMissionProgressEvent);
            EventBus.Subscribe<PlayerRangeChagnedEvent>(OnPlayerRangeEvent);

            await UniTask.Yield();
        }

        public override void OnShow()
        {
            base.OnShow();
            _breakButton.gameObject.SetActive(IsImposter);

            _interactButton.interactable = false;
            _reportButton.interactable = GameManager.Instance.IsDebugMode || false;
            _breakButton.interactable = false;
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
            if(GameManager.Instance.GameState != EGameState.InGame)
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
                    LogService.Log(new IgnoredBodyLog());
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
                _breakButton.interactable = true;
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
            ValidateBreakButton();
        }
    }
}
