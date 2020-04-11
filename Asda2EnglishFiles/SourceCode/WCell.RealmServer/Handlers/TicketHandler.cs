using WCell.Constants;
using WCell.Constants.Tickets;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    /// <summary>
    /// TODO: Check for existing tickets before creating/deleting
    /// TODO: Always set Character.Ticket and Ticket.Owner correspondingly
    /// TODO: Commands
    /// TODO: Save to DB
    /// TODO: Enable staff to list any ticket that was issued since serverstart and maybe save a backup copy to Ticket archive
    /// TODO: Attach Tickets to mail system
    /// TODO: Enforced Help-chat for fast ticket-handling etc
    /// </summary>
    public static class TicketHandler
    {
        public static void HandleSystemStatusPacket(IRealmClient client, RealmPacketIn packet)
        {
            using (RealmPacketOut packet1 =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GMTICKET_SYSTEMSTATUS, 4))
            {
                packet1.Write(1);
                client.Send(packet1, false);
            }
        }

        public static void HandleCreateTicketPacket(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.Ticket == null)
            {
                int num1 = (int) packet.ReadUInt32();
                double num2 = (double) packet.ReadFloat();
                double num3 = (double) packet.ReadFloat();
                double num4 = (double) packet.ReadFloat();
                string message = packet.ReadCString();
                TicketType type = (TicketType) packet.ReadUInt32();
                int num5 = (int) packet.ReadByte();
                int num6 = (int) packet.ReadUInt32();
                int num7 = (int) packet.ReadUInt32();
                Ticket ticket = new Ticket(activeCharacter, message, type);
                TicketMgr.Instance.AddTicket(ticket);
                activeCharacter.Ticket = ticket;
                TicketHandler.SendCreateResponse((IPacketReceiver) client, TicketInfoResponse.Saved);
            }
            else
                TicketHandler.SendCreateResponse((IPacketReceiver) client, TicketInfoResponse.Fail);
        }

        public static void HandleReportLagTicket(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadUInt32();
            int num2 = (int) packet.ReadUInt32();
            double num3 = (double) packet.ReadFloat();
            double num4 = (double) packet.ReadFloat();
            double num5 = (double) packet.ReadFloat();
        }

        public static void HandleGetTicketPacket(IRealmClient client, RealmPacketIn packet)
        {
            TicketHandler.SendGetTicketResponse((IPacketReceiver) client, client.ActiveCharacter.Ticket);
        }

        public static void HandleDeleteTicketPacket(IRealmClient client, RealmPacketIn packet)
        {
            Ticket ticket = client.ActiveCharacter.Ticket;
            if (ticket != null)
            {
                ticket.Delete();
                TicketHandler.SendDeleteResponse((IPacketReceiver) client, TicketInfoResponse.Deleted);
            }
            else
                TicketHandler.SendDeleteResponse((IPacketReceiver) client, TicketInfoResponse.NoTicket);
        }

        public static void HandleUpdateTicketPacket(IRealmClient client, RealmPacketIn packet)
        {
            Ticket ticket = client.ActiveCharacter.Ticket;
            if (ticket != null)
            {
                ticket.Message = packet.ReadCString();
                TicketHandler.SendUpdateResponse((IPacketReceiver) client, TicketInfoResponse.Saved);
            }
            else
                TicketHandler.SendUpdateResponse((IPacketReceiver) client, TicketInfoResponse.Fail);
        }

        public static void HandleResolveResponsePacket(IRealmClient client, RealmPacketIn packet)
        {
            TicketHandler.SendResolveResponse((IPacketReceiver) client, client.ActiveCharacter.Ticket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ticket">Can be null</param>
        public static void SendGetTicketResponse(IPacketReceiver client, Ticket ticket)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_GETTICKET))
            {
                if (ticket != null)
                {
                    packet.Write(6U);
                    packet.WriteCString(ticket.Message);
                    packet.Write((byte) ticket.Type);
                    packet.Write(0.0f);
                    packet.Write(0.0f);
                    packet.Write(0.0f);
                    packet.Write((ushort) 0);
                    client.Send(packet, false);
                }
                else
                    packet.Write(10U);
            }
        }

        public static void SendUpdateResponse(IPacketReceiver client, TicketInfoResponse response)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_UPDATETEXT))
            {
                packet.Write((uint) response);
                client.Send(packet, false);
            }
        }

        public static void SendCreateResponse(IPacketReceiver client, TicketInfoResponse response)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_CREATE))
            {
                packet.Write((uint) response);
                client.Send(packet, false);
            }
        }

        public static void SendDeleteResponse(IPacketReceiver client, TicketInfoResponse response)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_DELETETICKET))
            {
                packet.Write((uint) response);
                client.Send(packet, false);
            }
        }

        public static void SendResolveResponse(IPacketReceiver client, Ticket ticket)
        {
            ticket.Delete();
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GMTICKET_RESOLVE_RESPONSE, 4))
            {
                packet.WriteByte(0);
                client.Send(packet, false);
            }
        }
    }
}