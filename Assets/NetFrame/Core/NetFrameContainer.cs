namespace NetFrame.Core
{
    public static class NetFrameContainer
    {
        public static NetFrameClientNew NetFrameClient { get; private set; }
        public static NetFrameServerNew NetFrameServer { get; private set; }
        
        internal static void SetClient(NetFrameClientNew netFrameClient)
        {
            NetFrameClient = netFrameClient;
        }

        internal static void SetServer(NetFrameServerNew netFrameServer)
        {
            NetFrameServer = netFrameServer;
        }
    }
}