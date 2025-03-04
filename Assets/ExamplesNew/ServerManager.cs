using ENet;
using NetFrame.Core;
using UnityEngine;

namespace ExamplesNew
{
    public class ServerManager : MonoBehaviour
    {
        private NetFrameServerNew _netFrameServerNew;
        
        private void Awake()
        {
            _netFrameServerNew = new NetFrameServerNew();
            _netFrameServerNew.Start(8080, 10);

            _netFrameServerNew.ClientConnection += OnClientConnection;
            _netFrameServerNew.ClientDisconnect += OnClientDisconnect;
        }

        private void Update()
        {
            _netFrameServerNew.Run(0);

            if (Input.GetKeyDown(KeyCode.S))
            {
                _netFrameServerNew.SendAllTest();
            }
        }

        private void OnDestroy()
        {
            _netFrameServerNew.Stop();
            _netFrameServerNew.ClientConnection -= OnClientConnection;
            _netFrameServerNew.ClientDisconnect -= OnClientDisconnect;
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
    }
}