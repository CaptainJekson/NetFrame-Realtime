using System.Reflection;
using ENet;
using NetFrame.Core;
using NetFrame.Dataframe;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Scripts.Managers
{
    public class ClientRealtimeManager : MonoBehaviour
    {
        [Header("IpAddress")]
        [SerializeField] private string ipAddress; //"127.0.0.1" //"192.168.31.103"
        
        [Header("Buttons")] 
        [SerializeField] private Button connectButton;
        [SerializeField] private Button disconnectButton;
        
        
        private NetFrameClientNew _netFrameClient;
        
        public NetFrameClientNew Client => _netFrameClient;
        
        private void Awake()
        {
            NetFrameDataframeCollection.Initialize(Assembly.GetExecutingAssembly());
            
            _netFrameClient = new NetFrameClientNew(2048);
        
            _netFrameClient.ConnectionSuccessful += OnConnectionSuccessful;
            //_netFrameClient.LogCall += OnLog;
            _netFrameClient.Disconnected += OnDisconnected;
            
            connectButton.onClick.AddListener(() =>
            {
                _netFrameClient.Connect(ipAddress, 8080);
            });
            
            disconnectButton.onClick.AddListener(() =>
            {
                _netFrameClient.Disconnect();
            });
        }
        
        private void Update()
        {
            _netFrameClient.Run(100);
        }
        
        private void OnDisconnected()
        {
            Debug.Log("Disconnected from the server");
        }
        
        private void OnConnectionSuccessful(Peer peer)
        {
            Debug.Log("Connected Successful to server");
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
            _netFrameClient.ConnectionSuccessful -= OnConnectionSuccessful;
            //_netFrameClient.LogCall -= OnLog;
            _netFrameClient.Disconnected -= OnDisconnected;
        
            _netFrameClient.Disconnect();
        }
    }
}