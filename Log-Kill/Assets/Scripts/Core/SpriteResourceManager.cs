using LogKill.Character;
using UnityEngine;

namespace LogKill
{
    [CreateAssetMenu(fileName = "SpriteResourceManager", menuName = "Scriptable Objects/SpriteResourceManager")]
    public class SpriteResourceManager : ScriptableObject
    {
        [SerializeField] private Sprite[] _playerSprites;

        private static SpriteResourceManager _instance;
        public static SpriteResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SpriteResourceManager>("SpriteResourceManager");
                }
                return _instance;
            }
        }

        public Sprite GetPlayerSprite(EColorType colorType)
        {
            int index = (int)colorType;
            if (index < 0 || index >= _playerSprites.Length)
            {
                Debug.LogError($"Index {index} is out of range for player sprites.");
                return null;
            }
            return _playerSprites[index];
        }
        
    }
}
