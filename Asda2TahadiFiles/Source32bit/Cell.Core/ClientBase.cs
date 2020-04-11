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
      get { return _totalBytesSent; }
    }

    /// <summary>
    /// Gets the total number of bytes received by all clients.
    /// </summary>
    public static long TotalBytesReceived
    {
      get { return _totalBytesReceived; }
    }

    /// <summary>Default constructor</summary>
    /// <param name="server">The server this client is connected to.</param>
    protected ClientBase(ServerBase server)
    {
      _server = server;
      _bufferSegment = Buffers.CheckOut();
    }

    public ServerBase Server
    {
      get { return _server; }
    }

    /// <summary>Gets the IP address of the client.</summary>
    public IPAddress ClientAddress
    {
      get
      {
        return _tcpSock == null || _tcpSock.RemoteEndPoint == null
          ? null
          : ((IPEndPoint) _tcpSock.RemoteEndPoint).Address;
      }
    }

    /// <summary>Gets the port the client is communicating on.</summary>
    public int Port
    {
      get
      {
        return _tcpSock == null || _tcpSock.RemoteEndPoint == null
          ? -1
          : ((IPEndPoint) _tcpSock.RemoteEndPoint).Port;
      }
    }

    /// <summary>
    /// Gets the port the client should receive UDP datagrams on.
    /// </summary>
    public IPEndPoint UdpEndpoint
    {
      get { return _udpEndpoint; }
      set { _udpEndpoint = value; }
    }

    /// <summary>
    /// Gets/Sets the socket this client is using for TCP communication.
    /// </summary>
    public Socket TcpSocket
    {
      get { return _tcpSock; }
      set
      {
        if(_tcpSock != null && _tcpSock.Connected)
        {
          _tcpSock.Shutdown(SocketShutdown.Both);
          _tcpSock.Close();
        }

        if(value == null)
          return;
        _tcpSock = value;
      }
    }

    public uint ReceivedBytes
    {
      get { return _bytesReceived; }
    }

    public uint SentBytes
    {
      get { return _bytesSent; }
    }

    public bool IsConnected
    {
      get { return _tcpSock != null && _tcpSock.Connected; }
    }

    public string AddrTemp { get; set; }

    /// <summary>Begins asynchronous TCP receiving for this client.</summary>
    public void BeginReceive()
    {
      ResumeReceive();
    }

    /// <summary>Resumes asynchronous TCP receiving for this client.</summary>
    private void ResumeReceive()
    {
      if(_tcpSock == null || !_tcpSock.Connected)
        return;
      SocketAsyncEventArgs socketAsyncEventArgs = SocketHelpers.AcquireSocketArg();
      socketAsyncEventArgs.SetBuffer(_bufferSegment.Buffer.Array,
        _bufferSegment.Offset + _remainingLength, 8192);
      socketAsyncEventArgs.UserToken = this;
      socketAsyncEventArgs.Completed += ReceiveAsyncComplete;
      if(!_tcpSock.ReceiveAsync(socketAsyncEventArgs))
        ProcessRecieve(socketAsyncEventArgs);
    }

    private void ProcessRecieve(SocketAsyncEventArgs args)
    {
      try
      {
        int bytesTransferred = args.BytesTransferred;
        if(bytesTransferred == 0)
        {
          _server.DisconnectClient(this, true, false);
        }
        else
        {
          _bytesReceived += (uint) bytesTransferred;
          Interlocked.Add(ref _totalBytesReceived, bytesTransferred);
          _remainingLength += bytesTransferred;
          if(_remainingLength > 6)
            OnReceive();
          ResumeReceive();
        }
      }
      catch(ObjectDisposedException ex)
      {
        _server.DisconnectClient(this, true, false);
      }
      catch(Exception ex)
      {
        _server.Warning(this, ex);
        _server.DisconnectClient(this, true, false);
      }
      finally
      {
        args.Completed -= ReceiveAsyncComplete;
        SocketHelpers.ReleaseSocketArg(args);
      }
    }

    private void ReceiveAsyncComplete(object sender, SocketAsyncEventArgs args)
    {
      ProcessRecieve(args);
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
      Send(packet, 0, packet.Length);
    }

    public void SendCopy(byte[] packet)
    {
      byte[] packet1 = new byte[packet.Length];
      Array.Copy(packet, packet1, packet.Length);
      Send(packet1, 0, packet1.Length);
    }

    public void Send(BufferSegment segment, int length)
    {
      Send(segment.Buffer.Array, segment.Offset, length);
    }

    /// <summary>Asynchronously sends a packet of data to the client.</summary>
    /// <param name="packet">An array of bytes containing the packet to be sent.</param>
    /// <param name="length">The number of bytes to send starting at offset.</param>
    /// <param name="offset">The offset into packet where the sending begins.</param>
    public virtual void Send(byte[] packet, int offset, int length)
    {
      lock(this)
      {
        if(_tcpSock == null || !_tcpSock.Connected)
          return;
        Interlocked.Add(ref _totalBytesSent, length);
        _bytesSent += (uint) length;
        SocketAsyncEventArgs e = SocketHelpers.AcquireSocketArg();
        if(e != null)
        {
          e.Completed += SendAsyncComplete;
          e.SetBuffer(packet, offset, length);
          e.UserToken = this;
          _tcpSock.SendAsync(e);
          _bytesSent += (uint) length;
          Interlocked.Add(ref _totalBytesSent, length);
        }
        else
          log.Error("Client {0}'s SocketArgs are null", this);
      }
    }

    private static void SendAsyncComplete(object sender, SocketAsyncEventArgs args)
    {
      args.Completed -= SendAsyncComplete;
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
      Connect(IPAddress.Parse(host), port);
    }

    /// <summary>
    /// Connects the client to the server at the specified address and port.
    /// </summary>
    /// <remarks>This function uses IPv4.</remarks>
    /// <param name="addr">The IP address of the server to connect to.</param>
    /// <param name="port">The port to use when connecting to the server.</param>
    public void Connect(IPAddress addr, int port)
    {
      if(_tcpSock == null)
        return;
      if(_tcpSock.Connected)
        _tcpSock.Disconnect(true);
      _tcpSock.Connect(addr, port);
      BeginReceive();
    }

    ~ClientBase()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      lock(typeof(ClientBase))
      {
        if(_tcpSock == null || !_tcpSock.Connected)
          return;
        try
        {
          _bufferSegment.DecrementUsage();
          _tcpSock.Shutdown(SocketShutdown.Both);
          _tcpSock.Close();
          _tcpSock = null;
        }
        catch(Exception ex)
        {
        }
      }
    }

    public override string ToString()
    {
      object obj;
      if(TcpSocket != null && TcpSocket.Connected)
      {
        EndPoint remoteEndPoint = TcpSocket.RemoteEndPoint;
        obj = remoteEndPoint != null ? remoteEndPoint : (object) "<unknown client>";
      }
      else
        obj = "<disconnected client>";

      return obj.ToString();
    }
  }
}