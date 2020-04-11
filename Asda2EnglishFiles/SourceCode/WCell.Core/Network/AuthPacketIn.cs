using Cell.Core;
using WCell.Constants;
using WCell.Core.Network;

namespace WCell.AuthServer
{
    /// <summary>Represents an inbound packet from the client.</summary>
    public sealed class AuthPacketIn : PacketIn
    {
        /// <summary>
        /// Constant indicating this <c>AuthPacketIn</c> header size.
        /// </summary>
        private const int _headerSize = 1;

        /// <summary>
        /// The <c>AuthPacketOut</c> header size.
        /// </summary>
        public override int HeaderSize
        {
            get { return 1; }
        }

        /// <summary>Default constructor.</summary>
        /// <param name="length">the length of bytes to read</param>
        public AuthPacketIn(BufferSegment segment, int length)
            : base(segment, 0, length)
        {
        }

        /// <summary>Default constructor.</summary>
        /// <param name="offset">the zero-based index to read from</param>
        /// <param name="length">the length of bytes to read</param>
        public AuthPacketIn(BufferSegment segment, int offset, int length)
            : base(segment, offset, length)
        {
            this._packetID = (PacketId) ((AuthServerOpCode) this.ReadByte());
        }
    }
}