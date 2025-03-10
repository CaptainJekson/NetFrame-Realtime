using NetFrame.Dataframe;
using NetFrame.WriteAndRead;

namespace Examples.Scripts.Dataframes
{
    public struct PlayerDeSpawnResponse : INetworkDataframe
    {
        public uint Id;
        
        public void Write(NetFrameWriter writer)
        {
            writer.WriteUInt(Id);
        }

        public void Read(NetFrameReader reader)
        {
            Id = reader.ReadUInt();
        }
    }
}