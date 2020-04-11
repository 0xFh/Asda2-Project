using System;
using WCell.Constants;
using WCell.Util.Variables;

namespace WCell.Core.Network
{
    /// <summary>Packet sent to the realm server</summary>
    public class RealmPacketOut : PacketOut
    {
        [NotVariable] private static int _cnter = 288368098;

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
            this.WriteByte(251);
            this.WriteInt16(0);
            this.WriteByte(RealmPacketOut._xorKeyNum++);
            this.WriteInt32(7077887);
            this.WriteInt16((ushort) packetOpCode);
        }

        public RealmPacketOut(PacketId packetId, int maxContentLength)
            : base(packetId, maxContentLength + 4)
        {
            this.Position += 2L;
            this.Write((ushort) packetId.RawId);
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
            get { return (RealmServerOpCode) this.PacketId.RawId; }
            set
            {
                if (this.OpCode == value)
                    return;
                long position = this.Position;
                this.Position = 2L;
                this.WriteShort((short) value);
                this.Position = position;
            }
        }

        public bool? FinalizedForRus { get; set; }

        /// <summary>todo optimize perfomance by sortin eng rus clients</summary>
        /// <param name="addEndOfPacket"></param>
        /// <param name="isRus"></param>
        public void FinalizeAsda(bool addEndOfPacket, Locale locale)
        {
            if (locale == this.EncodedLocale)
                return;
            if (this.EncodedLocale == Locale.Any && addEndOfPacket)
            {
                this.WriteInt32(RealmPacketOut._cnter++);
                this.WriteInt16(0);
            }

            Asda2CryptHelper.XorData(this.BufferSegment.Buffer.Array, this.BufferSegment.Offset + 3, this.Position - 3L,
                this.EncodedLocale, locale);
            if (this.EncodedLocale == Locale.Any)
                this.WriteByte(254);
            short position = (short) this.Position;
            if (this.EncodedLocale == Locale.Any)
                Array.Copy((Array) BitConverter.GetBytes(position), 0, (Array) this.BufferSegment.Buffer.Array,
                    this.BufferSegment.Offset + 1, 2);
            this.EncodedLocale = locale;
            if (position >= (short) 2048)
                throw new InvalidOperationException(string.Format("Packet {0} Len >= 2048", (object) this));
        }

        /// <summary>Finalize packet data</summary>
        protected override void FinalizeWrite()
        {
            base.FinalizeWrite();
            this.Position = 0L;
            this.WriteUShortBE((ushort) ((ulong) this.BaseStream.Length - 2UL));
        }

        public void WriteSkip(byte[] bytes)
        {
            this.Write(bytes);
        }

        public void WriteFixedAsciiString(string str, int len, Locale locale = Locale.Start)
        {
            this.WriteAsdaString(str, len, locale);
        }

        public void WriteAsciiString(string msg, Locale locale = Locale.Start)
        {
            this.Write(Asda2EncodingHelper.Encode(msg, locale));
            this.WriteByte(0);
        }
    }
}