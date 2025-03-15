using LogKill.Core;
using LogKill.UI;
using UnityEngine;

namespace LogKill
{
	public class GameManager : MonoSingleton<GameManager>
	{
		private void Start()
		{
			ServiceLocator.AutoRegisterServices();
			UIManager.Instance.InitializeAsync();
		}
	}
}
