using Cell.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.Util.NLog;

namespace WCell.RealmServer.Network
{
  /// <summary>Represents a client connected to the realm server</summary>
  public sealed class RealmClient : ClientBase, IRealmClient, IClient, IDisposable, IPacketReceiver
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public static readonly List<IRealmClient> EmptyArray = new List<IRealmClient>();
    private int decryptUntil = -1;
    private byte[] m_sessionKey;
    public bool KnownClientVersion;
    private PacketCrypt m_packetCrypt;
    private int encrypt;
    private int decryptSeq;

    /// <summary>The server this client is connected to.</summary>
    public RealmServer Server
    {
      get { return (RealmServer) _server; }
    }

    public bool IsGameServerConnection { get; set; }

    /// <summary>
    /// The <see cref="T:WCell.Core.ClientInformation">system information</see> for this client.
    /// </summary>
    public ClientInformation Info { get; set; }

    /// <summary>The compressed addon data sent by the client.</summary>
    public byte[] Addons { get; set; }

    /// <summary>The account on this session.</summary>
    public RealmAccount Account { get; set; }

    /// <summary>
    /// The <see cref="T:WCell.RealmServer.Entities.Character" /> that the client is currently playing.
    /// </summary>
    public Character ActiveCharacter { get; set; }

    /// <summary>Whether or not this client is currently logging out.</summary>
    public bool IsOffline { get; set; }

    /// <summary>
    /// Whether or not communication with this client is encrypted.
    /// </summary>
    public bool IsEncrypted
    {
      get { return m_sessionKey != null; }
    }

    /// <summary>The local system uptime of the client.</summary>
    public uint ClientTime { get; set; }

    /// <summary>Connection latency between client and server.</summary>
    public int Latency { get; set; }

    /// <summary>The amount of time skipped by the client.</summary>
    /// <remarks>Deals with the the way we calculate movement delay.</remarks>
    public uint OutOfSyncDelay { get; set; }

    public uint LastClientMoveTime { get; set; }

    /// <summary>The client tick count.</summary>
    /// <remarks>It is set by opcodes 912/913, and seems to be a client ping sequence that is
    /// local to the map, and thus it resets to 0 on a map change.  Real usage isn't known.</remarks>
    public uint TickCount { get; set; }

    /// <summary>
    /// The client seed sent by the client during re-authentication.
    /// </summary>
    public uint ClientSeed { get; set; }

    /// <summary>
    /// The authentication message digest received from the client during re-authentication.
    /// </summary>
    public BigInteger ClientDigest { get; set; }

    /// <summary>Create an realm client for a given server.</summary>
    /// <param name="server">reference to the parent RealmServer</param>
    public RealmClient(RealmServer server)
      : base(server)
    {
    }

    /// <summary>
    /// Pass recieved data into the packet buffer and try to parse.
    /// </summary>
    /// <param name="_remainingLength">number of bytes waiting to be read</param>
    /// <returns>false, if there is a part of a packet still remaining</returns>
    protected override bool OnReceive()
    {
      ushort uint16;
      while(true)
      {
        int offset = _bufferSegment.Offset;
        uint16 = BitConverter.ToUInt16(_bufferSegment.Buffer.Array, offset + 1);
        if(uint16 != 0)
        {
          if(uint16 <= 4096)
          {
            if(_remainingLength >= uint16)
            {
              if(!KnownClientVersion)
              {
                byte[] buffer = new byte[2]
                {
                  _bufferSegment.Buffer.Array[offset + 3],
                  _bufferSegment.Buffer.Array[offset + 4]
                };
                Asda2CryptHelper.XorData(buffer, 0, 2L, Locale.Start, Locale.Any);
                if(buffer[1] == 0)
                  Locale = Locale.Start;
                else if(buffer[1] == 116)
                  Locale = Locale.Ar;
                else if(buffer[1] == 228)
                  Locale = Locale.Ru;
                else
                {
                  Asda2CryptHelper.XorData(buffer, 0, 2L, Locale.Start, Locale.Tahadi);

                  if(buffer[1] == 0)
                    Locale = Locale.Tahadi;
                  else if(buffer[1] == 103)
                    Locale = Locale.LOS;
                }

                KnownClientVersion = true;
              }

              Asda2CryptHelper.XorData(_bufferSegment.Buffer.Array, offset + 3,
                uint16 - 4, Locale, Locale.Any);
              RealmPacketIn packet = new RealmPacketIn(_bufferSegment, 7, uint16 - 8,
                IsGameServerConnection);
              RealmPacketMgr.Instance.HandlePacket(this, packet);
              _remainingLength -= uint16;
              BufferSegment bufferSegment = _bufferSegment;
              _bufferSegment = Buffers.CheckOut();
              if(_remainingLength > 0)
                Array.Copy(bufferSegment.Buffer.Array, bufferSegment.Offset + uint16,
                  _bufferSegment.Buffer.Array, _bufferSegment.Offset,
                  _remainingLength);
              else
                goto label_14;
            }
            else
              goto label_5;
          }
          else
            goto label_3;
        }
        else
          break;
      }

      throw new ObjectDisposedException("none");
      label_3:
      LogUtil.WarnException("{0} send packet with lenght {1}. HACKER! Remaining length {2}", (object) AccountName,
        (object) uint16, (object) _remainingLength);
      _remainingLength = 0;
      Disconnect(false);
      throw new InvalidOperationException("Wrong data from client.");
      label_5:
      return true;
      label_14:
      return true;
    }

    public Locale Locale { get; set; }

    /// <summary>
    /// Sends the given bytes representing a full packet, to the Client
    /// </summary>
    /// <param name="packet"></param>
    public override void Send(byte[] packet, int offset, int count)
    {
      if(IsOffline)
        return;
      base.Send(packet, offset, count);
    }

    public void Send(RealmPacketOut packet, bool addEndOfPacket = false)
    {
      packet.FinalizeAsda(addEndOfPacket, Locale);
      try
      {
        Send(packet.BufferSegment, packet.TotalLength);
      }
      catch(SocketException ex)
      {
        Disconnect(false);
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(AddrTemp);
      stringBuilder.Append(" - Account: ");
      stringBuilder.Append(AccountName);
      if(ActiveCharacter != null)
      {
        stringBuilder.Append(" - Char: ");
        stringBuilder.Append(ActiveCharacter.Name);
      }

      return stringBuilder.ToString();
    }

    public Auth.Accounts.Account AuthAccount { get; set; }

    public void Disconnect(bool disconnectSocketWithDelay)
    {
      if(!IsConnected)
        return;
      _server.DisconnectClient(this, true, disconnectSocketWithDelay);
    }

    /// <summary>
    /// The session key for the latest session of this account.
    /// </summary>
    public byte[] SessionKey
    {
      get { return m_sessionKey; }
      set
      {
        m_sessionKey = value;
        m_packetCrypt = new PacketCrypt(value);
      }
    }

    public string AccountName { get; set; }

    public string Password { get; set; }

    /// <summary>Encrypts the byte array</summary>
    /// <param name="data">The raw packet data to encrypt</param>
    private void Encrypt(byte[] data, int offset)
    {
      if(Interlocked.Exchange(ref encrypt, 1) == 1)
        log.Error("Encrypt Error");
      m_packetCrypt.Encrypt(data, offset, 4);
      Interlocked.Exchange(ref encrypt, 0);
    }

    private byte GetDecryptedByte(byte[] inputData, int baseOffset, int offset)
    {
      if(Interlocked.Exchange(ref decryptSeq, 1) == 1)
        log.Error("Decrypt Error");
      int start = baseOffset + offset;
      if(decryptUntil < offset)
        m_packetCrypt.Decrypt(inputData, start, 1);
      Interlocked.Exchange(ref decryptSeq, 0);
      return inputData[start];
    }

    private int GetDecryptedOpcode(byte[] inputData, int baseOffset, int offset)
    {
      int num = baseOffset + offset;
      if(decryptUntil < offset + 4)
        m_packetCrypt.Decrypt(inputData, num, 4);
      return BitConverter.ToInt32(inputData, num);
    }
  }
}