using System.Collections.Generic;
using System.Reflection;
using ENet;
using Examples.Scripts.Model;
using NetFrame.Core;
using NetFrame.Dataframe;
using UnityEngine;

namespace Examples.Scripts.Managers
{
    public class ServerRealTimeManager : MonoBehaviour
    {
        private NetFrameServerNew _netFrameServer;
        
        private Dictionary<int, PlayerModel> _players;
        
        public NetFrameServerNew Server => _netFrameServer;
        
        private void Awake()
        {
            _players = new Dictionary<int, PlayerModel>();
            
            NetFrameDataframeCollection.Initialize(Assembly.GetExecutingAssembly());
            _netFrameServer = new NetFrameServerNew(2048);
            
            _netFrameServer.Start(8080, 10);
        
            _netFrameServer.ClientConnection += OnClientConnection;
            _netFrameServer.ClientDisconnect += OnClientDisconnect;
            //_netFrameServer.LogCall += OnLog;
        }
        
        private void Update()
        {
            _netFrameServer.Run(100);
        }
        
        private void OnClientConnection(Peer peer)
        {
            Debug.Log($"client connected Id = {peer.ID}");
        }
        
        private void OnClientDisconnect(Peer peer)
        {
            Debug.Log($"client disconnected Id = {peer.ID}");
        }
        
        // private void OnLog(NetworkLogType reason, string value)
        // {
        //     switch (reason)
        //     {
        //         case NetworkLogType.Info:
        //             Debug.Log(value);
        //             break;
        //         case NetworkLogType.Warning:
        //             Debug.LogWarning(value);
        //             break;
        //         case NetworkLogType.Error:
        //             Debug.LogError(value);
        //             break;
        //     }
        // }
        
        private void OnApplicationQuit()
        {
            _netFrameServer.Stop();
        }
    }
}