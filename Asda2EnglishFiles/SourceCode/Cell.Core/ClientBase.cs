using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cell.Core
{
    /// <summary>Base class for all clients.</summary>
    /// <seealso cref="T:Cell.Core.ServerBase" />
    public abstract class ClientBase : IClient, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected static readonly BufferManager Buffers = BufferManager.Default;

        /// <summary>
        /// The socket containing the TCP connection this client is using.
        /// </summary>
        protected Socket _tcpSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public const int BufferSize = 8192;

        /// <summary>
        /// Total number of bytes that have been received by all clients.
        /// </summary>
        private static long _totalBytesReceived;

        /// <summary>
        /// Total number of bytes that have been sent by all clients.
        /// </summary>
        private static long _totalBytesSent;

        /// <summary>
        /// Number of bytes that have been received by this client.
        /// </summary>
        private uint _bytesReceived;

        /// <summary>Number of bytes that have been sent by this client.</summary>
        private uint _bytesSent;

        /// <summary>Pointer to the server this client is connected to.</summary>
        protected ServerBase _server;

        /// <summary>The port the client should receive UDP datagrams on.</summary>
        protected IPEndPoint _udpEndpoint;

        /// <summary>The buffer containing the data received.</summary>
        protected BufferSegment _bufferSegment;

        /// <summary>The offset in the buffer to write at.</summary>
        protected int _remainingLength;

        /// <summary>Gets the total number of bytes sent to all clients.</summary>
        public static long TotalBytesSent
        {
            get { return ClientBase._totalBytesSent; }
        }

        /// <summary>
        /// Gets the total number of bytes received by all clients.
        /// </summary>
        public static long TotalBytesReceived
        {
            get { return ClientBase._totalBytesReceived; }
        }

        /// <summary>Default constructor</summary>
        /// <param name="server">The server this client is connected to.</param>
        protected ClientBase(ServerBase server)
        {
            this._server = server;
            this._bufferSegment = ClientBase.Buffers.CheckOut();
        }

        public ServerBase Server
        {
            get { return this._server; }
        }

        /// <summary>Gets the IP address of the client.</summary>
        public IPAddress ClientAddress
        {
            get
            {
                return this._tcpSock == null || this._tcpSock.RemoteEndPoint == null
                    ? (IPAddress) null
                    : ((IPEndPoint) this._tcpSock.RemoteEndPoint).Address;
            }
        }

        /// <summary>Gets the port the client is communicating on.</summary>
        public int Port
        {
            get
            {
                return this._tcpSock == null || this._tcpSock.RemoteEndPoint == null
                    ? -1
                    : ((IPEndPoint) this._tcpSock.RemoteEndPoint).Port;
            }
        }

        /// <summary>
        /// Gets the port the client should receive UDP datagrams on.
        /// </summary>
        public IPEndPoint UdpEndpoint
        {
            get { return this._udpEndpoint; }
            set { this._udpEndpoint = value; }
        }

        /// <summary>
        /// Gets/Sets the socket this client is using for TCP communication.
        /// </summary>
        public Socket TcpSocket
        {
            get { return this._tcpSock; }
            set
            {
                if (this._tcpSock != null && this._tcpSock.Connected)
                {
                    this._tcpSock.Shutdown(SocketShutdown.Both);
                    this._tcpSock.Close();
                }

                if (value == null)
                    return;
                this._tcpSock = value;
            }
        }

        public uint ReceivedBytes
        {
            get { return this._bytesReceived; }
        }

        public uint SentBytes
        {
            get { return this._bytesSent; }
        }

        public bool IsConnected
        {
            get { return this._tcpSock != null && this._tcpSock.Connected; }
        }

        public string AddrTemp { get; set; }

        /// <summary>Begins asynchronous TCP receiving for this client.</summary>
        public void BeginReceive()
        {
            this.ResumeReceive();
        }

        /// <summary>Resumes asynchronous TCP receiving for this client.</summary>
        private void ResumeReceive()
        {
            if (this._tcpSock == null || !this._tcpSock.Connected)
                return;
            SocketAsyncEventArgs socketAsyncEventArgs = SocketHelpers.AcquireSocketArg();
            socketAsyncEventArgs.SetBuffer(this._bufferSegment.Buffer.Array,
                this._bufferSegment.Offset + this._remainingLength, 8192);
            socketAsyncEventArgs.UserToken = (object) this;
            socketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.ReceiveAsyncComplete);
            if (!this._tcpSock.ReceiveAsync(socketAsyncEventArgs))
                this.ProcessRecieve(socketAsyncEventArgs);
        }

        private void ProcessRecieve(SocketAsyncEventArgs args)
        {
            try
            {
                int bytesTransferred = args.BytesTransferred;
                if (bytesTransferred == 0)
                {
                    this._server.DisconnectClient((IClient) this, true, false);
                }
                else
                {
                    this._bytesReceived += (uint) bytesTransferred;
                    Interlocked.Add(ref ClientBase._totalBytesReceived, (long) bytesTransferred);
                    this._remainingLength += bytesTransferred;
                    if (this._remainingLength > 6)
                        this.OnReceive();
                    this.ResumeReceive();
                }
            }
            catch (ObjectDisposedException ex)
            {
                this._server.DisconnectClient((IClient) this, true, false);
            }
            catch (Exception ex)
            {
                this._server.Warning((IClient) this, ex);
                this._server.DisconnectClient((IClient) this, true, false);
            }
            finally
            {
                args.Completed -= new EventHandler<SocketAsyncEventArgs>(this.ReceiveAsyncComplete);
                SocketHelpers.ReleaseSocketArg(args);
            }
        }

        private void ReceiveAsyncComplete(object sender, SocketAsyncEventArgs args)
        {
            this.ProcessRecieve(args);
        }

        /// <summary>
        /// Called when a packet has been received and needs to be processed.
        /// </summary>
        /// <param name="numBytes">The size of the packet in bytes.</param>
        protected abstract bool OnReceive();

        /// <summary>Asynchronously sends a packet of data to the client.</summary>
        /// <param name="packet">An array of bytes containing the packet to be sent.</param>
        public void Send(byte[] packet)
        {
            this.Send(packet, 0, packet.Length);
        }

        public void SendCopy(byte[] packet)
        {
            byte[] packet1 = new byte[packet.Length];
            Array.Copy((Array) packet, (Array) packet1, packet.Length);
            this.Send(packet1, 0, packet1.Length);
        }

        public void Send(BufferSegment segment, int length)
        {
            this.Send(segment.Buffer.Array, segment.Offset, length);
        }

        /// <summary>Asynchronously sends a packet of data to the client.</summary>
        /// <param name="packet">An array of bytes containing the packet to be sent.</param>
        /// <param name="length">The number of bytes to send starting at offset.</param>
        /// <param name="offset">The offset into packet where the sending begins.</param>
        public virtual void Send(byte[] packet, int offset, int length)
        {
            lock (this)
            {
                if (this._tcpSock == null || !this._tcpSock.Connected)
                    return;
                Interlocked.Add(ref ClientBase._totalBytesSent, (long) length);
                this._bytesSent += (uint) length;
                SocketAsyncEventArgs e = SocketHelpers.AcquireSocketArg();
                if (e != null)
                {
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(ClientBase.SendAsyncComplete);
                    e.SetBuffer(packet, offset, length);
                    e.UserToken = (object) this;
                    this._tcpSock.SendAsync(e);
                    this._bytesSent += (uint) length;
                    Interlocked.Add(ref ClientBase._totalBytesSent, (long) length);
                }
                else
                    ClientBase.log.Error("Client {0}'s SocketArgs are null", (object) this);
            }
        }

        private static void SendAsyncComplete(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= new EventHandler<SocketAsyncEventArgs>(ClientBase.SendAsyncComplete);
            SocketHelpers.ReleaseSocketArg(args);
        }

        /// <summary>
        /// Connects the client to the server at the specified address and port.
        /// </summary>
        /// <remarks>This function uses IPv4.</remarks>
        /// <param name="host">The IP address of the server to connect to.</param>
        /// <param name="port">The port to use when connecting to the server.</param>
        public void Connect(string host, int port)
        {
            this.Connect(IPAddress.Parse(host), port);
        }

        /// <summary>
        /// Connects the client to the server at the specified address and port.
        /// </summary>
        /// <remarks>This function uses IPv4.</remarks>
        /// <param name="addr">The IP address of the server to connect to.</param>
        /// <param name="port">The port to use when connecting to the server.</param>
        public void Connect(IPAddress addr, int port)
        {
            if (this._tcpSock == null)
                return;
            if (this._tcpSock.Connected)
                this._tcpSock.Disconnect(true);
            this._tcpSock.Connect(addr, port);
            this.BeginReceive();
        }

        ~ClientBase()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object) this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (typeof(ClientBase))
            {
                if (this._tcpSock == null || !this._tcpSock.Connected)
                    return;
                try
                {
                    this._bufferSegment.DecrementUsage();
                    this._tcpSock.Shutdown(SocketShutdown.Both);
                    this._tcpSock.Close();
                    this._tcpSock = (Socket) null;
                }
                catch (Exception ex)
                {
                }
            }
        }

        public override string ToString()
        {
            object obj;
            if (this.TcpSocket != null && this.TcpSocket.Connected)
            {
                EndPoint remoteEndPoint = this.TcpSocket.RemoteEndPoint;
                obj = remoteEndPoint != null ? (object) remoteEndPoint : (object) "<unknown client>";
            }
            else
                obj = (object) "<disconnected client>";

            return obj.ToString();
        }
    }
}