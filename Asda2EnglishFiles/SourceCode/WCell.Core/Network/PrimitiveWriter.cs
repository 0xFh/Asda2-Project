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
            get { return this._segmentStream.Segment; }
        }

        public PrimitiveWriter(Stream stream)
            : base(stream)
        {
            this._segmentStream = (SegmentStream) stream;
        }

        /// <summary>Writes a byte to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(byte val)
        {
            this.Write(val);
        }

        /// <summary>Writes a byte to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(ushort val)
        {
            this.Write((byte) val);
        }

        /// <summary>Writes a byte to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(short val)
        {
            this.Write((byte) val);
        }

        /// <summary>Writes a byte to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(uint val)
        {
            this.Write((byte) val);
        }

        /// <summary>Writes a byte to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(int val)
        {
            this.Write((byte) val);
        }

        /// <summary>Writes a byte to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(bool val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt16(byte val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt16(ushort val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt16(short val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt16(uint val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt16(int val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt32(byte val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt32(ushort val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt32(short val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt32(uint val)
        {
            this.Write(val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt32(int val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt32(long val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt16(byte val)
        {
            this.Write((short) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt16(ushort val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt16(short val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt16(uint val)
        {
            this.Write((short) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt16(int val)
        {
            this.Write((short) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt32(byte val)
        {
            this.Write((int) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt32(ushort val)
        {
            this.Write((int) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt32(short val)
        {
            this.Write((int) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt32(uint val)
        {
            this.Write(val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt32(int val)
        {
            this.Write(val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt64(byte val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt64(ushort val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt64(short val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt64(uint val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt64(int val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt64(ulong val)
        {
            this.Write(val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt64(long val)
        {
            this.Write((ulong) val);
        }

        public virtual void WriteInt64(long val)
        {
            this.Write(val);
        }

        public void WriteAsdaString(string s, int len, Locale locale = Locale.Start)
        {
            if (s == null)
                s = "null";
            byte[] buffer = Asda2EncodingHelper.Encode(s.Length > len ? s.Substring(0, len) : s, locale);
            this.Write(buffer);
            for (int index = 0; index < len - buffer.Length; ++index)
                this.Write((byte) 0);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(byte val)
        {
            this.Write((short) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(ushort val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(short val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(uint val)
        {
            this.Write((short) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(int val)
        {
            this.Write((short) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(byte val)
        {
            this.Write((int) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(ushort val)
        {
            this.Write((int) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(short val)
        {
            this.Write((int) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(uint val)
        {
            this.Write(val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(int val)
        {
            this.Write(val);
        }

        /// <summary>Writes a float to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(byte val)
        {
            this.Write((float) val);
        }

        /// <summary>Writes a float to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(ushort val)
        {
            this.Write((float) val);
        }

        /// <summary>Writes a float to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(short val)
        {
            this.Write((int) val);
        }

        /// <summary>Writes a float to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(uint val)
        {
            this.Write((float) val);
        }

        /// <summary>Writes a float to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(int val)
        {
            this.Write((float) val);
        }

        /// <summary>Writes a float to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(double val)
        {
            this.Write((float) val);
        }

        /// <summary>Writes a float to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(float val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(byte val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(ushort val)
        {
            this.Write(val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(short val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(uint val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes a short to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(int val)
        {
            this.Write((ushort) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(byte val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(ushort val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(short val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(uint val)
        {
            this.Write(val);
        }

        /// <summary>Writes an unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(int val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(long val)
        {
            this.Write((uint) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteULong(byte val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteULong(ushort val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteULong(short val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteULong(uint val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteULong(int val)
        {
            this.Write((ulong) val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteULong(ulong val)
        {
            this.Write(val);
        }

        /// <summary>Writes an eight byte unsigned int to the stream</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteULong(long val)
        {
            this.Write((ulong) val);
        }

        public override void Write(string str)
        {
            this.Write(PrimitiveWriter.DefaultEncoding.GetBytes(str));
            this.Write((byte) 0);
        }

        /// <summary>
        /// Writes a C-style string to the stream (actual string is null-terminated)
        /// </summary>
        /// <param name="str">String to write</param>
        public virtual void WriteCString(string str)
        {
            this.Write(PrimitiveWriter.DefaultEncoding.GetBytes(str));
            this.Write((byte) 0);
        }

        /// <summary>
        /// Writes a C-style UTF8 string to the stream (actual string is null-terminated)
        /// </summary>
        /// <param name="str">String to write</param>
        public virtual void WriteUTF8CString(string str)
        {
            this.Write(Encoding.UTF8.GetBytes(str));
            this.Write((byte) 0);
        }

        /// <summary>Writes a BigInteger to the stream</summary>
        /// <param name="bigInt">BigInteger to write</param>
        public virtual void WriteBigInt(BigInteger bigInt)
        {
            this.Write(bigInt.GetBytes());
        }

        /// <summary>Writes a BigInteger to the stream</summary>
        /// <param name="bigInt">BigInteger to write</param>
        /// <param name="length">maximum numbers of bytes to write for the BigInteger</param>
        public virtual void WriteBigInt(BigInteger bigInt, int length)
        {
            this.Write(bigInt.GetBytes(length));
        }

        /// <summary>
        /// Writes a BigInteger to the stream, while writing the length before it
        /// </summary>
        /// <param name="bigInt">BigInteger to write</param>
        public virtual void WriteBigIntLength(BigInteger bigInt)
        {
            byte[] bytes = bigInt.GetBytes();
            this.Write((byte) bytes.Length);
            this.Write(bytes);
        }

        /// <summary>
        /// Writes a BigInteger to the stream, while writing the length before it
        /// </summary>
        /// <param name="bigInt">BigInteger to write</param>
        /// <param name="length">maximum numbers of bytes to write for th BigInteger</param>
        public virtual void WriteBigIntLength(BigInteger bigInt, int length)
        {
            byte[] bytes = bigInt.GetBytes(length);
            this.Write((byte) length);
            this.Write(bytes);
        }

        /// <summary>Writes a short to the stream, in network order</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShortBE(byte val)
        {
            this.Write(IPAddress.HostToNetworkOrder((short) val));
        }

        /// <summary>Writes a short to the stream, in network order</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShortBE(short val)
        {
            this.Write(IPAddress.HostToNetworkOrder(val));
        }

        /// <summary>Writes a short to the stream, in network order</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShortBE(int val)
        {
            this.Write(IPAddress.HostToNetworkOrder((short) (byte) val));
        }

        public void WriteUShortBE(ushort val)
        {
            this.Write((byte) (((int) val & 65280) >> 8));
            this.Write((byte) ((uint) val & (uint) byte.MaxValue));
        }

        /// <summary>Writes an int to the stream, in network order</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteIntBE(byte val)
        {
            this.Write(IPAddress.HostToNetworkOrder((int) val));
        }

        /// <summary>Writes an int to the stream, in network order</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteIntBE(short val)
        {
            this.Write(IPAddress.HostToNetworkOrder((int) val));
        }

        /// <summary>Writes an int to the stream, in network order</summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteIntBE(int val)
        {
            this.Write(IPAddress.HostToNetworkOrder(val));
        }

        /// <summary>Writes a long to the stream, in network order</summary>
        /// <param name="val">the value to write</param>
        public void WriteLongBE(long val)
        {
            this.Write(IPAddress.HostToNetworkOrder(val));
        }

        /// <summary>Writes a date time to the stream, in WoW format</summary>
        /// <param name="dateTime">the time to write</param>
        public void WriteDateTime(DateTime dateTime)
        {
            this.Write(Utility.GetDateTimeToGameTime(dateTime));
        }

        public void InsertByteAt(byte value, long pos, bool returnOrigPos)
        {
            if (returnOrigPos)
            {
                long position = this.BaseStream.Position;
                this.BaseStream.Position = pos;
                this.Write(value);
                this.BaseStream.Position = position;
            }
            else
            {
                this.BaseStream.Position = pos;
                this.Write(value);
            }
        }

        public void InsertShortAt(short value, long pos, bool returnOrigPos)
        {
            if (returnOrigPos)
            {
                long position = this.BaseStream.Position;
                this.BaseStream.Position = pos;
                this.Write(value);
                this.BaseStream.Position = position;
            }
            else
            {
                this.BaseStream.Position = pos;
                this.Write(value);
            }
        }

        public void InsertIntAt(int value, long pos, bool returnOrigPos)
        {
            if (returnOrigPos)
            {
                long position = this.BaseStream.Position;
                this.BaseStream.Position = pos;
                this.Write(value);
                this.BaseStream.Position = position;
            }
            else
            {
                this.BaseStream.Position = pos;
                this.Write(value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            this._segmentStream.Position = 0L;
            base.Dispose(disposing);
        }
    }
}