using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace LogKill.Core
{
    public class ResourceManager : IService
    {
        private Dictionary<string, GameObject> _loadedAssets = new Dictionary<string, GameObject>();

        public void Initialize()
        {
        }

        public async UniTask<GameObject> LoadAsset(string key)
        {
            if (_loadedAssets.ContainsKey(key))
            {
                Debug.Log($"이미 로드된 리소스: {key}");
                return _loadedAssets[key];
            }

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(key);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedAssets[key] = handle.Result;
                Debug.Log($"로드 성공: {key}");
                return handle.Result;
            }
            else
            {
                Debug.LogError($"로드 실패: {key}");
            }

            return null;
        }

        public void UnloadAsset(string key)
        {
            if (_loadedAssets.ContainsKey(key))
            {
                Addressables.Release(_loadedAssets[key]);
                _loadedAssets.Remove(key);
                Debug.Log($"언로드 완료: {key}");
            }
        }

        public async UniTask<GameObject> CreateAsset(string key)
        {
            var handle = await Addressables.InstantiateAsync(key);
            return handle;
        }
    }

}
