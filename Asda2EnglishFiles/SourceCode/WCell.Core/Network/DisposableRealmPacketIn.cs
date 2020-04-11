using Cell.Core;
using NLog;
using System;
using WCell.Constants;
using WCell.RealmServer;

namespace WCell.Core.Network
{
    /// <summary>
    /// This kind of RealmPacketIn frees the used BufferSegment no disposal
    /// </summary>
    public class DisposableRealmPacketIn : RealmPacketIn
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        public DisposableRealmPacketIn(BufferSegment segment, int offset, int length, int contentLength,
            RealmServerOpCode packetId)
            : base(segment, offset, length, packetId, length - contentLength)
        {
        }

        /// <summary>Exposed BufferSegment for customizations etc.</summary>
        public BufferSegment Segment
        {
            get { return this._segment; }
        }

        public static DisposableRealmPacketIn CreateFromOutPacket(RealmPacketOut packet)
        {
            byte[] finalizedPacket = packet.GetFinalizedPacket();
            return DisposableRealmPacketIn.CreateFromOutPacket(finalizedPacket, 0, finalizedPacket.Length);
        }

        public static DisposableRealmPacketIn CreateFromOutPacket(BufferSegment segment, RealmPacketOut packet)
        {
            byte[] finalizedPacket = packet.GetFinalizedPacket();
            return DisposableRealmPacketIn.Create(packet.PacketId, finalizedPacket, packet.HeaderSize,
                finalizedPacket.Length - packet.HeaderSize, segment);
        }

        public static DisposableRealmPacketIn CreateFromOutPacket(byte[] outPacket, int offset, int totalLength)
        {
            int num = offset;
            RealmServerOpCode realmServerOpCode =
                (RealmServerOpCode) ((int) outPacket[num + 2] | (int) outPacket[num + 3] << 8);
            BufferSegment segment = BufferManager.GetSegment(totalLength + 2);
            return DisposableRealmPacketIn.Create((PacketId) realmServerOpCode, outPacket, offset + 4, totalLength - 4,
                segment);
        }

        public static DisposableRealmPacketIn CreateFromOutPacket(BufferSegment oldSegment, BufferSegment newSegment,
            int totalLength)
        {
            return DisposableRealmPacketIn.CreateFromOutPacket(oldSegment, newSegment, 0, totalLength);
        }

        public static DisposableRealmPacketIn CreateFromOutPacket(BufferSegment oldSegment, BufferSegment newSegment,
            int offset, int totalLength)
        {
            int num = oldSegment.Offset + offset;
            return DisposableRealmPacketIn.Create(
                (PacketId) ((RealmServerOpCode) ((int) oldSegment.Buffer.Array[num + 2] |
                                                 (int) oldSegment.Buffer.Array[num + 3] << 8)), oldSegment.Buffer.Array,
                oldSegment.Offset + offset + 4, totalLength - 4, newSegment);
        }

        public static DisposableRealmPacketIn Create(PacketId opCode, byte[] outPacketContent)
        {
            BufferSegment segment = BufferManager.GetSegment(outPacketContent.Length + 6);
            return DisposableRealmPacketIn.Create(opCode, outPacketContent, 0, outPacketContent.Length, segment);
        }

        public static DisposableRealmPacketIn Create(PacketId opCode, byte[] outPacketContent, int contentOffset,
            int contentLength, BufferSegment segment)
        {
            if (!Enum.IsDefined(typeof(RealmServerOpCode), (object) opCode.RawId))
                throw new Exception("Packet had undefined Opcode: " + (object) opCode);
            int num1 = contentLength > (int) short.MaxValue ? 7 : 6;
            int length = contentLength + num1;
            int num2 = length - (num1 - 4);
            if (num1 == 7)
            {
                segment.Buffer.Array[segment.Offset] = (byte) (num2 >> 16 | 128);
                segment.Buffer.Array[segment.Offset + 1] = (byte) (num2 >> 8);
                segment.Buffer.Array[segment.Offset + 2] = (byte) num2;
                segment.Buffer.Array[segment.Offset + 3] = (byte) opCode.RawId;
                segment.Buffer.Array[segment.Offset + 4] = (byte) (opCode.RawId >> 8);
            }
            else
            {
                segment.Buffer.Array[segment.Offset] = (byte) (num2 >> 8);
                segment.Buffer.Array[segment.Offset + 1] = (byte) num2;
                segment.Buffer.Array[segment.Offset + 2] = (byte) opCode.RawId;
                segment.Buffer.Array[segment.Offset + 3] = (byte) (opCode.RawId >> 8);
            }

            Array.Copy((Array) outPacketContent, contentOffset, (Array) segment.Buffer.Array, segment.Offset + num1,
                contentLength);
            return new DisposableRealmPacketIn(segment, 0, length, contentLength, (RealmServerOpCode) opCode.RawId);
        }
    }
}