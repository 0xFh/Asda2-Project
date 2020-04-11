using Cell.Core;
using NLog;
using System.IO;

namespace WCell.Core.Network
{
  /// <summary>Writes data to an outgoing packet stream</summary>
  public abstract class PacketOut : PrimitiveWriter
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    protected PacketId m_id;

    /// <summary>Constructs an empty packet with the given packet ID.</summary>
    /// <param name="id">the ID of the packet</param>
    protected PacketOut(PacketId id)
      : base(BufferManager.Default.CheckOutStream())
    {
      m_id = id;
    }

    /// <summary>
    /// Constructs an empty packet with an initial capacity of exactly or greater than the specified amount.
    /// </summary>
    /// <param name="id">the ID of the packet</param>
    /// <param name="maxCapacity">the minimum space required for the packet</param>
    protected PacketOut(PacketId id, int maxCapacity)
      : base(BufferManager.GetSegmentStream(maxCapacity))
    {
      m_id = id;
    }

    /// <summary>The packet header size.</summary>
    /// <returns>The header size in bytes.</returns>
    public abstract int HeaderSize { get; }

    /// <summary>The ID of this packet.</summary>
    /// <example>RealmServerOpCode.SMSG_QUESTGIVER_REQUEST_ITEMS</example>
    public PacketId PacketId
    {
      get { return m_id; }
    }

    /// <summary>The position within the current packet.</summary>
    public long Position
    {
      get { return BaseStream.Position; }
      set { BaseStream.Position = value; }
    }

    /// <summary>The length of this packet in bytes</summary>
    public int TotalLength
    {
      get { return (int) BaseStream.Length; }
      set { BaseStream.SetLength(value); }
    }

    public int ContentLength
    {
      get { return (int) BaseStream.Length - HeaderSize; }
      set { BaseStream.SetLength(value + HeaderSize); }
    }

    /// <summary>The buffer is already internally resized</summary>
    /// <returns></returns>
    public void Fill(byte val, int num)
    {
      for(int index = 0; index < num; ++index)
        Write(val);
    }

    public void Zero(int len)
    {
      for(int index = 0; index < len; ++index)
        Write((byte) 0);
    }

    /// <summary>Finalize packet data</summary>
    protected virtual void FinalizeWrite()
    {
    }

    /// <summary>Finalizes and copies the content of the packet</summary>
    /// <returns>Packet data</returns>
    public byte[] GetFinalizedPacket()
    {
      FinalizeWrite();
      byte[] buffer = new byte[TotalLength];
      BaseStream.Position = 0L;
      BaseStream.Read(buffer, 0, TotalLength);
      return buffer;
    }

    /// <summary>Reverses the contents of an array</summary>
    /// <typeparam name="T">type of the array</typeparam>
    /// <param name="buffer">array of objects to reverse</param>
    protected static void Reverse<T>(T[] buffer)
    {
      Reverse(buffer, buffer.Length);
    }

    /// <summary>Reverses the contents of an array</summary>
    /// <typeparam name="T">type of the array</typeparam>
    /// <param name="buffer">array of objects to reverse</param>
    /// <param name="length">number of objects in the array</param>
    protected static void Reverse<T>(T[] buffer, int length)
    {
      for(int index = 0; index < length / 2; ++index)
      {
        T obj = buffer[index];
        buffer[index] = buffer[length - index - 1];
        buffer[length - index - 1] = obj;
      }
    }

    /// <summary>
    /// Dumps the packet to string form, using hexadecimal as the formatter
    /// </summary>
    /// <returns>hexadecimal representation of the data parsed</returns>
    public string ToHexDump()
    {
      FinalizeWrite();
      return WCellUtil.ToHex(PacketId, ((MemoryStream) BaseStream).ToArray(), HeaderSize,
        ContentLength);
    }

    /// <summary>Gets the name of the packet ID. (ie. CMSG_PING)</summary>
    /// <returns>a string containing the packet's canonical name</returns>
    public override string ToString()
    {
      return PacketId.ToString();
    }

    /// <summary>String preceeded by uint length</summary>
    /// <param name="message"></param>
    public void WriteUIntPascalString(string message)
    {
      if(message.Length > 0)
      {
        byte[] bytes = DefaultEncoding.GetBytes(message);
        WriteUInt(bytes.Length + 1);
        Write(bytes);
        Write((byte) 0);
      }
      else
        WriteUInt(0);
    }
  }
}