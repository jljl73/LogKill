using UnityEngine;

namespace LogKill
{
    [CreateAssetMenu(fileName = "LobbyListData", menuName = "Scriptable Objects/LobbyListData")]
    public class LobbyListData : ScriptableObject
    {
        [field: SerializeField] public int MaxLobbyListCount { get; private set; }
        [field: SerializeField] public string LobbyListItemPrefabName { get; private set; }
    }
}
