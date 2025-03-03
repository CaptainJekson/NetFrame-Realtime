using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ENet;
using UnityEngine;
using EventType = ENet.EventType;

namespace NetFrame.Core
{
    public class NetFrameClientNew
    {
        private Host client;
        private Peer peer;
        private Thread clientThread;
        private int _timeout;
        private bool isRunning;

        private readonly Queue<Action> _mainThreadActions;

        public NetFrameClientNew()
        {
            _mainThreadActions = new Queue<Action>();
        }

        public void Connect(string ip, ushort port)
        {
            Library.Initialize();

            client = new Host();
            var address = new Address();
            
            address.SetHost(ip);
            address.Port = port;

            client.Create();

            peer = client.Connect(address);

            isRunning = true;
            clientThread = new Thread(ClientThreadLoop);
            clientThread.IsBackground = true;
            clientThread.Start();
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

        public void Stop()
        {
            isRunning = false;

            if (clientThread != null && clientThread.IsAlive)
            {
                clientThread.Join();
            }

            client.Flush();
            client.Dispose();
            Library.Deinitialize();
        }
        
        public void SendTest()
        {
            var packet = default(Packet);
            byte[] data = { 8, 0, 8, 0, 8, 0, 8, 0, 8 };

            packet.Create(data);
            peer.Send(0, ref packet);
        }

        private void ClientThreadLoop()
        {
            while (isRunning)
            {
                while (client.CheckEvents(out var netEvent) > 0 || client.Service(_timeout, out netEvent) > 0)
                {
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
