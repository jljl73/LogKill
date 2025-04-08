using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.LobbySystem;
using LogKill.Map;
using LogKill.Mission;
using LogKill.Room;
using LogKill.UI;
using LogKill.Vote;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill
{
    public class GameManager : MonoSingleton<GameManager>
	{
		[SerializeField] private MissionData _missionData;

		private MapService MapService => ServiceLocator.Get<MapService>();

		private async UniTask Start()
		{
			ServiceLocator.AutoRegisterServices();

			await UIManager.Instance.InitializeAsync();
			await LobbyManager.Instance.InitializeAsync();

            var onlineModeWindow = UIManager.Instance.ShowWindow<OnlineModeWindow>();
			onlineModeWindow.Initialize();
		}

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
				VoteManager.Instance.OnStartVotingServerRpc();
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
