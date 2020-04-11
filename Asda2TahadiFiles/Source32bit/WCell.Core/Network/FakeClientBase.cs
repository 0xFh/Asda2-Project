using Cell.Core;
using System;
using System.Net;
using System.Net.Sockets;

namespace WCell.Core.Network
{
  /// <summary>
  /// The FakeClientBase cannot handle sending of partial packets!
  /// </summary>
  /// <typeparam name="C">The type of this FakeClient</typeparam>
  /// <typeparam name="PI">The type of PacketIn</typeparam>
  /// <typeparam name="PO">The type of PacketOut</typeparam>
  /// <typeparam name="PM">The type of the PacketManager</typeparam>
  public abstract class FakeClientBase<C, PI, PO, PM> : IClient, IDisposable where C : IClient
    where PI : PacketIn
    where PO : PacketOut
    where PM : PacketManager<C, PI, ClientPacketHandlerAttribute>
  {
    public static IPAddress FakeAddress = IPAddress.Loopback;
    public static int FakePort = 1;
    protected ServerBase m_server;
    protected PM m_packetManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="server"></param>
    /// <param name="packetManager">The PacketManager that handles the packets sent to this Client by the server.</param>
    protected FakeClientBase(ServerBase server, PM packetManager)
    {
      m_server = server;
      m_packetManager = packetManager;
    }

    public ServerBase Server
    {
      get { return m_server; }
    }

    public IPAddress ClientAddress
    {
      get { return FakeAddress; }
    }

    public int Port
    {
      get { return FakePort; }
    }

    public IPEndPoint UdpEndpoint { get; set; }

    public Socket TcpSocket
    {
      get { return null; }
      set { }
    }

    public PM PacketManager
    {
      get { return m_packetManager; }
      set { m_packetManager = value; }
    }

    /// <summary>Returns this Client casted to C</summary>
    protected abstract C _ThisClient { get; }

    public bool IsConnected
    {
      get { return true; }
    }

    public string AddrTemp { get; set; }

    public virtual void BeginReceive()
    {
      throw new NotImplementedException("FakeClientBase cannot receive asynchronously.");
    }

    public virtual void Connect(string host, int port)
    {
      throw new NotImplementedException("FakeClientBase cannot connect anywhere.");
    }

    public virtual void Connect(IPAddress addr, int port)
    {
      throw new NotImplementedException("FakeClientBase cannot connect anywhere.");
    }

    /// <summary>Sends a new Packet to this Client.</summary>
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

    /// <summary>Sends a new Packet to this Client.</summary>
    public void Send(byte[] packet, int offset, int length)
    {
      HandleSMSG(CreatePacket(packet, offset, length));
    }

    public void Send(BufferSegment segment, int length)
    {
      Send(segment.Buffer.Array, segment.Offset, length);
    }

    /// <summary>Sends a new Packet to this Client.</summary>
    public void Send(PO packet, bool addEnd)
    {
      HandleSMSG(CreatePacket(packet));
    }

    /// <summary>Handles the given packet, sent by the server.</summary>
    /// <returns>Whether the packet got handled instantly or (if false) failed or was enqueued</returns>
    protected virtual bool HandleSMSG(PI packet)
    {
      if(m_packetManager.HandlePacket(_ThisClient, packet))
        return true;
      if(m_server == null)
        throw new Exception("Processing of Packet " + packet + " failed!");
      return false;
    }

    /// <summary>Remove all used resources</summary>
    public void Dispose()
    {
      m_server = null;
    }

    public override string ToString()
    {
      return nameof(FakeClientBase<C, PI, PO, PM>);
    }

    /// <summary>
    /// Creates a new PacketIn of this class' Packet-type, using the given
    /// PacketOut-bytes.
    /// </summary>
    /// <param name="outPacketBytes"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    protected abstract PI CreatePacket(byte[] outPacketBytes, int offset, int length);

    /// <summary>
    /// Creates a new PacketIn of this class' Packet-type, using the given
    /// PacketOut-bytes.
    /// </summary>
    /// <returns></returns>
    protected abstract PI CreatePacket(PO outPacket);
  }
}