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

    //public class PlayerData
    //{
    //    public EPlayerType PlayerType;
    //    public EColorType ColorType;
    //    public string Name;
    //    public bool IsDead;

    //    public void Initialize(string name)
    //    {
    //        PlayerType = EPlayerType.Normal;
    //        ColorType = EColorType.Red;
    //        Name = name;
    //        IsDead = false;
    //    }
    //}

    public struct PlayerData : INetworkSerializable
    {
        public ulong ClientId;
        public EPlayerType PlayerType;
        public EColorType ColorType;
        public FixedString32Bytes Name;
        public bool IsDead;

        public PlayerData(ulong id, string name, bool isDead = false)
        {
            ClientId = id;
            PlayerType = EPlayerType.Normal;
            ColorType = (EColorType)id;
            Name = name;
            IsDead = isDead;
        }

        //public void Initialize(string name)
        //{
        //    ClientId = NetworkManager.Singleton.LocalClientId;
        //    PlayerType = EPlayerType.Normal;
        //    ColorType = EColorType.White;
        //    Name = name;
        //    IsDead = false;
        //}

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
