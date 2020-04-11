using Cell.Core;
using NLog;
using System;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.UpdateFields
{
    /// <summary>TODO: Create fully customizable UpdatePacket class</summary>
    public class UpdatePacket : RealmPacketOut
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public const int DefaultCapacity = 1024;

        public UpdatePacket()
            : base(RealmServerOpCode.SMSG_UPDATE_OBJECT)
        {
            this.Position = 8L;
        }

        public UpdatePacket(int maxContentLength)
            : base((PacketId) RealmServerOpCode.SMSG_UPDATE_OBJECT, maxContentLength + 8)
        {
            this.Position = 8L;
        }

        /// <summary>Sends packet (might be compressed)</summary>
        /// <returns></returns>
        public void SendTo(IRealmClient client)
        {
        }

        public void Reset()
        {
            this.Position = 2L;
            this.Write((ushort) this.m_id.RawId);
            this.Zero(5);
        }

        private static void SendPacket(IRealmClient client, BufferSegment outputBuffer, int totalLength,
            int actualLength)
        {
            uint offset = (uint) outputBuffer.Offset;
            outputBuffer.Buffer.Array.SetUShortBE(offset, (ushort) (totalLength - 2));
            outputBuffer.Buffer.Array.SetBytes(offset + 2U, BitConverter.GetBytes((ushort) 502));
            outputBuffer.Buffer.Array.SetBytes(offset + 4U, BitConverter.GetBytes((uint) actualLength));
            client.Send(outputBuffer, totalLength);
        }
    }
}