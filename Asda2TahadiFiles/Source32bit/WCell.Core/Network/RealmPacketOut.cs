using System;
using WCell.Constants;
using WCell.Util.Variables;

namespace WCell.Core.Network
{
  /// <summary>Packet sent to the realm server</summary>
  public class RealmPacketOut : PacketOut
  {
    [NotVariable]private static int _cnter = 288368098;

    /// <summary>
    /// Constant indicating this <c>RealmPacketOut</c> header size.
    /// </summary>
    public const int HEADER_SIZE = 4;

    public const int FullUpdatePacketHeaderSize = 8;
    public const int MaxPacketSize = 65535;
    private static byte _xorKeyNum;

    public RealmPacketOut(PacketId packetId)
      : this(packetId, 128)
    {
    }

    public RealmPacketOut(RealmServerOpCode packetOpCode)
      : base(new PacketId(packetOpCode))
    {
      WriteByte(251);
      WriteInt16(0);
      WriteByte(_xorKeyNum++);
      WriteInt32(7077887);
      WriteInt16((ushort) packetOpCode);
    }

    public RealmPacketOut(PacketId packetId, int maxContentLength)
      : base(packetId, maxContentLength + 4)
    {
      Position += 2L;
      Write((ushort) packetId.RawId);
    }

    /// <summary>
    /// The <c>RealmPacketOut</c> header size.
    /// </summary>
    public override int HeaderSize
    {
      get { return 4; }
    }

    /// <summary>The opcode of the packet</summary>
    public RealmServerOpCode OpCode
    {
      get { return (RealmServerOpCode) PacketId.RawId; }
      set
      {
        if(OpCode == value)
          return;
        long position = Position;
        Position = 2L;
        WriteShort((short) value);
        Position = position;
      }
    }

    public bool? FinalizedForRus { get; set; }

    /// <summary>todo optimize perfomance by sortin eng rus clients</summary>
    /// <param name="addEndOfPacket"></param>
    /// <param name="isRus"></param>
    public void FinalizeAsda(bool addEndOfPacket, Locale locale)
    {
      if(locale == EncodedLocale)
        return;
      if(EncodedLocale == Locale.Any && addEndOfPacket)
      {
        WriteInt32(_cnter++);
        WriteInt16(0);
      }

      Asda2CryptHelper.XorData(BufferSegment.Buffer.Array, BufferSegment.Offset + 3, Position - 3L,
        EncodedLocale, locale);
      if(EncodedLocale == Locale.Any)
        WriteByte(254);
      short position = (short) Position;
      if(EncodedLocale == Locale.Any)
        Array.Copy(BitConverter.GetBytes(position), 0, BufferSegment.Buffer.Array,
          BufferSegment.Offset + 1, 2);
      EncodedLocale = locale;
      if(position >= 2048)
        throw new InvalidOperationException(string.Format("Packet {0} Len >= 2048", this));
    }

    /// <summary>Finalize packet data</summary>
    protected override void FinalizeWrite()
    {
      base.FinalizeWrite();
      Position = 0L;
      WriteUShortBE((ushort) ((ulong) BaseStream.Length - 2UL));
    }

    public void WriteSkip(byte[] bytes)
    {
      Write(bytes);
    }

    public void WriteFixedAsciiString(string str, int len, Locale locale = Locale.Start)
    {
      WriteAsdaString(str, len, locale);
    }

    public void WriteAsciiString(string msg, Locale locale = Locale.Start)
    {
      Write(Asda2EncodingHelper.Encode(msg, locale));
      WriteByte(0);
    }
  }
}