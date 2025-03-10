using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ENet;
using NetFrame.Dataframe;
using UnityEngine;
using EventType = ENet.EventType;

namespace NetFrame.Core
{
    public class NetFrameClientNew
    {
        private Host _client;
        private Peer _clientPeer;
        private Thread _clientThread;
        private int _timeout;
        private bool _isRunning;

        private readonly Queue<Action> _mainThreadActions;
        
        public event Action<Peer> ConnectionSuccessful;
        public event Action Disconnected;
        public event Action ConnectionFailed;

        public NetFrameClientNew(int bufferSize)
        {
            NetFrameContainer.SetClient(this);
            
            _mainThreadActions = new Queue<Action>();
        }

        public void Connect(string ip, ushort port)
        {
            Library.Initialize();

            _client = new Host();
            var address = new Address();
            
            address.SetHost(ip);
            address.Port = port;

            _client.Create();

            _clientPeer = _client.Connect(address);

            _isRunning = true;
            _clientThread = new Thread(ClientThreadLoop);
            _clientThread.IsBackground = true;
            _clientThread.Start();
        }

        public void Run(int timeout)
        {
            _timeout = timeout;
                
            lock (_mainThreadActions)
            {
                while (_mainThreadActions.Count > 0)
                {
                    var action = _mainThreadActions.Dequeue();
                    action?.Invoke();
                }
            }
        }

        public void Disconnect()
        {
            if (!TryCleanup())
            {
                return;
            }

            Disconnected?.Invoke();
        }

        public void Send<T>(ref T dataframe) where T : struct, INetworkDataframe
        {
            
        }

        public void Subscribe<T>(Action<T> handler) where T : struct, INetworkDataframe
        {
            
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct, INetworkDataframe
        {
            
        }

        private void ClientThreadLoop()
        {
            while (_isRunning)
            {
                while (_client.CheckEvents(out var netEvent) > 0 || _client.Service(_timeout, out netEvent) > 0)
                {
                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            break;
                        case EventType.Connect:
                            EnqueueAction(() => ConnectionSuccessful?.Invoke(netEvent.Peer));
                            break;
                        case EventType.Disconnect:
                            EnqueueAction(Disconnect);
                            break;
                        case EventType.Timeout:
                            EnqueueAction(OnConnectionFailed);
                            break;
                        case EventType.Receive:
                            netEvent.Packet.Dispose();
                            break;
                    }
                }
            }
        }

        private void EnqueueAction(Action action)
        {
            lock (_mainThreadActions)
            {
                _mainThreadActions.Enqueue(action);
            }
        }

        private void OnConnectionFailed()
        {
            if (!TryCleanup())
            {
                return;
            }

            ConnectionFailed?.Invoke();
        }
        
        private bool TryCleanup()
        {
            if (!_isRunning)
            {
                return false;
            }

            _isRunning = false;

            if (_clientThread != null && _clientThread.IsAlive)
            {
                _clientThread.Join();
            }

            _clientPeer.Disconnect(0);
            _client.Flush();
            _client.Dispose();
            Library.Deinitialize();
            
            return true;
        }
    }
}
