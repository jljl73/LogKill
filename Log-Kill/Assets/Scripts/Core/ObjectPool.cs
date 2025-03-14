using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LogKill.Core
{
    public class ObjectPool : IService
    {
        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, AsyncOperationHandle<GameObject>> handleDictionary = new Dictionary<string, AsyncOperationHandle<GameObject>>();

        public void Initialize()
        {
        }

        public void PreloadObject(string key, int count)
        {
            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary[key] = new Queue<GameObject>();

                Addressables.LoadAssetAsync<GameObject>(key).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        handleDictionary[key] = handle;
                        for (int i = 0; i < count; i++)
                        {
                            GameObject obj = Object.Instantiate(handle.Result);
                            obj.SetActive(false);
                            poolDictionary[key].Enqueue(obj);
                        }
                    }
                };
            }
        }

        /// <summary>
        /// 풀에서 오브젝트 가져오기
        /// </summary>
        public GameObject GetObject(string key)
        {
            if (poolDictionary.ContainsKey(key) && poolDictionary[key].Count > 0)
            {
                GameObject obj = poolDictionary[key].Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else if (handleDictionary.ContainsKey(key))  // 풀에 없으면 새로 생성
            {
                GameObject obj = Object.Instantiate(handleDictionary[key].Result);
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 오브젝트 반환
        /// </summary>
        public void ReturnObject(string key, GameObject obj)
        {
            obj.SetActive(false);
            poolDictionary[key].Enqueue(obj);
        }

        /// <summary>
        /// Addressables 리소스 해제
        /// </summary>
        public void ReleaseAddressables(string key)
        {
            if (handleDictionary.ContainsKey(key))
            {
                Addressables.Release(handleDictionary[key]);
                handleDictionary.Remove(key);
                poolDictionary.Remove(key);
            }
        }

    }

}
