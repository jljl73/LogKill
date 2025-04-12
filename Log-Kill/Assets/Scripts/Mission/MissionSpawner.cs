using System.Collections.Generic;
using UnityEngine;

namespace LogKill.Mission
{
    public class MissionSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _missionPrefab;
        [SerializeField] private List<Transform> _missionSpawnPoint = new();

        private List<GameObject> _spawnedMissions = new List<GameObject>();

        private void Start()
        {
            SpawnMission();
        }

        private void SpawnMission(int count = 5)
        {
            Shuffle();

            for (int i = 0; i < count; i++)
            {
                var spawnPoint = _missionSpawnPoint[i];
                var mission = Instantiate(_missionPrefab, spawnPoint.position, Quaternion.identity);
                mission.transform.SetParent(spawnPoint);
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

        private void DespawnMission()
        {
            foreach (var mission in _spawnedMissions)
            {
                Destroy(mission);
            }
            _spawnedMissions.Clear();
        }
    }
}
