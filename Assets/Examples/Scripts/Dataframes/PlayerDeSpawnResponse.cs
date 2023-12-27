using NetFrame;
using NetFrame.WriteAndRead;

namespace Examples.Scripts.Dataframes
{
    public struct PlayerDeSpawnResponse : INetworkDataframe
    {
        public int Id;
        
        public void Write(NetFrameWriter writer)
        {
            writer.WriteInt(Id);
        }

        public void Read(NetFrameReader reader)
        {
            Id = reader.ReadInt();
        }
    }
}