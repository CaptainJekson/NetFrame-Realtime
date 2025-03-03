using System;
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

        private readonly Dictionary<uint, Peer> _peersById;
        private Queue<Action> _mainThreadActions;

        public NetFrameServerNew()
        {
            _peersById = new Dictionary<uint, Peer>();
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
                    Debug.Log($"action.Invoke, current thread: {Thread.CurrentThread.ManagedThreadId}");
                    action?.Invoke();
                }
            }
        }

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
            
            _server.Flush();
            _server.Dispose();
            Library.Deinitialize();
        }

        private void ServerThreadLoop()
        {
            var polled = false;

            while (_isRunning)
            {
                while (!polled)
                {
                    if (_server.CheckEvents(out var netEvent) <= 0)
                    {
                        if (_server.Service(_timeout, out netEvent) <= 0)
                        {
                            break;
                        }

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            break;

                        case EventType.Connect:
                            EnqueueAction(() =>
                                Debug.Log("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP));
                            break;

                        case EventType.Disconnect:
                            EnqueueAction(() =>
                                Debug.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " +
                                          netEvent.Peer.IP));
                            break;

                        case EventType.Timeout:
                            EnqueueAction(() =>
                                Debug.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP));
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