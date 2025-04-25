using System.Collections.Generic;
using LogKill.Core;
using LogKill.Entity;
using LogKill.Event;
using UnityEngine;

namespace LogKill.Mission
{
    public class MissionSpawner : MonoBehaviour
    {
        [SerializeField] private List<BatteryEntity> _spawnedMissions = new();

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public void Initialize()
        {
            EventBus.Subscribe<MissionStartEvent>(OnMissionStartEvent);

            foreach (var mission in _spawnedMissions)
            {
                mission.gameObject.SetActive(false);
            }
        }

        public void Dipose()
        {
            EventBus.Unsubscribe<MissionStartEvent>(OnMissionStartEvent);
        }

        public void StartRandomSpawnMissions(int count = 5)
        {
            Shuffle();

            for (int i = 0; i < count; i++)
            {
                _spawnedMissions[i].gameObject.SetActive(true);
            }
        }

        private void RandomSpawnMission()
        {
            var activeSpawnMissions = _spawnedMissions.FindAll(obj => !obj.gameObject.activeSelf);

            int randomIndex = Random.Range(0, activeSpawnMissions.Count);

            activeSpawnMissions[randomIndex].gameObject.SetActive(true);
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

        private void OnMissionStartEvent(MissionStartEvent context)
        {
            RandomSpawnMission();
        }
    }
}
