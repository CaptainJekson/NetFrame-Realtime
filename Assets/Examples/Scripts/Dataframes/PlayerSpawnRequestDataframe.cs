using NetFrame;
using NetFrame.Dataframe;
using NetFrame.Unity.UnityTypesWriteAndRead;
using NetFrame.WriteAndRead;
using UnityEngine;

namespace Examples.Scripts.Dataframes
{
    public struct PlayerSpawnRequestDataframe : INetworkDataframe
    {
        public Vector3 StartPosition;
        public Quaternion StartRotation;
        
        public void Write(NetFrameWriter writer)
        {
            writer.WriteVector3(StartPosition);
            writer.WriteQuaternion(StartRotation);
        }

        public void Read(NetFrameReader reader)
        {
            StartPosition = reader.ReadVector3();
            StartRotation = reader.ReadQuaternion();
        }
    }
}