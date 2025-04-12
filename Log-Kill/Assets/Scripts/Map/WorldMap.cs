using UnityEngine;
using LogKill.Mission;

namespace LogKill.Map
{
    public interface IWorldMap
    {
        void Initialize();
        void Dispose();
    }

    public class WorldMap : MonoBehaviour, IWorldMap
    {
        [SerializeField] private MissionSpawner _missionSpawner;

        public void Initialize()
        {
            _missionSpawner.SpawnMission();
        }

        public void Dispose()
        {
            _missionSpawner.DespawnMission();
        }
    }
}
