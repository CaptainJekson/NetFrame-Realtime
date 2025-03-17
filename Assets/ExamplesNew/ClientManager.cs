using System.Reflection;
using System.Threading;
using ENet;
using ExamplesNew.Dataframes;
using NetFrame.Core;
using NetFrame.Dataframe;
using NetFrame.Enums;
using UnityEngine;

namespace ExamplesNew
{
    public class ClientManager : MonoBehaviour
    {
        private NetFrameClientNew _netFrameClientNew;
        
        private void Awake()
        {
            NetFrameDataframeCollection.Initialize(Assembly.GetExecutingAssembly());
            
            _netFrameClientNew = new NetFrameClientNew(2048);
            _netFrameClientNew.Connect("127.0.0.1", 8080);

            _netFrameClientNew.ConnectionSuccessful += OnConnectionSuccessful;
            _netFrameClientNew.Disconnected += OnDisconnected;
            _netFrameClientNew.ConnectionFailed += ConnectionFailed;
            _netFrameClientNew.LogCall += OnLogCall;
            _netFrameClientNew.Subscribe<OneTestDataframe>(OneTestDataframeHandler);
        }

        private void OneTestDataframeHandler(OneTestDataframe dataframe)
        {
            Debug.Log($"current thread id: {Thread.CurrentThread.ManagedThreadId}");
            Debug.Log($"TestByte: {dataframe.TestByte} | TestBool: {dataframe.TestBool} | TestString: {dataframe.TestString}");
        }

        private void Update()
        {
            _netFrameClientNew.Run(0);

            if (Input.GetKeyDown(KeyCode.S))
            {
                var oneTestDataframe = new OneTestDataframe
                {
                    TestByte = 12,
                    TestString = "Привет это сообщение с клиента",
                    TestBool = true,
                };
                _netFrameClientNew.Send(ref oneTestDataframe);
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                _netFrameClientNew.Disconnect();
            }
        }

        private void OnDestroy()
        {
            _netFrameClientNew.Unsubscribe<OneTestDataframe>(OneTestDataframeHandler);
            
            _netFrameClientNew.Disconnect();
            _netFrameClientNew.ConnectionSuccessful -= OnConnectionSuccessful;
            _netFrameClientNew.Disconnected -= OnDisconnected;
            _netFrameClientNew.ConnectionFailed -= ConnectionFailed;
            _netFrameClientNew.LogCall -= OnLogCall;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Application.runInBackground = true;
            }
        }
        
        private void OnConnectionSuccessful(Peer peer)
        {
            Debug.Log($"Connection successful, my id: {peer.ID}");
        }
        
        private void OnDisconnected()
        {
            Debug.Log("Disconnected");
        }
        
        private void ConnectionFailed()
        {
            Debug.Log("Connection failed");
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