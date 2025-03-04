using NetFrame.Unity.Interpolation;
using UnityEngine;

namespace NetFrame.Unity
{
    public interface INetworkDataframeTransform : ISnapshot//, INetworkDataframe
    {
        public int Id { get; set; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
    }
}