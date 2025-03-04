using ENet;
using NetFrame.Core;
using UnityEngine;

namespace ExamplesNew
{
    public class ClientManager : MonoBehaviour
    {
        private NetFrameClientNew _netFrameClientNew;
        
        private void Awake()
        {
            _netFrameClientNew = new NetFrameClientNew();
            _netFrameClientNew.Connect("127.0.0.1", 8080);

            _netFrameClientNew.ConnectionSuccessful += OnConnectionSuccessful;
            _netFrameClientNew.Disconnected += OnDisconnected;
            _netFrameClientNew.ConnectionFailed += ConnectionFailed;
        }

        private void Update()
        {
            _netFrameClientNew.Run(0);

            if (Input.GetKeyDown(KeyCode.S))
            {
                _netFrameClientNew.SendTest();
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                _netFrameClientNew.Disconnect();
            }
        }

        private void OnDestroy()
        {
            _netFrameClientNew.Disconnect();
            _netFrameClientNew.ConnectionSuccessful -= OnConnectionSuccessful;
            _netFrameClientNew.Disconnected -= OnDisconnected;
            _netFrameClientNew.ConnectionFailed -= ConnectionFailed;
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
    }
}