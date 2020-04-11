using Cell.Core;
using System.IO;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;

namespace WCell.RealmServer.Network
{
    public class CompressedPacket : RealmPacketOut
    {
        private BinaryWriter backingStream = new BinaryWriter((Stream) new MemoryStream(2000));

        public CompressedPacket()
            : base(RealmServerOpCode.SMSG_COMPRESSED_MOVES)
        {
        }

        public void AddPacket(RealmPacketOut packet)
        {
            if (packet.ContentLength > (int) byte.MaxValue)
                throw new InvalidDataException("Packets added to a compressed stream must have length less than 255");
            this.backingStream.Write((byte) packet.ContentLength);
            this.backingStream.Write((ushort) packet.OpCode);
            this.backingStream.Write(packet.GetFinalizedPacket(), packet.HeaderSize, packet.ContentLength);
        }

        protected override void FinalizeWrite()
        {
            this.Position = (long) this.HeaderSize;
            BufferSegment segment = BufferManager.GetSegment(this.ContentLength);
            int deflatedLength;
            Compression.CompressZLib((this.backingStream.BaseStream as MemoryStream).GetBuffer(), segment.Buffer.Array,
                7, out deflatedLength);
            this.Write(deflatedLength);
            this.Write(segment.Buffer.Array);
            segment.DecrementUsage();
            base.FinalizeWrite();
        }
    }
}