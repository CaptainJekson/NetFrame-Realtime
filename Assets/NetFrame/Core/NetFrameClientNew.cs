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
using UnityEngine;
using EventType = ENet.EventType;

namespace NetFrame.Core
{
    public class NetFrameClientNew
    {
        private const char DataframeSeparatorTrigger = '\n';
        
        private Host _client;
        private Peer _clientPeer;
        private Thread _clientThread;
        private int _timeout;
        private bool _isRunning;
        private byte[] _buffer;
        private readonly NetFrameWriter _writer;
        private NetFrameReader _reader;

        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers;
        private readonly Queue<Action> _mainThreadActions;
        
        public event Action<Peer> ConnectionSuccessful;
        public event Action Disconnected;
        public event Action ConnectionFailed;
        public event Action<NetworkLogType, string> LogCall;

        public NetFrameClientNew(int bufferSize)
        {
            NetFrameContainer.SetClient(this);
            
            _handlers = new ConcurrentDictionary<Type, List<Delegate>>();
            _writer = new NetFrameWriter();
            _mainThreadActions = new Queue<Action>();
            _buffer = new byte[bufferSize];
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
            _writer.Reset();
            dataframe.Write(_writer);
            
            var headerDataframe = GetByTypeName(dataframe) + DataframeSeparatorTrigger;

            var heaterDataframe = Encoding.UTF8.GetBytes(headerDataframe);
            var dataDataframe = _writer.ToArraySegment();
            var allData = heaterDataframe.Concat(dataDataframe).ToArray();
            
            Send(allData);
        }

        public void Subscribe<T>(Action<T> handler) where T : struct, INetworkDataframe
        {
            Debug.LogWarning($"add type: {typeof(T).Name}");
            _handlers.AddOrUpdate(typeof(T), new List<Delegate> { handler }, (_, currentHandlers) => 
            {
                currentHandlers ??= new List<Delegate>();
                currentHandlers.Add(handler);
                return currentHandlers;
            });
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct, INetworkDataframe
        {
            if (_handlers.TryGetValue(typeof(T), out var handlers))
            {
                handlers.Remove(handler);
                
                if (handlers.Count == 0)
                {
                    _handlers.TryRemove(typeof(T), out _);
                }
            }
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
                            var packetLength = netEvent.Packet.Length;
                            if (_buffer.Length < packetLength)
                            {
                                LogCall?.Invoke(NetworkLogType.Error, "[NetFrameClient.ClientThreadLoop] " +
                                                                      "message too big: " + netEvent.Packet.Length +
                                                                      ". Limit: " + _buffer.Length);
                                netEvent.Packet.Dispose();
                                break;
                            }
                            netEvent.Packet.CopyTo(_buffer);
                            BeginReadDataframe(packetLength);
                            netEvent.Packet.Dispose();
                            break;
                    }
                }
            }
        }

        private void BeginReadDataframe(int packetLength)
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
                LogCall?.Invoke(NetworkLogType.Error, $"[NetFrameClient.BeginReadDataframe] no dataframe: {headerDataframe}");
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

            var localDataframe = dataframe;
            
            foreach (var handler in handlers)
            {
                var localHandler = handler;
                EnqueueAction(() => localHandler.DynamicInvoke(localDataframe));
            }
        }
        
        private bool Send(byte[] data)
        {
            if (data.Length <= _buffer.Length)
            {
                var packet = default(Packet);
                packet.Create(data);
                _clientPeer.Send(0, ref packet);
                
                return true;
            }

            LogCall?.Invoke(NetworkLogType.Error, $"[NetFrameClient.Send] Server.Send: message too big: {data.Length}. Limit {_buffer.Length}");
            return false;
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
        
        private string GetByTypeName<T>(T dataframe) where T : struct, INetworkDataframe
        {
            return typeof(T).Name;
        }
    }
}
