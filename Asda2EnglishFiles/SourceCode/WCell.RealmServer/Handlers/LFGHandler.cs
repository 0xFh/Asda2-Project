using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    /// <summary>
    /// When opening the LFG panel, player sends:
    /// 	CMSG_LFG_GET_PLAYER_INFO
    /// 	CMSG_LFG_GET_PARTY_INFO
    /// </summary>
    public static class LFGHandler
    {
        public static void HandleJoin(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadUInt32();
            packet.SkipBytes(2);
            byte num2 = packet.ReadByte();
            if (num2 == (byte) 0)
                return;
            for (byte index = 0; (int) index < (int) num2; ++index)
                packet.ReadUInt32();
            byte num3 = packet.ReadByte();
            packet.SkipBytes((int) num3);
            packet.ReadCString();
        }

        public static void HandleLeave(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
        }

        public static void HandleSearchJoin(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
        }

        public static void HandleSearchLeave(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleProposalResponse(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
            packet.ReadBoolean();
        }

        public static void HandleSetComment(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadCString();
        }

        public static void HandleSetRoles(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadByte();
        }

        public static void HandleSetNeeds(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleBootPlayerVote(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadBoolean();
        }

        public static void HandleGetPlayerInfo(IRealmClient client, RealmPacketIn packet)
        {
            LFGHandler.SendPlayerInfo(client);
        }

        public static void HandleTeleport(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadBoolean();
        }

        public static void HandleGetPartyInfo(IRealmClient client, RealmPacketIn packet)
        {
            LFGHandler.SendPartyInfo(client);
        }

        public static void SendSearchResults(IRealmClient client)
        {
        }

        public static void SendProposalUpdate(IRealmClient client)
        {
        }

        public static void SendRoleCheckUpdate(IRealmClient client)
        {
        }

        public static void SendJoinResult(IRealmClient client)
        {
        }

        public static void SendQueueStatus(IRealmClient client)
        {
        }

        public static void SendUpdatePlayer(IRealmClient client)
        {
        }

        public static void SendUpdateParty(IRealmClient client)
        {
        }

        public static void SendUpdateSearch(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_UPDATE_SEARCH))
            {
                packet.Write(true);
                client.Send(packet, false);
            }
        }

        public static void SendBootPlayer(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_BOOT_PROPOSAL_UPDATE))
            {
                packet.WriteByte(true);
                packet.WriteByte(false);
                packet.WriteByte(true);
                packet.Write((ulong) EntityId.Zero);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.Write("Too noobzor for this l33t grpz");
                client.Send(packet, false);
            }
        }

        public static void SendPlayerInfo(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_PARTY_INFO))
            {
                byte num1 = 0;
                packet.Write(num1);
                uint num2 = 0;
                packet.Write(num2);
                packet.WriteByte(false);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                int val = 0;
                packet.WriteByte(val);
                for (byte index = 0; (int) index < val; ++index)
                {
                    packet.WriteUInt(0);
                    packet.WriteUInt(0);
                    packet.WriteUInt(0);
                }

                uint num3 = 0;
                packet.Write(num3);
                for (uint index = 0; index < num3; ++index)
                {
                    packet.Write(num2);
                    packet.Write(6U);
                }

                client.Send(packet, false);
            }
        }

        public static void SendPartyInfo(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LFG_PARTY_INFO))
            {
                byte num = 0;
                packet.Write(num);
                for (byte index = 0; (int) index < (int) num; ++index)
                    packet.Write((ulong) EntityId.Zero);
                client.Send(packet, false);
            }
        }
    }
}