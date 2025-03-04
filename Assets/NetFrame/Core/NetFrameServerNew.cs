using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ENet;
using UnityEngine;
using EventType = ENet.EventType;

namespace NetFrame.Core
{
    public class NetFrameServerNew
    {
        private Host _server;
        private Thread serverThread;
        private int _timeout;
        private bool _isRunning;

        private readonly ConcurrentDictionary<uint, Peer> _peersById;
        private readonly Queue<Action> _mainThreadActions;
        
        public event Action<Peer> ClientConnection;
        public event Action<Peer> ClientDisconnect;

        public NetFrameServerNew()
        {
            _peersById = new ConcurrentDictionary<uint, Peer>();
            _mainThreadActions = new Queue<Action>();
        }

        public void Start(ushort port, int maxClients)
        {
            Library.Initialize();

            _server = new Host();
            var address = new Address
            {
                Port = port,
            };

            _server.Create(address, maxClients);

            serverThread = new Thread(ServerThreadLoop);
            serverThread.IsBackground = true;
            _isRunning = true;
            serverThread.Start();
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

        [Obsolete("только для теста")]
        public void SendAllTest()
        {
            var packet = default(Packet);
            byte[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            packet.Create(data);

            foreach (var peer in _peersById.Values)
            {
                peer.Send(0, ref packet);
            }
        }

        public void Stop()
        {
            _isRunning = false;

            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Join();
            }
            
            foreach (var peer in _peersById)
            {
                peer.Value.Disconnect(0);
            }
            _peersById.Clear();
            
            _server.Flush();
            _server.Dispose();
            
            Library.Deinitialize();
        }

        private void ServerThreadLoop()
        {
            while (_isRunning)
            {
                while (_server.CheckEvents(out var netEvent) > 0 || _server.Service(_timeout, out netEvent) > 0)
                {
                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            break;
                        case EventType.Connect:
                            _peersById.TryAdd(netEvent.Peer.ID, netEvent.Peer);
                            EnqueueAction(() => ClientConnection?.Invoke(netEvent.Peer));
                            break;
                        case EventType.Disconnect:
                            EnqueueAction(() => ClientDisconnect?.Invoke(netEvent.Peer));
                            _peersById.Remove(netEvent.Peer.ID, out _);
                            break;
                        case EventType.Timeout:
                            EnqueueAction(() => ClientDisconnect?.Invoke(netEvent.Peer));
                            _peersById.Remove(netEvent.Peer.ID, out _);
                            break;
                        case EventType.Receive:

                            var sb = new StringBuilder();

                            byte[] buffer = new byte[netEvent.Packet.Length]; // создаем массив нужного размера
                            netEvent.Packet.CopyTo(buffer); // копируем данные из пакета в массив

                            foreach (var bb in buffer)
                            {
                                sb.Append(bb + "|");
                            }

                            EnqueueAction(() =>
                                Debug.Log("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " +
                                          netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " +
                                          netEvent.Packet.Length + ", Data: " + sb));

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
    }
}