using UnityEngine;
using LogKill.UI;
using LogKill.Core;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace LogKill.Mission
{
    public class MissionWindow : WindowBase
    {
        private MissionData _missionData;
        private Dictionary<MissionData, IMisison> _missions = new();
        private ResourceManager ResourceManager => ServiceLocator.Get<ResourceManager>();

        public async void StartMission(MissionData mission)
        {
            _missionData = mission;
            if (!_missions.ContainsKey(mission))
                await CreateMission(mission);

            _missions[mission].StartMission();
        }

        private async UniTask CreateMission(MissionData missionData)
        {
            var prefab = await ResourceManager.LoadAsset(missionData.MissionPrefabName);
            var mission = Instantiate(prefab, transform).GetComponent<IMisison>();
            mission.Initialize(missionData.MissionId);

            _missions.Add(missionData, mission);
        }
    }
}
