using LogKill.Character;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public struct VoteData : INetworkSerializable
    {
        public PlayerData PlayerData;
        public string LogMessage;

        public VoteData(PlayerData playerData, string logMessage)
        {
            PlayerData = playerData;
            LogMessage = logMessage;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerData);
            serializer.SerializeValue(ref LogMessage);
        }
    }
}
