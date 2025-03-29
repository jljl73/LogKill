using Cysharp.Threading.Tasks;
using LogKill.Core;
using UnityEngine;

namespace LogKill.Map
{
    public class MapService : IService
    {
        private ResourceManager ResourceManager => ServiceLocator.Get<ResourceManager>();
        private readonly string _mapPrefabName = "Assets/Prefabs/Map/WorldMap.prefab";

        public void Initialize()
        {
        }

        public async UniTask LoadMap(int mapIndex)
        {
            await ResourceManager.CreateAsset(_mapPrefabName);
            Debug.Log("<< Finished: Load Map");
        }
    }
}