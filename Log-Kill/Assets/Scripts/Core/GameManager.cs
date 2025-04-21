using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.Map;
using LogKill.Mission;
using LogKill.Room;
using LogKill.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill
{
	public enum EGameState
	{
		Title,
		Lobby,
		InGame,
		Vote,
		Result,
	}

	public class GameManager : MonoSingleton<GameManager>
	{
		[SerializeField] private MissionData _missionData;
		[SerializeField] private bool _isDebugMode = false;

		public bool IsDebugMode => _isDebugMode;
		private EventBus EventBus => ServiceLocator.Get<EventBus>();
		private MapService MapService => ServiceLocator.Get<MapService>();
		public EGameState GameState { get; private set; } = EGameState.Lobby;

		private async UniTask Start()
		{
			ServiceLocator.AutoRegisterServices();

            await UIManager.Instance.InitializeAsync();
            await SoundManager.Instance.InitializeAsync();

            EventBus.Subscribe<GameStartEvent>(OnGameStart);
            EventBus.Subscribe<VoteEndEvent>(OnVoteEndEvent);

            OnMoveTitleScene();
        }

		public void OnGameStart(GameStartEvent context)
		{
			GameState = EGameState.InGame;
			UIManager.Instance.CloseAllWindows();
			UIManager.Instance.ShowHUD<InGameHud>();
		}

		public void OnMoveLobbyScene()
		{
			GameState = EGameState.Lobby;
			UIManager.Instance.HideCurrentHUD();
			UIManager.Instance.CloseAllWindows();

			var lobbyHUD = UIManager.Instance.ShowHUD<LobbyHUD>();
			lobbyHUD.Initialize();
		}

		public void OnMoveTitleScene()
		{
			GameState = EGameState.Title;
			UIManager.Instance.HideCurrentHUD();
			UIManager.Instance.CloseAllWindows();

			var onlineModeWindow = UIManager.Instance.ShowWindow<OnlineModeWindow>();
			onlineModeWindow.Initialize();
		}

		public async UniTask StartSession()
		{
			Debug.Log(">> Start: Init Session");
			List<UniTask> tasks = new();

			tasks.Add(MapService.LoadMap(0));
			PlayerDataManager.Instance.StartWave();

			await tasks;
			SessionManager.Instance.NotifyPlayerLoadedServerRpc(NetworkManager.Singleton.LocalClientId);
			Debug.Log("<< Finished: Init Session");
		}

		private void OnVoteEndEvent(VoteEndEvent context)
		{
			GameState = EGameState.InGame;
			PlayerDataManager.Instance.StartWave();
		}
	}
}
