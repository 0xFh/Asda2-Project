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
            : base((Stream) BufferManager.Default.CheckOutStream())
        {
            this.m_id = id;
        }

        /// <summary>
        /// Constructs an empty packet with an initial capacity of exactly or greater than the specified amount.
        /// </summary>
        /// <param name="id">the ID of the packet</param>
        /// <param name="maxCapacity">the minimum space required for the packet</param>
        protected PacketOut(PacketId id, int maxCapacity)
            : base((Stream) BufferManager.GetSegmentStream(maxCapacity))
        {
            this.m_id = id;
        }

        /// <summary>The packet header size.</summary>
        /// <returns>The header size in bytes.</returns>
        public abstract int HeaderSize { get; }

        /// <summary>The ID of this packet.</summary>
        /// <example>RealmServerOpCode.SMSG_QUESTGIVER_REQUEST_ITEMS</example>
        public PacketId PacketId
        {
            get { return this.m_id; }
        }

        /// <summary>The position within the current packet.</summary>
        public long Position
        {
            get { return this.BaseStream.Position; }
            set { this.BaseStream.Position = value; }
        }

        /// <summary>The length of this packet in bytes</summary>
        public int TotalLength
        {
            get { return (int) this.BaseStream.Length; }
            set { this.BaseStream.SetLength((long) value); }
        }

        public int ContentLength
        {
            get { return (int) this.BaseStream.Length - this.HeaderSize; }
            set { this.BaseStream.SetLength((long) (value + this.HeaderSize)); }
        }

        /// <summary>The buffer is already internally resized</summary>
        /// <returns></returns>
        public void Fill(byte val, int num)
        {
            for (int index = 0; index < num; ++index)
                this.Write(val);
        }

        public void Zero(int len)
        {
            for (int index = 0; index < len; ++index)
                this.Write((byte) 0);
        }

        /// <summary>Finalize packet data</summary>
        protected virtual void FinalizeWrite()
        {
        }

        /// <summary>Finalizes and copies the content of the packet</summary>
        /// <returns>Packet data</returns>
        public byte[] GetFinalizedPacket()
        {
            this.FinalizeWrite();
            byte[] buffer = new byte[this.TotalLength];
            this.BaseStream.Position = 0L;
            this.BaseStream.Read(buffer, 0, this.TotalLength);
            return buffer;
        }

        /// <summary>Reverses the contents of an array</summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="buffer">array of objects to reverse</param>
        protected static void Reverse<T>(T[] buffer)
        {
            PacketOut.Reverse<T>(buffer, buffer.Length);
        }

        /// <summary>Reverses the contents of an array</summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="buffer">array of objects to reverse</param>
        /// <param name="length">number of objects in the array</param>
        protected static void Reverse<T>(T[] buffer, int length)
        {
            for (int index = 0; index < length / 2; ++index)
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
            this.FinalizeWrite();
            return WCellUtil.ToHex(this.PacketId, ((MemoryStream) this.BaseStream).ToArray(), this.HeaderSize,
                this.ContentLength);
        }

        /// <summary>Gets the name of the packet ID. (ie. CMSG_PING)</summary>
        /// <returns>a string containing the packet's canonical name</returns>
        public override string ToString()
        {
            return this.PacketId.ToString();
        }

        /// <summary>String preceeded by uint length</summary>
        /// <param name="message"></param>
        public void WriteUIntPascalString(string message)
        {
            if (message.Length > 0)
            {
                byte[] bytes = PrimitiveWriter.DefaultEncoding.GetBytes(message);
                this.WriteUInt(bytes.Length + 1);
                this.Write(bytes);
                this.Write((byte) 0);
            }
            else
                this.WriteUInt(0);
        }
    }
}