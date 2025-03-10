using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ENet;
using NetFrame.Dataframe;
using NetFrame.Enums;
using NetFrame.WriteAndRead;
using EventType = ENet.EventType;

namespace NetFrame.Core
{
    public class NetFrameServerNew
    {
        private const char DataframeSeparatorTrigger = '\n';

        private Host _server;
        private Thread serverThread;
        private int _timeout;
        private bool _isRunning;
        private byte[] _buffer;
        private readonly NetFrameWriter _writer;
        private NetFrameReader _reader;

        private readonly ConcurrentDictionary<uint, Peer> _peersById;
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers;
        private readonly Queue<Action> _mainThreadActions;

        public event Action<Peer> ClientConnection;
        public event Action<Peer> ClientDisconnect;
        public event Action<NetworkLogType, string> LogCall;

        public NetFrameServerNew(int bufferSize)
        {
            _peersById = new ConcurrentDictionary<uint, Peer>();
            _handlers = new ConcurrentDictionary<Type, List<Delegate>>();
            _writer = new NetFrameWriter();
            _mainThreadActions = new Queue<Action>();
            _buffer = new byte[bufferSize];
        }

        public void Start(ushort port, int maxClients)
        {
            NetFrameContainer.SetServer(this);

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

        public void Send<T>(ref T dataframe, uint peerId) where T : struct, INetworkDataframe
        {
            _writer.Reset();
            dataframe.Write(_writer);
            
            var headerDataframe = GetByTypeName(dataframe) + DataframeSeparatorTrigger;

            var heaterDataframe = Encoding.UTF8.GetBytes(headerDataframe);
            var dataDataframe = _writer.ToArraySegment();
            var allData = heaterDataframe.Concat(dataDataframe).ToArray();

            Send(peerId, allData);
        }

        public void SendAll<T>(ref T dataframe) where T : struct, INetworkDataframe
        {
            foreach (var id in _peersById.Keys)
            {
                Send(ref dataframe, id);
            }
        }

        public void SendAllExcept<T>(ref T dataframe, uint peerId) where T : struct, INetworkDataframe
        {
            foreach (var id in _peersById.Keys)
            {
                if (peerId == id)
                {
                    continue;
                }
                
                Send(ref dataframe, id);
            }
        }

        public void Subscribe<T>(Action<T, uint> handler) where T : struct, INetworkDataframe
        {
            _handlers.AddOrUpdate(typeof(T), new List<Delegate> { handler }, (_, currentHandlers) =>
            {
                currentHandlers ??= new List<Delegate>();
                currentHandlers.Add(handler);
                return currentHandlers;
            });
        }

        public void Unsubscribe<T>(Action<T, uint> handler) where T : struct, INetworkDataframe
        {
            if (!_handlers.TryGetValue(typeof(T), out var handlers))
            {
                return;
            }
            
            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _handlers.TryRemove(typeof(T), out _);
            }
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
                            var packetLength = netEvent.Packet.Length;
                            if (_buffer.Length < packetLength)
                            {
                                LogCall?.Invoke(NetworkLogType.Error, "[NetFrameServerNew.ServerThreadLoop] " +
                                                                      "message too big: " + netEvent.Packet.Length +
                                                                      ". Limit: " + _buffer.Length);
                                break;
                            }

                            BeginReadDataframe(netEvent.Peer.ID, packetLength);

                            netEvent.Packet.CopyTo(_buffer);
                            netEvent.Packet.Dispose();

                            break;
                    }
                }
            }
        }

        private void BeginReadDataframe(uint clientId, int packetLength)
        {
            var tempIndex = 0;

            for (var index = 0; index < _buffer.Length; index++)
            {
                if (_buffer[index] != DataframeSeparatorTrigger)
                {
                    continue;
                }
                
                tempIndex = index + 1;
                break;
            }

            var headerSegment = new ArraySegment<byte>(_buffer, 0, tempIndex - 1);
            var contentSegment = new ArraySegment<byte>(_buffer, tempIndex, packetLength - tempIndex);
            var headerDataframe = Encoding.UTF8.GetString(headerSegment);

            if (!NetFrameDataframeCollection.TryGetByKey(headerDataframe, out var dataframe))
            {
                LogCall?.Invoke(NetworkLogType.Error, $"[NetFrame.BeginReadDataframe] no datagram: {headerDataframe}");
                return;
            }

            var targetType = dataframe.GetType();

            _reader = new NetFrameReader(new byte[_buffer.Length]);
            _reader.SetBuffer(contentSegment);

            dataframe.Read(_reader);

            if (!_handlers.TryGetValue(targetType, out var handlers))
            {
                return;
            }
            
            foreach (var handler in handlers)
            {
                handler.DynamicInvoke(dataframe, clientId);
            }
        }
        
        private bool Send(uint connectionId, byte[] data)
        {
            if (data.Length <= _buffer.Length)
            {
                if (!_peersById.TryGetValue(connectionId, out var peer))
                {
                    return false;
                }
                
                var packet = default(Packet);
                packet.Create(data);
                peer.Send(0, ref packet);
                
                return true;
            }

            LogCall?.Invoke(NetworkLogType.Error, $"[NetFrameServer.Send] Server.Send: message too big: {data.Length}. Limit {_buffer.Length}");

            return false;
        }

        private void EnqueueAction(Action action)
        {
            lock (_mainThreadActions)
            {
                _mainThreadActions.Enqueue(action);
            }
        }
        
        private string GetByTypeName<T>(T dataframe) where T : struct, INetworkDataframe
        {
            return typeof(T).Name;
        }
    }
}