using Cell.Core;
using System;
using WCell.Constants;
using WCell.Core.Network;

namespace WCell.RealmServer
{
    /// <summary>
    /// Reads data from incoming packet stream, targetted specifically for the realm server
    /// </summary>
    public class RealmPacketIn : PacketIn
    {
        public const int HEADER_SIZE = 6;
        public const int LARGE_PACKET_HEADER_SIZE = 7;
        public const int LARGE_PACKET_THRESHOLD = 32767;
        protected bool _oversizedPacket;
        protected int _headerSize;

        /// <summary>
        /// Constructs a RealmPacketIn object given the buffer to read, the offset to read from, and the number of bytes to read.
        /// Do not use this, unless you have a BufferManager that ensures the BufferWrapper's content to be pinned.
        /// For self-managed RealmPackets, use <c>DisposableRealmPacketIn</c>.
        /// </summary>
        /// <param name="segment">buffer container to read from</param>
        /// <param name="offset">offset to read from relative to the segment offset</param>
        /// <param name="length">number of bytes to read</param>
        /// <param name="opcode">the opcode of this packet</param>
        public RealmPacketIn(BufferSegment segment, int offset, int length, RealmServerOpCode opcode, int headerSize)
            : base(segment, offset, length)
        {
            this._packetID = (PacketId) opcode;
            this.Position = headerSize;
            this._headerSize = headerSize;
            this._oversizedPacket = this._headerSize == 7;
        }

        public RealmPacketIn(BufferSegment segment, int offset, int length, bool isGameServerConnection)
            : base(segment, offset, length)
        {
            ++this.Position;
            this._packetID = (PacketId) ((RealmServerOpCode) this.ReadUInt16());
            this.Position += 24;
            if (!isGameServerConnection)
                return;
            this.Position += 4;
        }

        public override int HeaderSize
        {
            get { return this._headerSize; }
        }

        public override string ToString()
        {
            return this._packetID.ToString() + " (Length: " + (object) this.Length + ")";
        }

        /// <summary>Make sure to Dispose the copied packet!</summary>
        /// <returns></returns>
        public DisposableRealmPacketIn Copy()
        {
            BufferSegment segment = BufferManager.GetSegment(this.Length);
            if (this.ContentLength > this.Length || segment.Buffer.Array.Length <= segment.Offset + this.Length)
                throw new Exception(string.Format(
                    "Cannot copy Packet \"" + (object) this +
                    "\" because of invalid Boundaries - ArrayLength: {0} PacketLength: {1}, Offset: {2}",
                    (object) segment.Buffer.Array.Length, (object) this.Length, (object) segment.Offset));
            Buffer.BlockCopy((Array) this._segment.Buffer.Array, this._segment.Offset + this._offset,
                (Array) segment.Buffer.Array, segment.Offset, this.Length);
            return new DisposableRealmPacketIn(segment, 0, this.Length, this.ContentLength,
                (RealmServerOpCode) this._packetID.RawId);
        }
    }
}