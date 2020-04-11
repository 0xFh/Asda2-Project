using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
    internal class GlobalHandler
    {
        private static readonly byte[] unk81 = new byte[6]
        {
            (byte) 250,
            (byte) 20,
            (byte) 124,
            (byte) 80,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stub80 = new byte[16]
        {
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue
        };

        private static readonly byte[] Unk13 = new byte[40]
        {
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue
        };

        private static readonly byte[] stub12 = new byte[96]
        {
            (byte) 0,
            (byte) 0,
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
            (byte) 0,
            (byte) 0,
            (byte) 0,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
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
            byte.MaxValue,
            byte.MaxValue
        };

        private static readonly byte[] Stab23 = new byte[2];

        private static readonly byte[] Stab33 = new byte[120]
        {
            (byte) 4,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
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
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] Stab34 = new byte[495]
        {
            (byte) 4,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 3,
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
            (byte) 2,
            (byte) 0,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 101,
            (byte) 0,
            (byte) 96,
            (byte) 2,
            (byte) 91,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 60,
            (byte) 1,
            (byte) 250,
            (byte) 0,
            (byte) 60,
            (byte) 1,
            (byte) 250,
            (byte) 0,
            (byte) 4,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 4,
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
            (byte) 2,
            (byte) 0,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 104,
            (byte) 0,
            (byte) 96,
            (byte) 2,
            (byte) 91,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1,
            (byte) 1,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 50,
            (byte) 1,
            (byte) 3,
            (byte) 1,
            (byte) 50,
            (byte) 1,
            (byte) 3,
            (byte) 1,
            (byte) 4,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
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
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 5,
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
            (byte) 2,
            (byte) 0,
            (byte) 1,
            (byte) 0,
            (byte) 0
        };

        public static void SendGlobalMessage(string message, Color color)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MessageServerAboutWarStarts))
            {
                packet.WriteInt32(0);
                packet.WriteFixedAsciiString(message, 100, Locale.Start);
                packet.WriteByte(color.R);
                packet.WriteByte(color.G);
                packet.WriteByte(color.B);
                packet.WriteSkip(GlobalHandler.unk81);
                WCell.RealmServer.Global.World.Broadcast(packet, false, Locale.Any);
            }
        }

        public static void SendGlobalMessage(string message)
        {
            GlobalHandler.SendGlobalMessage(message, Color.Yellow);
        }

        [PacketHandler(RealmServerOpCode.IDontKnowAboutCHaracter)]
        public static void DontKnowAboutCHaracterRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character characterBySessionId =
                WCell.RealmServer.Global.World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null)
                return;
            GlobalHandler.SendCharacterVisibleNowResponse(client, characterBySessionId);
        }

        public static void SendTransformToPetResponse(Character chr, bool isInto, IRealmClient reciver = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TransformToPet))
            {
                packet.WriteByte(isInto ? 1 : 0);
                packet.WriteInt32(chr.AccId);
                packet.WriteInt16(chr.TransformationId);
                if (reciver != null)
                    reciver.Send(packet, true);
                else
                    chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendCharacterDeleteResponse(Character chr, IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterDelete))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(chr.AccId);
                if (client == null)
                    chr.SendPacketToArea(packet, false, true, Locale.Any, new float?());
                else
                    client.Send(packet, true);
            }
        }

        public static void SendCharacterVisibleNowResponse(IRealmClient client, Character visibleChr)
        {
            if (visibleChr == null)
                return;
            if (visibleChr.Record == null)
                throw new ArgumentException("cant be null", "visibleChr.Record");
            if (client == null)
                throw new ArgumentException("cant be null", nameof(client));
            if (client.ActiveCharacter == null)
                throw new ArgumentException("cant be null", "client.ActiveCharacter");
            if (client.ActiveCharacter.Map == null)
                throw new ArgumentException("cant be null", "client.ActiveCharacter.Map");
            if (visibleChr.SettingsFlags == null)
                throw new ArgumentException("cant be null", "visibleChr.SettingsFlags");
            if (visibleChr.Archetype == null)
                throw new ArgumentException("cant be null", "visibleChr.Archetype");
            if (visibleChr.Archetype.Class == null)
                throw new ArgumentException("cant be null", "visibleChr.Archetype.Class");
            if (visibleChr.Asda2Inventory == null)
                throw new ArgumentException("cant be null", "visibleChr.Asda2Inventory");
            if (visibleChr.Asda2Inventory.Equipment == null)
                throw new ArgumentException("cant be null", "visibleChr.Asda2Inventory.Equipment");
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterVisibleNow))
            {
                packet.WriteInt16(visibleChr.SessionId);
                packet.WriteInt16((short) visibleChr.Asda2X);
                packet.WriteInt16((short) visibleChr.Asda2Y);
                packet.WriteByte(visibleChr.SettingsFlags[15]);
                packet.WriteByte(visibleChr.AvatarMask);
                packet.WriteInt32(Utility.Random(0, int.MaxValue));
                packet.WriteInt32(visibleChr.AccId);
                packet.WriteFixedAsciiString(visibleChr.Name, 20, Locale.Start);
                packet.WriteByte(visibleChr.CharNum);
                packet.WriteByte(0);
                packet.WriteByte((byte) visibleChr.Gender);
                packet.WriteByte(visibleChr.ProfessionLevel);
                packet.WriteByte((byte) visibleChr.Archetype.Class.Id);
                packet.WriteByte(visibleChr.Level);
                packet.WriteInt16(visibleChr.IsAsda2BattlegroundInProgress ? 2 : 0);
                packet.WriteInt16(visibleChr.CurrentBattleGroundId);
                packet.WriteByte(visibleChr.Asda2FactionId == (short) -1 ? 0 : (int) visibleChr.Asda2FactionId);
                packet.WriteByte(visibleChr.IsAsda2BattlegroundInProgress ? 1 : 0);
                packet.WriteByte(visibleChr.Record.Zodiac);
                packet.WriteByte(visibleChr.HairStyle);
                packet.WriteByte(visibleChr.HairColor);
                packet.WriteByte(visibleChr.Record.Face);
                packet.WriteByte(visibleChr.EyesColor);
                packet.WriteInt16(visibleChr.SessionId);
                packet.WriteInt32(-1);
                packet.WriteInt16(visibleChr.IsDead ? 200 : (visibleChr.IsSitting ? 108 : 0));
                packet.WriteFloat(visibleChr.Asda2X);
                packet.WriteFloat(visibleChr.Asda2Y);
                packet.WriteFloat(visibleChr.IsMoving ? visibleChr.LastNewPosition.X : 0.0f);
                packet.WriteFloat(visibleChr.IsMoving ? visibleChr.LastNewPosition.Y : 0.0f);
                packet.WriteFloat(visibleChr.IsMoving ? visibleChr.Orientation : 0.0f);
                packet.WriteSkip(GlobalHandler.stub80);
                packet.WriteInt32(0);
                for (int index = 0; index < 20; ++index)
                {
                    Asda2Item asda2Item = visibleChr.Asda2Inventory.Equipment[index];
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(asda2Item == null ? 0 : (int) asda2Item.Enchant);
                }

                client.Send(packet, true);
            }

            if (!visibleChr.IsMoving || client.ActiveCharacter.Map == null)
                return;
            client.ActiveCharacter.Map.CallDelayed(200,
                (Action) (() => Asda2MovmentHandler.SendStartMoveCommonToOneClienResponset(visibleChr, client, false)));
        }

        public static void SendSpeedChangedResponse(IRealmClient client)
        {
            if (client.ActiveCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SpeedChanged))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteFloat(client.ActiveCharacter.RunSpeed);
                client.ActiveCharacter.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendCharacterPlaceInTitleRatingResponse(IRealmClient client, Character chr)
        {
            using (RealmPacketOut placeInRaitingPacket = GlobalHandler.CreateCharacterPlaceInRaitingPacket(chr))
                client.Send(placeInRaitingPacket, true);
        }

        public static void BroadcastCharacterPlaceInTitleRatingResponse(Character chr)
        {
            using (RealmPacketOut placeInRaitingPacket = GlobalHandler.CreateCharacterPlaceInRaitingPacket(chr))
                chr.SendPacketToArea(placeInRaitingPacket, true, true, Locale.Any, new float?());
        }

        public static RealmPacketOut CreateCharacterPlaceInRaitingPacket(Character chr)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.CharacterPlaceInTitleRating);
            realmPacketOut.WriteInt16(chr.SessionId);
            realmPacketOut.WriteInt32(chr.AccId);
            realmPacketOut.WriteInt32(chr.Asda2Rank);
            realmPacketOut.WriteInt16(chr.Record.PreTitleId);
            realmPacketOut.WriteInt16(chr.Record.PostTitleId);
            return realmPacketOut;
        }

        public static void SendCharacterFriendShipResponse(IRealmClient client, Character chr)
        {
            if (chr.SoulmateRecord == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterFriendShip))
            {
                packet.WriteInt32(chr.AccId);
                packet.WriteInt32((int) chr.SoulmateRecord.SoulmateRelationGuid);
                packet.WriteInt32(chr.SoulmateRecord.FriendAccId(chr.AccId));
                packet.WriteInt16(0);
                client.Send(packet, false);
            }
        }

        public static void SendCharacterInfoPetResponse(IRealmClient client, Character chr)
        {
            using (RealmPacketOut characterPetInfoPacket = GlobalHandler.CreateCharacterPetInfoPacket(chr))
                client.Send(characterPetInfoPacket, true);
        }

        public static void UpdateCharacterPetInfoToArea(Character chr)
        {
            using (RealmPacketOut characterPetInfoPacket = GlobalHandler.CreateCharacterPetInfoPacket(chr))
                chr.SendPacketToArea(characterPetInfoPacket, true, true, Locale.Any, new float?());
        }

        public static RealmPacketOut CreateCharacterPetInfoPacket(Character chr)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.CharacterInfoPet);
            realmPacketOut.WriteInt32(chr.AccId);
            realmPacketOut.WriteInt16(chr.Asda2Pet == null ? -1 : (int) chr.Asda2Pet.Id);
            realmPacketOut.WriteByte(chr.Asda2Pet == null ? 0 : (int) chr.Asda2Pet.Level);
            realmPacketOut.WriteFixedAsciiString(chr.Asda2Pet == null ? "" : chr.Asda2Pet.Name, 16, Locale.Start);
            return realmPacketOut;
        }

        public static void SendCharacterInfoClanNameResponse(IRealmClient client, Character chr)
        {
            if (chr.Guild == null)
                return;
            using (RealmPacketOut characterInfoClanName = GlobalHandler.CreateCharacterInfoClanName(chr))
                client.Send(characterInfoClanName, true);
        }

        public static void SendCharactrerInfoClanNameToAllNearbyCharacters(Character chr)
        {
            if (chr.Guild == null)
                return;
            using (RealmPacketOut characterInfoClanName = GlobalHandler.CreateCharacterInfoClanName(chr))
                chr.SendPacketToArea(characterInfoClanName, true, true, Locale.Any, new float?());
        }

        public static RealmPacketOut CreateCharacterInfoClanName(Character chr)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.CharacterInfoClanName);
            realmPacketOut.WriteInt32(chr.AccId);
            realmPacketOut.WriteInt16(chr.GuildId);
            realmPacketOut.WriteInt16(chr.Guild.Level);
            realmPacketOut.WriteFixedAsciiString(chr.Guild.Name, 16, Locale.Start);
            realmPacketOut.WriteByte(0);
            realmPacketOut.WriteByte(3);
            realmPacketOut.WriteByte(chr.Guild.ClanCrest[0] != (byte) 0 ? 1 : 0);
            realmPacketOut.WriteSkip(chr.Guild.ClanCrest[0] != (byte) 0 ? chr.Guild.ClanCrest : GlobalHandler.Unk13);
            return realmPacketOut;
        }

        public static void SendCharacterFactionAndFactionRankResponse(IRealmClient client, Character chr)
        {
            using (RealmPacketOut characterFactionPacket = GlobalHandler.CreateCharacterFactionPacket(chr))
                client.Send(characterFactionPacket, false);
        }

        public static void SendCharacterFactionToNearbyCharacters(Character chr)
        {
            using (RealmPacketOut characterFactionPacket = GlobalHandler.CreateCharacterFactionPacket(chr))
                chr.SendPacketToArea(characterFactionPacket, true, false, Locale.Any, new float?());
        }

        public static RealmPacketOut CreateCharacterFactionPacket(Character chr)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.CharacterFactionAndFactionRank);
            realmPacketOut.WriteInt32(chr.AccId);
            realmPacketOut.WriteInt16(chr.SessionId);
            realmPacketOut.WriteInt16(chr.Asda2FactionId);
            realmPacketOut.WriteByte(chr.Asda2FactionRank);
            return realmPacketOut;
        }

        public static void SendNpcVisiableNowResponse(IRealmClient client, GameObject npc)
        {
            if (npc.EntityId.Entry == 145U)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.NpcVisiableNow))
            {
                packet.WriteInt16(npc.UniqIdOnMap);
                packet.WriteInt16(npc.EntryId);
                packet.WriteInt32(npc.UniqWorldEntityId);
                packet.WriteInt16((short) npc.Asda2X);
                packet.WriteInt16((short) npc.Asda2Y);
                packet.WriteSkip(GlobalHandler.stub12);
                client.Send(packet, false);
            }
        }

        public static void SendMonstrVisibleNowResponse(IRealmClient client, NPC visibleNpc)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MonstVisible))
            {
                packet.WriteInt16(visibleNpc.UniqIdOnMap);
                packet.WriteInt16((ushort) visibleNpc.Entry.NPCId);
                packet.WriteInt32(91);
                packet.WriteByte(2);
                packet.WriteInt32(visibleNpc.Health);
                float num = visibleNpc.Movement.MoveType == AIMoveType.Walk
                    ? visibleNpc.WalkSpeed
                    : visibleNpc.RunSpeed;
                packet.WriteInt16((short) (1000.0 / (double) num));
                packet.WriteInt16(15000);
                packet.WriteSkip(GlobalHandler.Stab23);
                packet.WriteInt16((short) visibleNpc.Asda2X);
                packet.WriteInt16((short) visibleNpc.Asda2Y);
                if (visibleNpc.IsMoving)
                {
                    packet.WriteInt16((int) (short) visibleNpc.Movement.Destination.X - (int) visibleNpc.MapId * 1000);
                    packet.WriteInt16((int) (short) visibleNpc.Movement.Destination.Y - (int) visibleNpc.MapId * 1000);
                }
                else
                {
                    packet.WriteInt16((short) visibleNpc.Asda2X);
                    packet.WriteInt16((short) visibleNpc.Asda2Y);
                }

                packet.WriteSkip(GlobalHandler.Stab33);
                packet.WriteInt32(91);
                packet.WriteInt16(47);
                packet.WriteInt16(150);
                packet.WriteInt32(0);
                packet.WriteInt16(1500);
                packet.WriteInt16(1000);
                packet.WriteInt16(1000);
                client.Send(packet, false);
            }
        }

        public static void SendBuffsOnCharacterInfoResponse(IRealmClient rcv, Character chr)
        {
            if (chr.Auras == null || chr.Auras.ActiveAuras.Length == 0)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.BuffsOnCharacterInfo))
            {
                packet.WriteInt16(chr.SessionId);
                Aura[] activeAuras = chr.Auras.ActiveAuras;
                int length = activeAuras.Length;
                for (int index = 0; index < length; ++index)
                {
                    Aura aura = activeAuras[index];
                    packet.WriteInt16(0);
                    packet.WriteInt32(aura.Spell.RealId);
                    packet.WriteInt32(aura.Duration / 1000);
                    packet.WriteInt16(0);
                }

                if (chr.IsOnTransport)
                {
                    packet.WriteInt16(2);
                    packet.WriteInt32(chr.TransportItemId);
                    packet.WriteInt32(-1);
                    packet.WriteInt16(60);
                }

                rcv.Send(packet, false);
            }
        }

        public static void SendItemVisible(Character character, Asda2Loot loot)
        {
            foreach (Asda2LootItem asda2LootItem in loot.Items)
            {
                if (!asda2LootItem.Taken)
                {
                    using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemDroped))
                    {
                        packet.WriteInt32(asda2LootItem.Template.Id);
                        packet.WriteInt16(asda2LootItem.Position.X);
                        packet.WriteInt16(asda2LootItem.Position.Y);
                        packet.WriteInt32(character.SessionId);
                        packet.WriteByte(10);
                        character.Client.Send(packet, false);
                    }
                }
            }
        }

        public static void SendRemoveItemResponse(Asda2LootItem item)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RemoveItemFromWorld))
            {
                packet.WriteInt16(item.Position.X);
                packet.WriteInt16(item.Position.Y);
                packet.WriteInt32((int) item.Template.ItemId);
                item.Loot.SendPacketToArea(packet, false, true, Locale.Any, new float?());
            }
        }

        public static void SendRemoveItemResponse(IRealmClient receiver, short x, short y)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RemoveItemFromWorld))
            {
                packet.WriteInt16(x);
                packet.WriteInt16(y);
                receiver.Send(packet, true);
            }
        }

        public static void SendRemoveLootResponse(Character chr, Asda2Loot loot)
        {
            foreach (Asda2LootItem asda2LootItem in loot.Items)
            {
                if (!asda2LootItem.Taken)
                {
                    using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RemoveItemFromWorld))
                    {
                        packet.WriteInt16(asda2LootItem.Position.X);
                        packet.WriteInt16(asda2LootItem.Position.Y);
                        loot.SendPacketToArea(packet, false, true, Locale.Any, new float?());
                    }
                }
            }
        }

        [PacketHandler(RealmServerOpCode.WhoIsHere)]
        public static void WhoIsHereRequest(IRealmClient client, RealmPacketIn packet)
        {
            GlobalHandler.SendWhoIsHereListResponse(client);
        }

        public static void SendWhoIsHereListResponse(IRealmClient client)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(2U);
            switch (++progressRecord.Counter)
            {
                case 50:
                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Boring39);
                    break;
                case 100:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Boring39);
                    break;
            }

            progressRecord.SaveAndFlush();
            List<Character> list = client.ActiveCharacter.Map.Characters.Where<Character>((Func<Character, bool>) (c =>
            {
                if (c.Level >= client.ActiveCharacter.Level - 9)
                    return c.Level <= client.ActiveCharacter.Level + 9;
                return false;
            })).ToList<Character>();
            list.Remove(client.ActiveCharacter);
            List<List<Character>> characterListList = new List<List<Character>>();
            int num = list.Count / 8;
            if (num == 0 && list.Count != 0)
                num = 1;
            for (int index = 0; index < num; ++index)
                characterListList.Add(new List<Character>(list.Skip<Character>(index * 8).Take<Character>(8)));
            foreach (List<Character> characterList in characterListList)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WhoIsHereList))
                {
                    for (int index = 0; index < characterList.Count; ++index)
                    {
                        Character character = characterList[index];
                        packet.WriteByte(character.Level);
                        packet.WriteByte(character.ProfessionLevel);
                        packet.WriteByte((byte) character.Archetype.ClassId);
                        packet.WriteFixedAsciiString(character.Name, 20, Locale.Start);
                        packet.WriteInt32(-1);
                        packet.WriteByte(0);
                        packet.WriteInt16(character.SessionId);
                        packet.WriteInt32(character.AccId);
                        packet.WriteByte(character.CharNum);
                    }

                    client.Send(packet, false);
                }
            }

            GlobalHandler.SendWhoIsHereListEndedResponse(client);
        }

        public static void SendWhoIsHereListEndedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WhoIsHereListEnded))
                client.Send(packet, true);
        }

        [PacketHandler(RealmServerOpCode.AddFriend)]
        public static void AddFriendRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character characterBySessionId =
                WCell.RealmServer.Global.World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null)
            {
                client.ActiveCharacter.SendInfoMsg("Target character not found.");
                GlobalHandler.SendFriendAddedResponse(client, false, (Character) null);
            }
            else
            {
                characterBySessionId.CurrentFriendInviter = client.ActiveCharacter;
                GlobalHandler.SendInviteFromeSomeoneFriendResponse(characterBySessionId, client.ActiveCharacter);
            }
        }

        public static void SendFriendAddedResponse(IRealmClient client, bool success, Character friend)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FriendAdded))
            {
                packet.WriteByte(success);
                packet.WriteInt32(friend == null ? 0U : friend.AccId);
                packet.WriteByte(friend == null ? 0 : (int) friend.CharNum);
                packet.WriteByte(0);
                packet.WriteByte(friend == null ? (byte) 0 : (byte) friend.MapId);
                packet.WriteByte(friend == null ? 0 : friend.Level);
                packet.WriteByte(friend == null ? 0 : (int) friend.ProfessionLevel);
                packet.WriteByte(friend == null ? (byte) 0 : (byte) friend.Archetype.ClassId);
                packet.WriteByte(1);
                packet.WriteFixedAsciiString(friend == null ? "" : friend.Name, 20, Locale.Start);
                client.Send(packet, true);
            }
        }

        public static void SendInviteFromeSomeoneFriendResponse(Character invitee, Character inviter)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.InviteFromeSomeoneFriend))
            {
                packet.WriteInt32(-1);
                packet.WriteFixedAsciiString(inviter.Name, 20, Locale.Start);
                packet.WriteInt16(inviter.SessionId);
                invitee.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.InviteFriendAnswer)]
        public static void InviteFriendAnswerRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 16;
            ushort sessId = packet.ReadUInt16();
            if (packet.ReadByte() != (byte) 1)
            {
                if (client.ActiveCharacter.CurrentFriendInviter != null)
                    client.ActiveCharacter.CurrentFriendInviter.SendInfoMsg(
                        "Failed to make friend with " + client.ActiveCharacter.Name);
            }
            else
            {
                Character inviter = WCell.RealmServer.Global.World.GetCharacterBySessionId(sessId);
                if (inviter == client.ActiveCharacter)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to friendshinp with him self.", 80);
                    return;
                }

                if (inviter == null)
                {
                    client.ActiveCharacter.SendInfoMsg("Target character not found.");
                }
                else
                {
                    if (client.ActiveCharacter.Friends.ContainsKey(inviter.AccId) ||
                        inviter.Friends.ContainsKey(client.ActiveCharacter.AccId))
                    {
                        inviter.SendSystemMessage("Already friends with " + client.ActiveCharacter.Name);
                        client.ActiveCharacter.SendSystemMessage("Already friends with " + inviter.Name);
                        client.ActiveCharacter.CurrentFriendInviter = (Character) null;
                        return;
                    }

                    if (inviter != client.ActiveCharacter.CurrentFriendInviter)
                    {
                        if (client.ActiveCharacter.CurrentFriendInviter != null)
                            client.ActiveCharacter.CurrentFriendInviter.SendInfoMsg(
                                "Failed to make friend with " + client.ActiveCharacter.Name);
                    }
                    else
                    {
                        ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                        {
                            Asda2FriendshipRecord record = new Asda2FriendshipRecord(client.ActiveCharacter, inviter);
                            record.CreateLater();
                            lock (client.ActiveCharacter.Friends)
                            {
                                client.ActiveCharacter.Friends.Add(inviter.AccId, inviter.Record);
                                client.ActiveCharacter.FriendRecords.Add(record);
                            }

                            lock (inviter.Friends)
                            {
                                inviter.Friends.Add(client.ActiveCharacter.AccId, client.ActiveCharacter.Record);
                                inviter.FriendRecords.Add(record);
                            }
                        }));
                        GlobalHandler.SendFriendAddedResponse(inviter.Client, true, client.ActiveCharacter);
                        GlobalHandler.SendFriendAddedResponse(client, true, inviter);
                        AchievementProgressRecord progressRecord1 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(11U);
                        switch (++progressRecord1.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Popular49);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Popular49);
                                break;
                        }

                        progressRecord1.SaveAndFlush();
                        AchievementProgressRecord progressRecord2 =
                            inviter.Client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(11U);
                        switch (++progressRecord2.Counter)
                        {
                            case 50:
                                inviter.Client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Popular49);
                                break;
                            case 100:
                                inviter.Client.ActiveCharacter.GetTitle(Asda2TitleId.Popular49);
                                break;
                        }

                        progressRecord2.SaveAndFlush();
                    }
                }
            }

            client.ActiveCharacter.CurrentFriendInviter = (Character) null;
        }

        [PacketHandler(RealmServerOpCode.ShowFriendList)]
        public static void ShowFriendListRequest(IRealmClient client, RealmPacketIn packet)
        {
            GlobalHandler.SendFriendListResponse(client);
        }

        public static void SendFriendListResponse(IRealmClient client)
        {
            lock (client.ActiveCharacter.Friends)
            {
                foreach (CharacterRecord characterRecord in client.ActiveCharacter.Friends.Values)
                {
                    Character character = WCell.RealmServer.Global.World.GetCharacter(characterRecord.EntityLowId);
                    using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FriendList))
                    {
                        packet.WriteUInt32(characterRecord.AccountId);
                        packet.WriteByte(characterRecord.CharNum);
                        packet.WriteByte(1);
                        packet.WriteByte(character == null ? (byte) characterRecord.MapId : (byte) character.MapId);
                        packet.WriteByte(character == null ? characterRecord.Level : character.Level);
                        packet.WriteByte(
                            character == null ? characterRecord.ProfessionLevel : character.ProfessionLevel);
                        packet.WriteByte(character == null
                            ? (byte) characterRecord.Class
                            : (byte) character.Archetype.ClassId);
                        packet.WriteByte(character == null ? 0 : 1);
                        packet.WriteFixedAsciiString(character == null ? characterRecord.Name : character.Name, 20,
                            Locale.Start);
                        client.Send(packet, false);
                    }
                }
            }

            GlobalHandler.SendFriendListEndedResponse(client);
        }

        public static void SendFriendListEndedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FriendListEnded))
                client.Send(packet, true);
        }

        [PacketHandler(RealmServerOpCode.DeleteFromFriendList)]
        public static void DeleteFromFriendListRequest(IRealmClient client, RealmPacketIn packet)
        {
            uint targetAccId = packet.ReadUInt32();
            byte charNum = packet.ReadByte();
            if (client.ActiveCharacter.Friends.ContainsKey(targetAccId))
            {
                lock (client.ActiveCharacter.Friends)
                {
                    Character characterByAccId = WCell.RealmServer.Global.World.GetCharacterByAccId(targetAccId);
                    if (characterByAccId != null)
                    {
                        if (characterByAccId.Friends.ContainsKey(client.ActiveCharacter.AccId))
                        {
                            characterByAccId.Friends.Remove(client.ActiveCharacter.AccId);
                            Asda2FriendshipRecord friendshipRecord1 =
                                characterByAccId.FriendRecords.FirstOrDefault<Asda2FriendshipRecord>(
                                    (Func<Asda2FriendshipRecord, bool>) (f =>
                                    {
                                        if ((int) f.FirstCharacterAccId == (int) client.ActiveCharacter.EntityId.Low)
                                            return (int) f.SecondCharacterAccId == (int) targetAccId;
                                        return false;
                                    }));
                            Asda2FriendshipRecord friendshipRecord2 =
                                characterByAccId.FriendRecords.FirstOrDefault<Asda2FriendshipRecord>(
                                    (Func<Asda2FriendshipRecord, bool>) (f =>
                                    {
                                        if ((int) f.FirstCharacterAccId == (int) targetAccId)
                                            return (int) f.SecondCharacterAccId ==
                                                   (int) client.ActiveCharacter.EntityId.Low;
                                        return false;
                                    }));
                            if (friendshipRecord1 != null)
                                characterByAccId.FriendRecords.Remove(friendshipRecord1);
                            if (friendshipRecord2 != null)
                                characterByAccId.FriendRecords.Remove(friendshipRecord2);
                        }

                        characterByAccId.SendInfoMsg(string.Format("{0} is no longer your friend.",
                            (object) client.ActiveCharacter.Name));
                    }

                    client.ActiveCharacter.Friends.Remove(targetAccId);
                    Asda2FriendshipRecord record1 =
                        client.ActiveCharacter.FriendRecords.FirstOrDefault<Asda2FriendshipRecord>(
                            (Func<Asda2FriendshipRecord, bool>) (f =>
                            {
                                if ((int) f.FirstCharacterAccId == (int) client.ActiveCharacter.EntityId.Low)
                                    return (int) f.SecondCharacterAccId == (int) targetAccId;
                                return false;
                            }));
                    Asda2FriendshipRecord record2 =
                        client.ActiveCharacter.FriendRecords.FirstOrDefault<Asda2FriendshipRecord>(
                            (Func<Asda2FriendshipRecord, bool>) (f =>
                            {
                                if ((int) f.FirstCharacterAccId == (int) targetAccId)
                                    return (int) f.SecondCharacterAccId == (int) client.ActiveCharacter.EntityId.Low;
                                return false;
                            }));
                    if (record1 != null)
                    {
                        client.ActiveCharacter.FriendRecords.Remove(record1);
                        record1.DeleteLater();
                    }

                    if (record2 != null)
                    {
                        client.ActiveCharacter.FriendRecords.Remove(record2);
                        record2.DeleteLater();
                    }
                }
            }

            GlobalHandler.SendDeletedFromFriendListResponse(client, targetAccId, charNum);
        }

        public static void SendDeletedFromFriendListResponse(IRealmClient client, uint accId, byte charNum)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.DeletedFromFriendList))
            {
                packet.WriteByte(1);
                packet.WriteInt32(accId);
                packet.WriteByte(charNum);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.TeleportByCristal)]
        public static void TeleportByCristalRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte num = packet.ReadByte();
            if (!Asda2TeleportMgr.Teleports.ContainsKey((int) num))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to use teleport crystal to unknown location.", 10);
            }
            else
            {
                Asda2TeleportCristalVector teleport = Asda2TeleportMgr.Teleports[(int) num];
                if (!client.ActiveCharacter.SubtractMoney((uint) teleport.Price))
                {
                    client.ActiveCharacter.SendInfoMsg("Not enoght money to teleport.");
                }
                else
                {
                    AchievementProgressRecord progressRecord =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(3U);
                    switch (++progressRecord.Counter)
                    {
                        case 500:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Lazy40);
                            break;
                        case 1000:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Lazy40);
                            break;
                    }

                    progressRecord.SaveAndFlush();
                    client.ActiveCharacter.SendMoneyUpdate();
                    client.ActiveCharacter.TeleportTo(teleport.ToMap, teleport.To);
                }
            }
        }

        public static void SendTeleportedByCristalResponse(IRealmClient client, MapId map, short x, short y,
            TeleportByCristalStaus staus = TeleportByCristalStaus.Ok)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TeleportedByCristal))
            {
                packet.WriteByte((byte) staus);
                if (client.AddrTemp.Contains("192.168."))
                    packet.WriteFixedAsciiString(RealmServerConfiguration.ExternalAddress, 20, Locale.Start);
                else
                    packet.WriteFixedAsciiString(RealmServerConfiguration.RealExternalAddress, 20, Locale.Start);
                packet.WriteUInt16(ServerApp<WCell.RealmServer.RealmServer>.Instance.Port);
                packet.WriteInt16((byte) map);
                packet.WriteInt16(x);
                packet.WriteInt16(y);
                packet.WriteByte(0);
                if (map == MapId.Testing)
                    packet.WriteByte(1);
                else
                    packet.WriteByte(0);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt32(-1);
                if (map == MapId.BatleField)
                    packet.WriteInt16(1);
                else
                    packet.WriteInt16(-1);
                client.Send(packet, true);
            }
        }

        public static void SendFightingModeChangedResponse(IRealmClient client, short rcvSessId, int rcvAccId,
            short victimSessId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FightingModeChanged))
            {
                packet.WriteInt16(rcvSessId);
                packet.WriteInt32(rcvAccId);
                packet.WriteByte(0);
                packet.WriteByte(victimSessId == (short) -1 ? 0 : 1);
                packet.WriteInt32(victimSessId);
                packet.WriteByte(1);
                client.Send(packet, true);
            }
        }

        public static void SendFightingModeChangedOnWarResponse(IRealmClient client, short rcvSessId, int rcvAccId,
            int factionId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FightingModeChanged))
            {
                packet.WriteInt16(rcvSessId);
                packet.WriteInt32(rcvAccId);
                packet.WriteByte(0);
                packet.WriteByte(2);
                packet.WriteInt32(factionId);
                packet.WriteByte(1);
                client.Send(packet, false);
            }
        }

        public static void SendSetClientTimeResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SetClientTime))
            {
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                int val1 = 0;
                int val2;
                if (hour < 6)
                {
                    val2 = hour * 10 + minute / 6;
                }
                else
                {
                    val1 = hour / 6;
                    val2 = (hour - val1 * 6) * 10 + minute / 6;
                }

                packet.WriteByte(val1);
                packet.WriteByte(val2);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.SaveBindLocation)]
        public static void SaveLocationRequest(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.BindLocation =
                (IWorldZoneLocation) new WCell.RealmServer.Entities.WorldZoneLocation(
                    (IWorldZoneLocation) client.ActiveCharacter);
        }
    }
}