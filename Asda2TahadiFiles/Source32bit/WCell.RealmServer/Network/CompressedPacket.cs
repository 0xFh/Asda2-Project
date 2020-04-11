using Cell.Core;
using System.IO;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;

namespace WCell.RealmServer.Network
{
  public class CompressedPacket : RealmPacketOut
  {
    private BinaryWriter backingStream = new BinaryWriter(new MemoryStream(2000));

    public CompressedPacket()
      : base(RealmServerOpCode.SMSG_COMPRESSED_MOVES)
    {
    }

    public void AddPacket(RealmPacketOut packet)
    {
      if(packet.ContentLength > byte.MaxValue)
        throw new InvalidDataException("Packets added to a compressed stream must have length less than 255");
      backingStream.Write((byte) packet.ContentLength);
      backingStream.Write((ushort) packet.OpCode);
      backingStream.Write(packet.GetFinalizedPacket(), packet.HeaderSize, packet.ContentLength);
    }

    protected override void FinalizeWrite()
    {
      Position = HeaderSize;
      BufferSegment segment = BufferManager.GetSegment(ContentLength);
      int deflatedLength;
      Compression.CompressZLib((backingStream.BaseStream as MemoryStream).GetBuffer(), segment.Buffer.Array,
        7, out deflatedLength);
      Write(deflatedLength);
      Write(segment.Buffer.Array);
      segment.DecrementUsage();
      base.FinalizeWrite();
    }
  }
}