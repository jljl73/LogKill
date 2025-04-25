using UnityEngine;
using LogKill.Mission;
using LogKill.Item;

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
        [SerializeField] private BatterySpawner _batterySpawner;

        public void Initialize()
        {
            _missionSpawner.Initialize();
            _missionSpawner.StartRandomSpawnMissions();

            _batterySpawner.Initialize();
            _batterySpawner.Spawn();
        }

        public void Dispose()
        {
            _missionSpawner.Dipose();
            _batterySpawner.Dipose();
        }
    }
}
