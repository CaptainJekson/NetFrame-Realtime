using NetFrame;
using NetFrame.Unity;
using NetFrame.Unity.UnityTypesWriteAndRead;
using NetFrame.WriteAndRead;
using UnityEngine;

namespace Examples.Scripts.Dataframes
{
    public struct PlayerMoveDataframe : INetworkDataframeTransform
    {
        public uint Id { get; set; }
        public double RemoteTime { get; set; }
        public double LocalTime { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        
        public void Write(NetFrameWriter writer)
        {
            writer.WriteUInt(Id);
            writer.WriteDouble(RemoteTime);
            writer.WriteDouble(LocalTime);
            writer.WriteVector3(Position);
            writer.WriteQuaternion(Rotation);
        }

        public void Read(NetFrameReader reader)
        {
            Id = reader.ReadUInt();
            RemoteTime = reader.ReadDouble();
            LocalTime = reader.ReadDouble();
            Position = reader.ReadVector3();
            Rotation = reader.ReadQuaternion();
        }
    }
}