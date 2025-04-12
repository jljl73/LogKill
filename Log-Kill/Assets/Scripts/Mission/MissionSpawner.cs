using System.Collections.Generic;
using LogKill.Entity;
using UnityEngine;

namespace LogKill.Mission
{
    public class MissionSpawner : MonoBehaviour
    {
        [SerializeField] private BatteryEntity _missionPrefab;
        [SerializeField] private List<MissionData> _missionData = new();
        [SerializeField] private List<Transform> _missionSpawnPoint = new();

        private List<BatteryEntity> _spawnedMissions = new ();
        
        public void SpawnMission(int count = 5)
        {
            Shuffle();

            for (int i = 0; i < count; i++)
            {
                var spawnPoint = _missionSpawnPoint[i];
                var mission = Instantiate(_missionPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
                mission.Initialize(_missionData[i]);
                _spawnedMissions.Add(mission);
            }
        }

        private void Shuffle()
        {
            var random = new System.Random();
            for (int i = _spawnedMissions.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                var temp = _spawnedMissions[i];
                _spawnedMissions[i] = _spawnedMissions[j];
                _spawnedMissions[j] = temp;
            }
        }

        public void DespawnMission()
        {
            foreach (var mission in _spawnedMissions)
            {
                Destroy(mission);
            }
            _spawnedMissions.Clear();
        }
    }
}
