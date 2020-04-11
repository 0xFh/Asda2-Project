using System;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2TeleportHandler
    {
        private static readonly byte[] unk9 = new byte[96]
        {
            (byte) 199,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 211,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 212,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 134,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 135,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 190,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 189,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 172,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            (byte) 191,
            (byte) 78,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 5,
            (byte) 10,
            (byte) 0,
            (byte) 0,
            (byte) 199,
            (byte) 78,
            (byte) 211,
            (byte) 78,
            (byte) 212,
            (byte) 78,
            (byte) 134,
            (byte) 78,
            (byte) 135,
            (byte) 78,
            (byte) 190,
            (byte) 78,
            (byte) 189,
            (byte) 78,
            (byte) 172,
            (byte) 78,
            (byte) 191,
            (byte) 78,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 54,
            (byte) 2,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0
        };

        [PacketHandler(RealmServerOpCode.SaveLocation)]
        public static void SaveLocationRequest(IRealmClient client, RealmPacketIn packet)
        {
            string name = packet.ReadAsdaString(32, Locale.Start);
            ++packet.Position;
            int num1 = (int) packet.ReadByte();
            ushort pointNum = packet.ReadUInt16();
            int num2 = (int) packet.ReadInt16();
            int num3 = (int) packet.ReadInt16();
            if (pointNum > (ushort) 9)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to save teleportation point with id more than 9.",
                    50);
                Asda2TeleportHandler.SendLocationSavedResponse(client, LocationSavedStatus.Fail,
                    (Asda2TeleportingPointRecord) null, (short) 0);
            }
            else if (client.ActiveCharacter.TeleportPoints[(int) pointNum] == null)
            {
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                {
                    Asda2TeleportingPointRecord record = Asda2TeleportingPointRecord.CreateRecord(
                        client.ActiveCharacter.EntityId.Low, (short) client.ActiveCharacter.Position.X,
                        (short) client.ActiveCharacter.Position.Y, client.ActiveCharacter.MapId);
                    record.CreateLater();
                    client.ActiveCharacter.TeleportPoints[(int) pointNum] = record;
                    record.X = (short) client.ActiveCharacter.Position.X;
                    record.Y = (short) client.ActiveCharacter.Position.Y;
                    record.MapId = client.ActiveCharacter.MapId;
                    record.Name = name;
                    Asda2TeleportHandler.SendLocationSavedResponse(client, LocationSavedStatus.Ok, record,
                        (short) pointNum);
                }));
            }
            else
            {
                Asda2TeleportingPointRecord teleportPoint = client.ActiveCharacter.TeleportPoints[(int) pointNum];
                teleportPoint.X = (short) client.ActiveCharacter.Position.X;
                teleportPoint.Y = (short) client.ActiveCharacter.Position.Y;
                teleportPoint.MapId = client.ActiveCharacter.MapId;
                teleportPoint.Name = name;
                teleportPoint.SaveLater();
                Asda2TeleportHandler.SendLocationSavedResponse(client, LocationSavedStatus.Ok, teleportPoint,
                    (short) pointNum);
            }
        }

        public static void SendLocationSavedResponse(IRealmClient client, LocationSavedStatus status,
            Asda2TeleportingPointRecord rec, short pointNum)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemsFromAvatarWhRetrived))
            {
                packet.WriteByte((byte) status);
                packet.WriteFixedAsciiString(rec == null ? "" : rec.Name, 32, Locale.Start);
                packet.WriteByte(0);
                byte val = rec == null ? (byte) 0 : (byte) rec.MapId;
                packet.WriteByte(val);
                packet.WriteInt16(pointNum);
                packet.WriteInt16(rec == null ? 0 : (int) rec.X - 1000 * (int) val);
                packet.WriteInt16(rec == null ? 0 : (int) rec.Y - 1000 * (int) val);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.DeleteSavedLocation)]
        public static void DeleteSavedLocationRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte num = packet.ReadByte();
            if (client.ActiveCharacter.TeleportPoints[(int) num] == null)
            {
                client.ActiveCharacter.SendInfoMsg("Can't delete, point not founded.");
                Asda2TeleportHandler.SendSavedLocationDeletedResponse(client, LocationSavedStatus.Fail, (short) -1);
            }
            else
            {
                Asda2TeleportingPointRecord teleportPoint = client.ActiveCharacter.TeleportPoints[(int) num];
                client.ActiveCharacter.TeleportPoints[(int) num] = (Asda2TeleportingPointRecord) null;
                teleportPoint.DeleteLater();
                Asda2TeleportHandler.SendSavedLocationDeletedResponse(client, LocationSavedStatus.Ok, (short) num);
            }
        }

        public static void SendSavedLocationDeletedResponse(IRealmClient client, LocationSavedStatus status,
            short pointId = -1)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SavedLocationDeleted))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(pointId);
                client.Send(packet, false);
            }
        }

        public static void SendSavedLocationsInitResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SavedLocationsInit))
            {
                packet.WriteByte(1);
                for (int index = 0; index < client.ActiveCharacter.TeleportPoints.Length; ++index)
                {
                    Asda2TeleportingPointRecord teleportPoint = client.ActiveCharacter.TeleportPoints[index];
                    packet.WriteFixedAsciiString(teleportPoint == null ? "" : teleportPoint.Name, 32, Locale.Start);
                    packet.WriteByte(0);
                    byte val = teleportPoint == null ? (byte) 0 : (byte) teleportPoint.MapId;
                    packet.WriteByte(val);
                    packet.WriteInt16(teleportPoint == null ? -1 : index);
                    packet.WriteInt16(teleportPoint == null ? 0 : (int) teleportPoint.X - 1000 * (int) val);
                    packet.WriteInt16(teleportPoint == null ? 0 : (int) teleportPoint.Y - 1000 * (int) val);
                }

                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.DisplayItem)]
        public static void DisplayItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            ushort sessId = packet.ReadUInt16();
            byte num = packet.ReadByte();
            short slotInq = packet.ReadInt16();
            Character characterBySessionId = World.GetCharacterBySessionId(sessId);
            Asda2Item asda2Item = num == (byte) 1
                ? client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq)
                : client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq);
            if (asda2Item == null)
                client.ActiveCharacter.SendInfoMsg("Item not founded.");
            else
                Asda2TeleportHandler.SendItemDisplayedResponse(client.ActiveCharacter, asda2Item, characterBySessionId);
        }

        public static void SendItemDisplayedResponse(Character displayer, Asda2Item item, Character reciever)
        {
            if (reciever == null)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemDisplayed))
                {
                    packet.WriteInt32(displayer.AccId);
                    packet.WriteFixedAsciiString(displayer.Name, 20, Locale.Start);
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                    displayer.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                }
            }
            else
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemDisplayed))
                {
                    packet.WriteInt32(displayer.AccId);
                    packet.WriteFixedAsciiString(displayer.Name, 20, Locale.Start);
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                    reciever.Send(packet, false);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.GetCharecterInfo)]
        public static void GetCharecterInfoRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            Character characterBySessionId = World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null)
            {
                client.ActiveCharacter.SendInfoMsg("Character not founded.");
            }
            else
            {
                Asda2TeleportHandler.SendCharacterFullInfoResponse(client, characterBySessionId);
                Asda2TeleportHandler.SendCharacterRegularEquipmentInfoResponse(client, characterBySessionId);
            }
        }

        public static void SendCharacterFullInfoResponse(IRealmClient client, Character target)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(8U);
            switch (++progressRecord.Counter)
            {
                case 500:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Curious46)));
                    break;
                case 1000:
                    client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Curious46)));
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterFullInfo))
            {
                packet.WriteByte(target.Level);
                packet.WriteByte(target.ProfessionLevel);
                packet.WriteByte((byte) target.Archetype.ClassId);
                packet.WriteFixedAsciiString(target.Guild == null ? "" : target.Guild.Name, 17, Locale.Start);
                packet.WriteSkip(Asda2TeleportHandler.unk9);
                packet.WriteInt32(target.AccId);
                packet.WriteByte(3);
                for (int index = 0; index < 9; ++index)
                {
                    Asda2Item asda2Item = target.Asda2Inventory.Equipment[index + 11];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                client.Send(packet, true);
            }
        }

        public static void SendCharacterRegularEquipmentInfoResponse(IRealmClient client, Character target)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterRegularEquipmentInfo))
            {
                packet.WriteInt32(target.AccId);
                for (int index = 0; index < 11; ++index)
                {
                    Asda2Item asda2Item = target.Asda2Inventory.Equipment[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                client.Send(packet, true);
            }
        }
    }
}