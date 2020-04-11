using Cell.Core;
using System;
using System.IO;
using System.Net;
using System.Text;
using WCell.Core.Cryptography;
using WCell.Util;

namespace WCell.Core.Network
{
  /// <summary>
  /// An extension of <seealso cref="T:System.IO.BinaryWriter" />, which provides overloads
  /// for writing primitives to a stream, including special WoW structures
  /// </summary>
  public class PrimitiveWriter : BinaryWriter
  {
    public static Encoding DefaultEncoding = Encoding.UTF8;
    public Locale EncodedLocale = Locale.Any;
    public Locale TextLocale;
    private readonly SegmentStream _segmentStream;

    public BufferSegment BufferSegment
    {
      get { return _segmentStream.Segment; }
    }

    public PrimitiveWriter(Stream stream)
      : base(stream)
    {
      _segmentStream = (SegmentStream) stream;
    }

    /// <summary>Writes a byte to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteByte(byte val)
    {
      Write(val);
    }

    /// <summary>Writes a byte to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteByte(ushort val)
    {
      Write((byte) val);
    }

    /// <summary>Writes a byte to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteByte(short val)
    {
      Write((byte) val);
    }

    /// <summary>Writes a byte to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteByte(uint val)
    {
      Write((byte) val);
    }

    /// <summary>Writes a byte to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteByte(int val)
    {
      Write((byte) val);
    }

    /// <summary>Writes a byte to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteByte(bool val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt16(byte val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt16(ushort val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt16(short val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt16(uint val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt16(int val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt32(byte val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt32(ushort val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt32(short val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt32(uint val)
    {
      Write(val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt32(int val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt32(long val)
    {
      Write((uint) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt16(byte val)
    {
      Write((short) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt16(ushort val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt16(short val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt16(uint val)
    {
      Write((short) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt16(int val)
    {
      Write((short) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt32(byte val)
    {
      Write((int) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt32(ushort val)
    {
      Write((int) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt32(short val)
    {
      Write((int) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt32(uint val)
    {
      Write(val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt32(int val)
    {
      Write(val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt64(byte val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt64(ushort val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt64(short val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt64(uint val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt64(int val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt64(ulong val)
    {
      Write(val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt64(long val)
    {
      Write((ulong) val);
    }

    public virtual void WriteInt64(long val)
    {
      Write(val);
    }

    public void WriteAsdaString(string s, int len, Locale locale = Locale.Start)
    {
      if(s == null)
        s = "null";
      byte[] buffer = Asda2EncodingHelper.Encode(s.Length > len ? s.Substring(0, len) : s, locale);
      Write(buffer);
      for(int index = 0; index < len - buffer.Length; ++index)
        Write((byte) 0);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShort(byte val)
    {
      Write((short) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShort(ushort val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShort(short val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShort(uint val)
    {
      Write((short) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShort(int val)
    {
      Write((short) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt(byte val)
    {
      Write((int) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt(ushort val)
    {
      Write((int) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt(short val)
    {
      Write((int) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt(uint val)
    {
      Write(val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteInt(int val)
    {
      Write(val);
    }

    /// <summary>Writes a float to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteFloat(byte val)
    {
      Write((float) val);
    }

    /// <summary>Writes a float to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteFloat(ushort val)
    {
      Write((float) val);
    }

    /// <summary>Writes a float to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteFloat(short val)
    {
      Write((int) val);
    }

    /// <summary>Writes a float to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteFloat(uint val)
    {
      Write((float) val);
    }

    /// <summary>Writes a float to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteFloat(int val)
    {
      Write((float) val);
    }

    /// <summary>Writes a float to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteFloat(double val)
    {
      Write((float) val);
    }

    /// <summary>Writes a float to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteFloat(float val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUShort(byte val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUShort(ushort val)
    {
      Write(val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUShort(short val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUShort(uint val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes a short to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUShort(int val)
    {
      Write((ushort) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt(byte val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt(ushort val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt(short val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt(uint val)
    {
      Write(val);
    }

    /// <summary>Writes an unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt(int val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteUInt(long val)
    {
      Write((uint) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteULong(byte val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteULong(ushort val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteULong(short val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteULong(uint val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteULong(int val)
    {
      Write((ulong) val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteULong(ulong val)
    {
      Write(val);
    }

    /// <summary>Writes an eight byte unsigned int to the stream</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteULong(long val)
    {
      Write((ulong) val);
    }

    public override void Write(string str)
    {
      Write(DefaultEncoding.GetBytes(str));
      Write((byte) 0);
    }

    /// <summary>
    /// Writes a C-style string to the stream (actual string is null-terminated)
    /// </summary>
    /// <param name="str">String to write</param>
    public virtual void WriteCString(string str)
    {
      Write(DefaultEncoding.GetBytes(str));
      Write((byte) 0);
    }

    /// <summary>
    /// Writes a C-style UTF8 string to the stream (actual string is null-terminated)
    /// </summary>
    /// <param name="str">String to write</param>
    public virtual void WriteUTF8CString(string str)
    {
      Write(Encoding.UTF8.GetBytes(str));
      Write((byte) 0);
    }

    /// <summary>Writes a BigInteger to the stream</summary>
    /// <param name="bigInt">BigInteger to write</param>
    public virtual void WriteBigInt(BigInteger bigInt)
    {
      Write(bigInt.GetBytes());
    }

    /// <summary>Writes a BigInteger to the stream</summary>
    /// <param name="bigInt">BigInteger to write</param>
    /// <param name="length">maximum numbers of bytes to write for the BigInteger</param>
    public virtual void WriteBigInt(BigInteger bigInt, int length)
    {
      Write(bigInt.GetBytes(length));
    }

    /// <summary>
    /// Writes a BigInteger to the stream, while writing the length before it
    /// </summary>
    /// <param name="bigInt">BigInteger to write</param>
    public virtual void WriteBigIntLength(BigInteger bigInt)
    {
      byte[] bytes = bigInt.GetBytes();
      Write((byte) bytes.Length);
      Write(bytes);
    }

    /// <summary>
    /// Writes a BigInteger to the stream, while writing the length before it
    /// </summary>
    /// <param name="bigInt">BigInteger to write</param>
    /// <param name="length">maximum numbers of bytes to write for th BigInteger</param>
    public virtual void WriteBigIntLength(BigInteger bigInt, int length)
    {
      byte[] bytes = bigInt.GetBytes(length);
      Write((byte) length);
      Write(bytes);
    }

    /// <summary>Writes a short to the stream, in network order</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShortBE(byte val)
    {
      Write(IPAddress.HostToNetworkOrder(val));
    }

    /// <summary>Writes a short to the stream, in network order</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShortBE(short val)
    {
      Write(IPAddress.HostToNetworkOrder(val));
    }

    /// <summary>Writes a short to the stream, in network order</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteShortBE(int val)
    {
      Write(IPAddress.HostToNetworkOrder((byte) val));
    }

    public void WriteUShortBE(ushort val)
    {
      Write((byte) ((val & 65280) >> 8));
      Write((byte) (val & (uint) byte.MaxValue));
    }

    /// <summary>Writes an int to the stream, in network order</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteIntBE(byte val)
    {
      Write(IPAddress.HostToNetworkOrder((int) val));
    }

    /// <summary>Writes an int to the stream, in network order</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteIntBE(short val)
    {
      Write(IPAddress.HostToNetworkOrder((int) val));
    }

    /// <summary>Writes an int to the stream, in network order</summary>
    /// <param name="val">the value to write</param>
    public virtual void WriteIntBE(int val)
    {
      Write(IPAddress.HostToNetworkOrder(val));
    }

    /// <summary>Writes a long to the stream, in network order</summary>
    /// <param name="val">the value to write</param>
    public void WriteLongBE(long val)
    {
      Write(IPAddress.HostToNetworkOrder(val));
    }

    /// <summary>Writes a date time to the stream, in WoW format</summary>
    /// <param name="dateTime">the time to write</param>
    public void WriteDateTime(DateTime dateTime)
    {
      Write(Utility.GetDateTimeToGameTime(dateTime));
    }

    public void InsertByteAt(byte value, long pos, bool returnOrigPos)
    {
      if(returnOrigPos)
      {
        long position = BaseStream.Position;
        BaseStream.Position = pos;
        Write(value);
        BaseStream.Position = position;
      }
      else
      {
        BaseStream.Position = pos;
        Write(value);
      }
    }

    public void InsertShortAt(short value, long pos, bool returnOrigPos)
    {
      if(returnOrigPos)
      {
        long position = BaseStream.Position;
        BaseStream.Position = pos;
        Write(value);
        BaseStream.Position = position;
      }
      else
      {
        BaseStream.Position = pos;
        Write(value);
      }
    }

    public void InsertIntAt(int value, long pos, bool returnOrigPos)
    {
      if(returnOrigPos)
      {
        long position = BaseStream.Position;
        BaseStream.Position = pos;
        Write(value);
        BaseStream.Position = position;
      }
      else
      {
        BaseStream.Position = pos;
        Write(value);
      }
    }

    protected override void Dispose(bool disposing)
    {
      _segmentStream.Position = 0L;
      base.Dispose(disposing);
    }
  }
}