using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LogKill.Core;
using UnityEngine;

namespace LogKill.UI
{
	public class UIManager : MonoSingleton<UIManager>
	{
		[SerializeField] private Transform _windowLayer;
		[SerializeField] private Transform _hudLayer;

		[SerializeField] private string[] _windowPrefabPaths;
		[SerializeField] private string[] _hudPrefabPaths;

		private WindowBase _currentWindow;
		private HUDBase _currentHUD;

		private readonly Stack<WindowBase> _windowHistory = new Stack<WindowBase>();
		private readonly Queue<Type> _windowQueue = new Queue<Type>();
		private readonly Dictionary<Type, WindowBase> _windowCache = new Dictionary<Type, WindowBase>();
		private readonly Dictionary<Type, HUDBase> _hudCache = new Dictionary<Type, HUDBase>();

		private bool _isInitialized = false;
		public bool IsInitialized => _isInitialized;

		private float _initializationProgress = 0f;
		public float InitializationProgress => _initializationProgress;

		private ResourceManager _resourceManager;
		public ResourceManager ResourceManager => _resourceManager ??= ServiceLocator.Get<ResourceManager>();

		public async UniTask InitializeAsync(IProgress<float> progress = null)
		{
			if (_isInitialized)
			{
				Debug.LogWarning("UIManager is already initialized.");
				return;
			}

			Debug.Log("Starting UI initialization with UniTask...");
			_initializationProgress = 0f;

			int totalItems = _windowPrefabPaths.Length + _hudPrefabPaths.Length;
			int processedItems = 0;

			foreach (string path in _windowPrefabPaths)
			{
				try
				{
					var asset = await ResourceManager.LoadAsset(path);
					// Ensure the prefab is inactive before instantiation
					asset.SetActive(false);
					WindowBase prefab = asset.GetComponent<WindowBase>();
					if (prefab != null)
					{
						WindowBase instance = Instantiate(prefab, _windowLayer);
						Type windowType = instance.GetType();

						if (!_windowCache.ContainsKey(windowType))
						{
							_windowCache.Add(windowType, instance);
							await instance.InitializeAsync();
							instance.Hide();
							Debug.Log($"Window loaded and initialized: {windowType.Name}");
						}
						else
						{
							Debug.LogWarning($"Duplicate window type: {windowType.Name}");
							Destroy(instance.gameObject);
						}
					}
					else
					{
						Debug.LogError($"Failed to load window prefab at path: {path}");
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"Error loading window prefab at path {path}: {ex.Message}");
				}

				processedItems++;
				_initializationProgress = (float)processedItems / totalItems;
				progress?.Report(_initializationProgress);

				await UniTask.Yield();
			}

			foreach (string path in _hudPrefabPaths)
			{
				try
				{
					var asset = await ResourceManager.LoadAsset(path);
					HUDBase prefab = asset.GetComponent<HUDBase>();
					if (prefab != null)
					{
						HUDBase instance = Instantiate(prefab, _hudLayer);
						Type hudType = instance.GetType();

						if (!_hudCache.ContainsKey(hudType))
						{
							_hudCache.Add(hudType, instance);
							await instance.InitializeAsync();
							instance.Hide();
							Debug.Log($"HUD loaded and initialized: {hudType.Name}");
						}
						else
						{
							Debug.LogWarning($"Duplicate HUD type: {hudType.Name}");
							Destroy(instance.gameObject);
						}
					}
					else
					{
						Debug.LogError($"Failed to load HUD prefab at path: {path}");
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"Error loading HUD prefab at path {path}: {ex.Message}");
				}

				processedItems++;
				_initializationProgress = (float)processedItems / totalItems;
				progress?.Report(_initializationProgress);

				await UniTask.Yield();
			}

			_isInitialized = true;
			_initializationProgress = 1f;
			progress?.Report(1f);

			Debug.Log("UI initialization completed successfully.");
		}

		public void EnqueueWindow<T>() where T : WindowBase
		{
			Type windowType = typeof(T);

			if (!_windowCache.ContainsKey(windowType))
			{
				Debug.LogError($"Window of type {windowType.Name} not found! Cannot enqueue.");
				return;
			}

			_windowQueue.Enqueue(windowType);
			Debug.Log($"Window of type {windowType.Name} enqueued. Queue count: {_windowQueue.Count}");

			if (_currentWindow == null && _windowHistory.Count == 0)
			{
				ShowNextQueuedWindow();
			}
		}

		private void ShowNextQueuedWindow()
		{
			if (_windowQueue.Count == 0)
			{
				Debug.Log("No windows in queue.");
				return;
			}

			Type nextWindowType = _windowQueue.Dequeue();
			WindowBase nextWindow = _windowCache[nextWindowType];

			_currentWindow = nextWindow;
			_currentWindow.Show();

			Debug.Log($"Showing queued window of type {nextWindowType.Name}. Remaining in queue: {_windowQueue.Count}");
		}

		public T ShowWindow<T>(bool remember = true) where T : WindowBase
		{
			Type windowType = typeof(T);

			if (!_windowCache.TryGetValue(windowType, out WindowBase window))
			{
				Debug.LogError($"Window of type {windowType.Name} not found!");
				return null;
			}

			if (_currentWindow != null)
			{
				if (_currentWindow == window)
					return window as T; 

				if (remember && _currentWindow.RememberInHistory)
				{
					_windowHistory.Push(_currentWindow);
				}

				_currentWindow.Hide();
			}

			_currentWindow = window;
			_currentWindow.Show();

			return window as T;
		}

		public void CloseCurrentWindow()
		{
			if (_currentWindow != null)
			{
				_currentWindow.Hide();
				_currentWindow = null;
			}

			if (_windowHistory.Count > 0)
			{
				_currentWindow = _windowHistory.Pop();
				_currentWindow.Show();
			}
			else if (_windowQueue.Count > 0)
			{
				ShowNextQueuedWindow();
			}
		}

		public void CloseAllWindows()
		{
			if (_currentWindow != null)
			{
				_currentWindow.Hide();
				_currentWindow = null;
			}

			_windowHistory.Clear();

			if (_windowQueue.Count > 0)
			{
				ShowNextQueuedWindow();
			}
		}

		public void ClearWindowQueue()
		{
			_windowQueue.Clear();
			Debug.Log("Window queue cleared.");
		}


		public T ShowHUD<T>() where T : HUDBase
		{
			Type hudType = typeof(T);

			if (!_hudCache.TryGetValue(hudType, out HUDBase hud))
			{
				Debug.LogError($"HUD of type {hudType.Name} not found!");
				return null;
			}

			if (_currentHUD != null)
			{
				if (_currentHUD == hud)
					return hud as T;

				_currentHUD.Hide();
			}

			_currentHUD = hud;
			_currentHUD.Show();

			return hud as T;
		}

		public void HideCurrentHUD()
		{
			if (_currentHUD != null)
			{
				_currentHUD.Hide();
				_currentHUD = null;
			}
		}

		public T GetCurrentWindow<T>() where T : WindowBase
		{
			return _currentWindow as T;
		}

		public T GetCurrentHUD<T>() where T : HUDBase
		{
			return _currentHUD as T;
		}

		public int GetWindowQueueCount()
		{
			return _windowQueue.Count;
		}
	}
}
