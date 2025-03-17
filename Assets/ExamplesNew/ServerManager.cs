using System;
using System.Reflection;
using ENet;
using ExamplesNew.Dataframes;
using NetFrame.Core;
using NetFrame.Dataframe;
using NetFrame.Enums;
using UnityEngine;

namespace ExamplesNew
{
    public class ServerManager : MonoBehaviour
    {
        private NetFrameServerNew _netFrameServerNew;
        
        private void Awake()
        {
            NetFrameDataframeCollection.Initialize(Assembly.GetExecutingAssembly());
            
            _netFrameServerNew = new NetFrameServerNew(1024);
            _netFrameServerNew.Start(8080, 10);

            _netFrameServerNew.ClientConnection += OnClientConnection;
            _netFrameServerNew.ClientDisconnect += OnClientDisconnect;
            _netFrameServerNew.LogCall += OnLogCall;
            _netFrameServerNew.Subscribe<OneTestDataframe>(OneTestDataframeHandler);
        }

        private void OneTestDataframeHandler(OneTestDataframe dataframe, uint id)
        {
            Debug.Log($"clientID: {id} | TestByte: {dataframe.TestByte} | TestBool: {dataframe.TestBool} | TestString: {dataframe.TestString}");
        }

        private void Update()
        {
            _netFrameServerNew.Run(0);

            if (Input.GetKeyDown(KeyCode.S))
            {
                var oneTestDataframe = new OneTestDataframe
                {
                    TestByte = 244,
                    TestString = "Привет это сообщение с сервера",
                    TestBool = false,
                };
                _netFrameServerNew.Send(ref oneTestDataframe, 0);
            }
        }

        private void OnDestroy()
        {
            _netFrameServerNew.Unsubscribe<OneTestDataframe>(OneTestDataframeHandler);
            
            _netFrameServerNew.Stop();
            _netFrameServerNew.ClientConnection -= OnClientConnection;
            _netFrameServerNew.ClientDisconnect -= OnClientDisconnect;
            _netFrameServerNew.LogCall -= OnLogCall;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Application.runInBackground = true;
            }
        }
        
        private void OnClientConnection(Peer peer)
        {
            Debug.Log($"Client connection: {peer.ID}");
        }
        
        private void OnClientDisconnect(Peer peer)
        {
            Debug.Log($"Client disconnection: {peer.ID}");
        }
        
        private void OnLogCall(NetworkLogType logType, string value)
        {
            switch (logType)
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
    }
}