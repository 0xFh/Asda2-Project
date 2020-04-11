using System;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2Titles
{
    public static class Asda2TitlesHandler
    {
        public static void SendDiscoveredTitlesResponse(IRealmClient client)
        {
            if (client.ActiveCharacter.DiscoveredTitles == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.DiscoveredTitles))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                client.ActiveCharacter.DiscoveredTitles.WriteToAsda2Packet((PrimitiveWriter) packet);
                packet.WriteInt16(1);
                packet.WriteInt16(10);
                packet.WriteInt16(2);
                packet.WriteInt16(20);
                packet.WriteInt16(3);
                packet.WriteInt16(40);
                client.Send(packet, true);
            }
        }

        public static void SendGetedTitlesResponse(IRealmClient client)
        {
            if (client.ActiveCharacter.GetedTitles == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.GetedTitles))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt32(client.ActiveCharacter.Asda2TitlePoints);
                client.ActiveCharacter.GetedTitles.WriteToAsda2Packet((PrimitiveWriter) packet);
                client.Send(packet, true);
            }
        }

        public static void SendYouGetNewTitleResponse(Character chr, short newTitleId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.YouGetNewTitle))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(chr.AccId);
                packet.WriteInt16(newTitleId);
                packet.WriteByte(0);
                packet.WriteInt32(chr.Asda2TitlePoints);
                chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendTitleDiscoveredResponse(IRealmClient client, short titleId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TitleDiscovered))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt16(titleId);
                client.ActiveCharacter.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        [PacketHandler(RealmServerOpCode.SetTitle)]
        public static void SetTitleRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            short num1 = packet.ReadInt16();
            short num2 = packet.ReadInt16();
            if (num1 != (short) -1)
            {
                if ((Decimal) num1 > new Decimal(418) || num1 < (short) -1)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Wrong title id : " + (object) num1, 1);
                    return;
                }

                if (!client.ActiveCharacter.GetedTitles.GetBit((int) num1))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Tries to set not owned title id : " + (object) num1,
                        1);
                    return;
                }

                client.ActiveCharacter.Record.PreTitleId = num1;
            }

            if (num2 != (short) -1)
            {
                if ((Decimal) num2 > new Decimal(418) || num2 < (short) -1)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Wrong title id : " + (object) num1, 1);
                    return;
                }

                if (!client.ActiveCharacter.GetedTitles.GetBit((int) num2))
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Tries to set not owned title id : " + (object) num2,
                        1);
                    return;
                }

                client.ActiveCharacter.Record.PostTitleId = num2;
            }

            GlobalHandler.BroadcastCharacterPlaceInTitleRatingResponse(client.ActiveCharacter);
        }
    }
}