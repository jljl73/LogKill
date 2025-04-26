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
        Purple,
        Gray,
        Pink,
        Orange,
        Cyan,
    }

    public struct PlayerData : INetworkSerializable
    {
        public ulong ClientId;
        public EPlayerType PlayerType;
        public EColorType ColorType;
        public FixedString32Bytes Name;
        public bool IsDead;

        public PlayerData(ulong id, EColorType colorType, string name, bool isDead = false)
        {
            ClientId = id;
            PlayerType = EPlayerType.Normal;
            ColorType = colorType;
            Name = name;
            IsDead = isDead;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerType);
            serializer.SerializeValue(ref ColorType);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref IsDead);
        }

        public bool GetIsImposter()
        {
            return PlayerType == EPlayerType.Imposter;
        }
    }
}
