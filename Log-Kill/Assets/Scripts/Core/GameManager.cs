using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.LobbySystem;
using LogKill.Mission;
using LogKill.UI;
using UnityEngine;

namespace LogKill
{
	public class GameManager : MonoSingleton<GameManager>
	{
		[SerializeField] private MissionData _missionData;

		private async UniTask Start()
		{
			ServiceLocator.AutoRegisterServices();

			await UIManager.Instance.InitializeAsync();
			await LobbyManager.Instance.InitializeAsync();

            UIManager.Instance.ShowWindow<OnlineModeWindow>();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				var mission = UIManager.Instance.ShowWindow<MissionWindow>();
				mission.InitMission(_missionData);
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				UIManager.Instance.CloseCurrentWindow();
			}
		}
	}
}
