using NetFrame.Dataframe;
using NetFrame.WriteAndRead;

namespace ExamplesNew.Dataframes
{
    public struct OneTestDataframe : INetworkDataframe
    {
        public byte TestByte;
        public string TestString;
        public bool TestBool;
        
        public void Write(NetFrameWriter writer)
        {
            writer.WriteByte(TestByte);
            writer.WriteString(TestString);
            writer.WriteBool(TestBool);
        }

        public void Read(NetFrameReader reader)
        {
            TestByte = reader.ReadByte();
            TestString = reader.ReadString();
            TestBool = reader.ReadBool();
        }
    }
}