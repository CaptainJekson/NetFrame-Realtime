using System.Collections.Generic;
using System.Reflection;
using Examples.Scripts.Model;
//using NetFrame.Enums;
//using NetFrame.Server;
//using NetFrame.Utils;
using UnityEngine;

namespace Examples.Scripts.Managers
{
    public class ServerRealTimeManager : MonoBehaviour
    {
        // private NetFrameServer _netFrameServer;
        //
        // private Dictionary<int, PlayerModel> _players;
        //
        // public NetFrameServer Server => _netFrameServer;
        //
        // private void Awake()
        // {
        //     _players = new Dictionary<int, PlayerModel>();
        //     
        //     NetFrameDataframeCollection.Initialize(Assembly.GetExecutingAssembly());
        //     _netFrameServer = new NetFrameServer(2000);
        //     
        //     _netFrameServer.Start(8080, 10);
        //
        //     _netFrameServer.ClientConnection += OnClientConnection;
        //     _netFrameServer.ClientDisconnect += OnClientDisconnect;
        //     _netFrameServer.LogCall += OnLog;
        // }
        //
        // private void Update()
        // {
        //     _netFrameServer.Run(100);
        // }
        //
        // private void OnClientConnection(int id)
        // {
        //     Debug.Log($"client connected Id = {id}");
        // }
        //
        // private void OnClientDisconnect(int id)
        // {
        //     Debug.Log($"client disconnected Id = {id}");
        // }
        //
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
        //
        // private void OnApplicationQuit()
        // {
        //     _netFrameServer.Stop();
        // }
    }
}