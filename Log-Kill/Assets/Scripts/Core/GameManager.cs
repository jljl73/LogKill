using LogKill.Core;
using LogKill.Mission;
using LogKill.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace LogKill
{
	public class GameManager : MonoSingleton<GameManager>
	{
		[SerializeField] private MissionData _missionData;

		private void Start()
		{
			ServiceLocator.AutoRegisterServices();
			UIManager.Instance.InitializeAsync();
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
