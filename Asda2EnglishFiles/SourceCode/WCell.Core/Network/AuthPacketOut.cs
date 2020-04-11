using WCell.Constants;
using WCell.Core.Network;

namespace WCell.AuthServer
{
    /// <summary>Represents an outbound packet going to the client.</summary>
    public class AuthPacketOut : PacketOut
    {
        /// <summary>
        /// Constant indicating this <c>AuthPacketOut</c> header size.
        /// </summary>
        private const int _headerSize = 2;

        /// <summary>
        /// The <c>AuthPacketOut</c> header size.
        /// </summary>
        public override int HeaderSize
        {
            get { return 2; }
        }

        /// <summary>The op-code of this packet.</summary>
        public AuthServerOpCode OpCode
        {
            get { return (AuthServerOpCode) this.PacketId.RawId; }
        }

        /// <summary>Default constructor.</summary>
        /// <param name="packetOpCode">the opcode of the packet</param>
        public AuthPacketOut(AuthServerOpCode packetOpCode)
            : base(new PacketId(packetOpCode))
        {
            this.WriteByte((byte) packetOpCode);
        }
    }
}