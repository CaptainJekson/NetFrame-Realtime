using System.Collections.Generic;
using System.Reflection;
using Examples.Scripts.Dataframes;
using NetFrame.Enums;
using NetFrame.Server;
using NetFrame.Utils;
using Samples.DataframesForRealtime;
using UnityEngine;

namespace Examples.Scripts.Managers
{
    public class ServerRealTimeManager : MonoBehaviour
    {
        private NetFrameServer _netFrameServer;

        private Dictionary<int, Vector3> _players = new();

        private void Start()
        {
            NetFrameDataframeCollection.Initialize(Assembly.GetExecutingAssembly());
            _netFrameServer = new NetFrameServer(2000);
            
            _netFrameServer.Start(8080, 10);

            _netFrameServer.ClientConnection += OnClientConnection;
            _netFrameServer.ClientDisconnect += OnClientDisconnect;
            _netFrameServer.LogCall += OnLog;
            
            _netFrameServer.Subscribe<PlayerSpawnDataframe>(PlayerSpawnRemoteRequestDataframeHandler);
            _netFrameServer.Subscribe<PlayerMoveDataframe>(PlayerMoveDataframeHandler);
        }

        private void Update()
        {
            _netFrameServer.Run(100);
        }

        private void PlayerSpawnRemoteRequestDataframeHandler(PlayerSpawnDataframe dataframe, int id)
        {
            
            _netFrameServer.SendAllExcept(ref dataframe, id);

            foreach (var player in _players)
            {
                var dataframe2 = new PlayerSpawnDataframe
                {
                    StartPosition = player.Value,
                };
                
                _netFrameServer.Send(ref dataframe2, id);
            }
            
            _players.Add(id, dataframe.StartPosition);
        }
        
        private void PlayerMoveDataframeHandler(PlayerMoveDataframe dataframe, int id)
        {
            _netFrameServer.SendAllExcept(ref dataframe, id);

            _players[id] = dataframe.Position;
        }

        private void OnClientConnection(int id)
        {
            Debug.Log($"client connected Id = {id}");
        }
        
        private void OnClientDisconnect(int id)
        {
            Debug.Log($"client disconnected Id = {id}");
        }
        
        private void OnLog(NetworkLogType reason, string value)
        {
            switch (reason)
            {
                case NetworkLogType.Info:
                    Debug.Log(value);
                    break;
                case NetworkLogType.Warning:
                    Debug.LogWarning(value);
                    break;
                case NetworkLogType.Error:
                    Debug.LogError(value);
                    break;
            }
        }

        private void OnApplicationQuit()
        {
            _netFrameServer.Unsubscribe<PlayerSpawnDataframe>(PlayerSpawnRemoteRequestDataframeHandler);
            _netFrameServer.Unsubscribe<PlayerMoveDataframe>(PlayerMoveDataframeHandler);
            
            _netFrameServer.Stop();
        }
    }
}