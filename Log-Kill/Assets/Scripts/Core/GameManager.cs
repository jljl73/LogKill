using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.LobbySystem;
using LogKill.Event;
using LogKill.Map;
using LogKill.Mission;
using LogKill.Room;
using LogKill.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill
{
	public class GameManager : MonoSingleton<GameManager>
	{
		[SerializeField] private MissionData _missionData;

		private EventBus EventBus => ServiceLocator.Get<EventBus>();
		private MapService MapService => ServiceLocator.Get<MapService>();
		private LobbyManager LobbyManager => ServiceLocator.Get<LobbyManager>();

		private async UniTask Start()
		{
			ServiceLocator.AutoRegisterServices();

			await UIManager.Instance.InitializeAsync();

			EventBus.Subscribe<GameStartEvent>(OnGameStart);

			OnMoveTitleScene();
		}

		public void OnGameStart(GameStartEvent context)
		{
			UIManager.Instance.CloseAllWindows();
			UIManager.Instance.ShowHUD<InGameHud>();
		}

		public void OnMoveLobbyScene()
		{
			UIManager.Instance.HideCurrentHUD();
			UIManager.Instance.CloseAllWindows();

			var lobbyHUD = UIManager.Instance.ShowHUD<LobbyHUD>();
			lobbyHUD.Initialize();
		}

		public void OnMoveTitleScene()
		{
			UIManager.Instance.HideCurrentHUD();
			UIManager.Instance.CloseAllWindows();

			var onlineModeWindow = UIManager.Instance.ShowWindow<OnlineModeWindow>();
			onlineModeWindow.Initialize();
		}

		public void SelectImposters()
        {
			int imposterCount = LobbyManager.GetImposterCount();

			var playerDataDicts = PlayerDataManager.Instance.PlayerDataDicts;

			var suffleClientKeys = playerDataDicts.Keys.OrderBy(x => Random.value).ToList();

            for (int i = 0; i < imposterCount; i++)
            {
				var clientId = suffleClientKeys[i];
				var playerData = playerDataDicts[clientId];
				playerData.PlayerType = EPlayerType.Imposter;

				EventBus.Publish<PlayerData>(playerData);
			}
        }

		public async UniTask StartSession()
		{
			Debug.Log(">> Start: Init Session");
			List<UniTask> tasks = new();

			tasks.Add(MapService.LoadMap(0));

			await tasks;
			SessionManager.Instance.NotifyPlayerLoadedServerRpc(NetworkManager.Singleton.LocalClientId);
			Debug.Log("<< Finished: Init Session");
		}
	}
}
