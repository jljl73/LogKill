using UnityEngine;

namespace LogKill.Mission
{
    public class MissionData : ScriptableObject
    {
        [field: SerializeField]
        public string MissionPrefabName { get; private set; }
    }
}
