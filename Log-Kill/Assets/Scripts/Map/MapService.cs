using Cysharp.Threading.Tasks;
using LogKill.Core;
using UnityEngine;

namespace LogKill.Map
{
    public class MapService : IService
    {
        private ResourceManager ResourceManager => ServiceLocator.Get<ResourceManager>();
        private readonly string _mapPrefabName = "Assets/Prefabs/Map/WorldMap.prefab";

        private IWorldMap _worldMap;

        public void Initialize()
        {
        }

        public async UniTask LoadMap(int mapIndex)
        {
            var map = await ResourceManager.CreateAsset(_mapPrefabName);
            _worldMap = map.GetComponent<IWorldMap>();
            _worldMap.Initialize();
            Debug.Log("<< Finished: Load Map");
        }

        public void Dispose()
        {
            if (_worldMap != null)
            {
                _worldMap.Dispose();
                _worldMap = null;

                ResourceManager.UnloadAsset(_mapPrefabName);
            }
        }
    }
}