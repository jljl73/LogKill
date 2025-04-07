using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public enum EPlayerType
    {
        Normal,
        Imposter,
    }

    public enum EColorType
    { 
        White,
        Red,
        Blue,
        Green,
        Yellow,
        Purple
    }

    public struct PlayerData : INetworkSerializable
    {
        public ulong ClientId;
        public EPlayerType PlayerType;
        public EColorType ColorType;
        public FixedString32Bytes Name;
        public bool IsDead;

        public void Initialize(EColorType colorType, string name)
        {
            ClientId = NetworkManager.Singleton.LocalClientId;
            PlayerType = EPlayerType.Normal;
            ColorType = colorType;
            Name = name;
            IsDead = false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerType);
            serializer.SerializeValue(ref ColorType);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref IsDead);
        }

        public Color GetColor()
        {
            switch (ColorType)
            {
                case EColorType.White:
                    return Color.white;
                case EColorType.Red:
                    return Color.red;
                case EColorType.Blue:
                    return Color.blue;
                case EColorType.Green:
                    return Color.green;
                case EColorType.Yellow:
                    return Color.yellow;
                case EColorType.Purple:
                    return new Color(1f, 1f, 0f);
                default:
                    return Color.clear;
            }
        }
    }
}
