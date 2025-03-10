using NetFrame.Dataframe;
using NetFrame.Unity.Interpolation;
using UnityEngine;

namespace NetFrame.Unity
{
    public interface INetworkDataframeTransform : ISnapshot, INetworkDataframe
    {
        public uint Id { get; set; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
    }
}