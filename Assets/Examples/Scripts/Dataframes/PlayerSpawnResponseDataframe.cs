using NetFrame;
using NetFrame.UnityTypesWriteAndRead;
using NetFrame.WriteAndRead;
using UnityEngine;

namespace Examples.Scripts.Dataframes
{
    public struct PlayerSpawnResponseDataframe : INetworkDataframe
    {
        public bool IsLocal;
        public int Id;
        public Vector3 StartPosition;
        public Quaternion StartRotation;
        
        public void Write(NetFrameWriter writer)
        {
            writer.WriteBool(IsLocal);
            writer.WriteInt(Id);
            writer.WriteVector3(StartPosition);
            writer.WriteQuaternion(StartRotation);
        }

        public void Read(NetFrameReader reader)
        {
            IsLocal = reader.ReadBool();
            Id = reader.ReadInt();
            StartPosition = reader.ReadVector3();
            StartRotation = reader.ReadQuaternion();
        }
    }
}