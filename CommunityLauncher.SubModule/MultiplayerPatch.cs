using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using Hazel;
using TaleWorlds.MountAndBlade;
using Hazel.Udp;
using TaleWorlds.Core;
using TaleWorlds.Network;
using ConnectionState = Hazel.ConnectionState;
using DataReceivedEventArgs = Hazel.DataReceivedEventArgs;

namespace CommunityLauncher.Submodule
{
    public class NetworkProxy : DispatchProxy
    {
        private NetworkImplementation _impl;

        public NetworkProxy()
        {
            _impl = new NetworkImplementation();
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var impl = _impl.GetType();
            var method = impl.GetMethod(targetMethod.Name);
            return method.Invoke(_impl, args);
        }

        public class NetworkImplementation // : IGreeting - doesn't implement IGreeting but mimics it
        {
            private bool isDedicatedServer;
            private bool isServer;
            private int sessionKey;
            private int playerIndex;
            private Dictionary<int, Peer> _peers = new Dictionary<int, Peer>();
            private UdpConnectionListener listener;

            private UdpConnection _connection;
            private Queue<byte> sendBuffer;
            private Queue<byte> receiveBuffer;
            private Dictionary<Connection, List<byte>> connections = new Dictionary<Connection, List<byte>>();

            public bool GetMultiplayerDisabled()
            {
                return false;
                throw new System.NotImplementedException();
            }

            public bool IsDedicatedServer()
            {
                return isDedicatedServer;
            }

            public void InitializeServerSide(int port)
            {
                listener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, port));
                listener.Start();
                this.isServer = true;
            }

            public void InitializeClientSide(string serverAddress, int port, int sessionKey, int playerIndex)
            {
                _connection = new UdpClientConnection(new IPEndPoint(IPAddress.Parse(serverAddress), port));
                ((UdpClientConnection) _connection).Connect();

                this.connections.Add(_connection, new List<byte>());
                _connection.DataReceived += args =>
                {
                    foreach (var b in args.Message.Buffer)
                    {
                        this.receiveBuffer.Append(b);
                    }
                };
                _connection.Disconnected += (sender, x) => { Disconnected(_connection, sender, x); };
                this.sessionKey = sessionKey;
                this.playerIndex = playerIndex;
                this.isServer = false;
            }

            private void InitListener()
            {
                this.listener.NewConnection += (NewConnectionEventArgs args) =>
                {
                    Connected(args);
                    args.Connection.DataReceived += data => { DataReceived(args.Connection, data); };
                    args.Connection.Disconnected += (data, sender) => { Disconnected(args.Connection, data, sender); };
                };
            }

            private void Connected(NewConnectionEventArgs args)
            {
                this.connections.Add(args.Connection, new List<byte>());
            }

            private void Disconnected(Connection args, object sender, DisconnectedEventArgs disconnectedEventArgs)
            {
                this.connections.Remove(args);
                args.Dispose();
                if (_connection == args)
                {
                    _connection = null;
                }
            }

            private void DataReceived(Connection args, DataReceivedEventArgs data)
            {
                foreach (var b in data.Message.Buffer)
                {
                    this.receiveBuffer.Enqueue(b);
                }

                this.connections[args].AddRange(data.Message.Buffer);
            }

            public void TerminateServerSide()
            {
                this.isServer = false;

                this.listener.Dispose();
                this.listener = null;
            }

            public void TerminateClientSide()
            {
                this.listener.Dispose();
                this.listener = null;
            }

            public void ServerPing(string serverAddress, int port)
            {
            }

            public void AddPeerToDisconnect(int peer)
            {
                this._peers.Remove(peer);
            }

            public void SetDisableSendingMessages(int peer, bool value)
            {
            }

            public void SetDisableProcessMessages(bool value)
            {
            }

            public void PrepareNewUdpSession(int player, int sessionKey)
            {
            }

            public bool CanAddNewPlayersOnServer(int numPlayers)
            {
                return true;
            }

            public int AddNewPlayerOnServer(bool serverPlayer)
            {
                var peerindex = MBRandom.RandomInt(0, 10000000);
                this._peers.Add(peerindex,
                    new Peer(serverPlayer));
                return peerindex;
            }

            public void ResetMissionData()
            {
            }

            public void BeginBroadcastModuleEvent()
            {
                this.BeginBroadCast = true;
                this.sendBuffer = new Queue<byte>();
            }

            public bool BeginBroadCast { get; set; }

            public void EndBroadcastModuleEvent(int broadcastFlags, int targetPlayer, bool isReliable)
            {
                foreach (var keyValuePair in this.connections)
                {
                    keyValuePair.Key.SendBytes(this.sendBuffer.ToArray());
                }

                this.sendBuffer.Clear();
            }

            public double ElapsedTimeSinceLastUdpPacketArrived()
            {
                return 0;
            }

            public void BeginModuleEventAsClient(bool isReliable)
            {
                BeginBroadcastModuleEvent();
            }

            public void EndModuleEventAsClient(bool isReliable)
            {
                foreach (var keyValuePair in this.connections)
                {
                    keyValuePair.Key.SendBytes(this.sendBuffer.ToArray());
                }

                this.sendBuffer.Clear();
            }

            public bool ReadIntFromPacket(ref CompressionInfo.Integer compressionInfo, out int output)
            {
                byte[] dequeueChunk = DequeueChunk(this.receiveBuffer, 4);
                if (dequeueChunk.Length == 0)
                {
                    output = 0;
                    return false;
                }

                output = BitConverter.ToInt32(dequeueChunk, 0);
                return true;
            }

            public bool ReadUintFromPacket(ref CompressionInfo.UnsignedInteger compressionInfo, out uint output)
            {
                byte[] dequeueChunk = DequeueChunk(this.receiveBuffer, 4);
                if (dequeueChunk.Length == 0)
                {
                    output = 0;
                    return false;
                }

                output = BitConverter.ToUInt32(dequeueChunk, 0);
                return true;
            }

            public bool ReadLongFromPacket(ref CompressionInfo.LongInteger compressionInfo, out long output)
            {
                byte[] dequeueChunk = DequeueChunk(this.receiveBuffer, 8);
                if (dequeueChunk.Length == 0)
                {
                    output = 0;
                    return false;
                }

                output = BitConverter.ToInt64(dequeueChunk, 0);
                return true;
            }

            public bool ReadUlongFromPacket(ref CompressionInfo.UnsignedLongInteger compressionInfo, out ulong output)
            {
                byte[] dequeueChunk = DequeueChunk(this.receiveBuffer, 8);
                if (dequeueChunk.Length == 0)
                {
                    output = 0;
                    return false;
                }

                output = BitConverter.ToUInt64(dequeueChunk, 0);
                return true;
            }

            public bool ReadFloatFromPacket(ref CompressionInfo.Float compressionInfo, out float output)
            {
                byte[] dequeueChunk = DequeueChunk(this.receiveBuffer, 4);
                if (dequeueChunk.Length == 0)
                {
                    output = 0;
                    return false;
                }

                output = BitConverter.ToSingle(dequeueChunk, 0);
                return true;
            }

            public string ReadStringFromPacket(ref bool bufferReadValid)
            {
                List<byte> x = new List<byte>();

                while (receiveBuffer.Count > 0 && (x.Count == 0 || x[x.Count - 1] != 0x00))
                {
                    x.Add(receiveBuffer.Dequeue());
                }

                string s;
                try
                {
                    s = System.Text.Encoding.UTF8.GetString(x.ToArray());

                    bufferReadValid = true;
                }
                catch (Exception e)
                {
                    s = "";
                    bufferReadValid = false;
                }

                return s;
            }

            public void WriteIntToPacket(int value, ref CompressionInfo.Integer compressionInfo)
            {
                var buf = BitConverter.GetBytes(value);
                foreach (var b in buf)
                {
                    sendBuffer.Enqueue(b);
                }
            }

            public void WriteUintToPacket(uint value, ref CompressionInfo.UnsignedInteger compressionInfo)
            {
                var buf = BitConverter.GetBytes(value);
                foreach (var b in buf)
                {
                    sendBuffer.Enqueue(b);
                }
            }

            public void WriteLongToPacket(long value, ref CompressionInfo.LongInteger compressionInfo)
            {
                var buf = BitConverter.GetBytes(value);
                foreach (var b in buf)
                {
                    sendBuffer.Enqueue(b);
                }
            }

            public void WriteUlongToPacket(ulong value, ref CompressionInfo.UnsignedLongInteger compressionInfo)
            {
                var buf = BitConverter.GetBytes(value);
                foreach (var b in buf)
                {
                    sendBuffer.Enqueue(b);
                }
            }

            public void WriteFloatToPacket(float value, ref CompressionInfo.Float compressionInfo)
            {
                var buf = BitConverter.GetBytes(value);
                foreach (var b in buf)
                {
                    sendBuffer.Enqueue(b);
                }
            }

            public void WriteStringToPacket(string value)
            {
                var buf = Encoding.UTF8.GetBytes(value);
                foreach (var b in buf)
                {
                    sendBuffer.Enqueue(b);
                }
            }

            public int ReadByteArrayFromPacket(byte[] buffer, int offset, int bufferCapacity, ref bool bufferReadValid)
            {
                int count = 0;
                for (int i = offset; i < receiveBuffer.Count && i < bufferCapacity; i++)
                {
                    buffer[i] = receiveBuffer.Dequeue();
                    count++;
                }

                return count;
            }

            public void WriteByteArrayToPacket(byte[] value, int offset, int size)
            {
                for (int i = offset; i < size; i++)
                {
                    sendBuffer.Enqueue(value[i]);
                }
            }

            public void IncreaseTotalUploadLimit(int value)
            {
            }

            public void ResetDebugVariables()
            {
            }

            public void PrintDebugStats()
            {
            }

            public float GetAveragePacketLossRatio()
            {
                return 0;
            }

            public void GetDebugUploadsInBits(
                ref GameNetwork.DebugNetworkPacketStatisticsStruct networkStatisticsStruct,
                ref GameNetwork.DebugNetworkPositionCompressionStatisticsStruct posStatisticsStruct)
            {
            }

            public void ResetDebugUploads()
            {
            }

            public void PrintReplicationTableStatistics()
            {
            }

            public void ClearReplicationTableStatistics()
            {
            }

            public static byte[] DequeueChunk(Queue<byte> queue, int chunkSize)
            {
                var buf = new byte[chunkSize];
                for (int i = 0; i < chunkSize && queue.Count > 0; i++)
                {
                    buf[i] = queue.Dequeue();
                }

                return buf;
            }
        }
    }

    internal class Peer
    {
        private bool isServerPlayer;

        public Peer(bool isServerPlayer)
        {
            this.isServerPlayer = isServerPlayer;
        }
    }

    public static class MultiplayerPatch
    {
        public static void PatchFunctions()
        {
            var filed = typeof(MBAPI).GetField("IMBNetwork", BindingFlags.Static | BindingFlags.NonPublic);
            var type = filed.FieldType;

            var proxy = typeof(DispatchProxy).GetMethod(nameof(DispatchProxy.Create))
                .MakeGenericMethod(type, typeof(NetworkProxy)).Invoke(null, null);
            filed.SetValue(null, proxy);
        }
    }
}