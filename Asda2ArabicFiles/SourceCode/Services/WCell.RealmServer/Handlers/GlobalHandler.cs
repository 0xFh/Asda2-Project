using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
    internal class GlobalHandler
    {
        // Start : for the new GlobalMessege
        private static readonly byte[] unk81 = new byte[6] //for the New Global Messege
        {
            (byte) 250,
            (byte) 20,
            (byte) 124,
            (byte) 80,
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
            GlobalHandler.SendGlobalMessage(message, Color.Yellow);//لون شات العام Global Chat Color
        }

        // End : for the new GlobalMessege
        [PacketHandler(RealmServerOpCode.IDontKnowAboutCHaracter)]//5416
        public static void DontKnowAboutCHaracterRequest(IRealmClient client, RealmPacketIn packet)
        {
            var sessId = packet.ReadUInt16();//default : 1001Len : 2
            var chr = World.GetCharacterBySessionId(sessId);
            if (chr == null)
            {
                //client.ActiveCharacter.YouAreFuckingCheater("Requesting info abount not existing character",50);
                return;
            }
            SendCharacterVisibleNowResponse(client, chr);
        }

        public static void SendTransformToPetResponse(Character chr, bool isInto, IRealmClient reciver = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TransformToPet))//6637
            {
                packet.WriteByte(isInto ? 1 : 0);//{status}default value : 2 Len : 1
                packet.WriteInt32(chr.AccId);//{accId}default value : 361343 Len : 4
                packet.WriteInt16(chr.TransformationId);//{monstrId}default value : 807 Len : 2
                if (reciver != null)
                    reciver.Send(packet, addEnd: true);
                else
                {
                    chr.SendPacketToArea(packet, true, true);
                }
            }
        }
        public static void SendCharacterDeleteResponse(Character chr, IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterDelete))//4011
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 0
                packet.WriteInt32(chr.AccId);//{accId}default value : 0
                if (client == null)
                    chr.SendPacketToArea(packet, false, true);
                else
                    client.Send(packet, addEnd: true);
            }
        }

        public static void SendCharacterVisibleNowResponse(IRealmClient client, Character visibleChr)
        {
            if (visibleChr == null) return;
            if (visibleChr.Record == null)
                throw new ArgumentException("cant be null", "visibleChr.Record");
            if (client == null)
                throw new ArgumentException("cant be null", "client");
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

            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterVisibleNow)) //4005
            {
                packet.WriteInt16(visibleChr.SessionId); //{sessionID}default value : 31 Len : 2
                packet.WriteInt16((short)visibleChr.Asda2X); //{x}default value : 125 Len : 2
                packet.WriteInt16((short)visibleChr.Asda2Y); //{y}default value : 390 Len : 2
                packet.WriteByte(visibleChr.SettingsFlags[15]); //value name : unk7 default value : 1Len : 1
                packet.WriteByte(visibleChr.AvatarMask); //value name : unk8 default value : 72482Len : 4
                /*packet.WriteByte(174); //value name : unk8 default value : 72482Len : 4
                packet.WriteByte(135); //value name : unk8 default value : 72482Len : 4
                packet.WriteByte(0); //value name : unk8 default value : 72482Len : 4
                packet.WriteByte(0); //value name : unk7 default value : 1Len : 1*/
                packet.WriteInt32(Utility.Random(0, int.MaxValue));
                packet.WriteInt32(visibleChr.AccId); //{accId}default value : 347860 Len : 4
                packet.WriteFixedAsciiString(visibleChr.Name, 20); //{charName}default value :  Len : 20
                packet.WriteByte(visibleChr.CharNum); //{charNum}default value : 10 Len : 1
                packet.WriteByte(0); //value name : unk12 default value : 0Len : 1
                packet.WriteByte((byte)visibleChr.Gender); //{gender}default value : 1 Len : 1
                packet.WriteByte(visibleChr.ProfessionLevel); //{proffLevel}default value : 1 Len : 1
                packet.WriteByte((byte)visibleChr.Archetype.Class.Id); //{class}default value : 1 Len : 1
                packet.WriteByte(visibleChr.Level); //value name : level default value : 28Len : 1
                packet.WriteInt16(visibleChr.IsAsda2BattlegroundInProgress ? 2 : 0); //value name : unk16 default value : 1Len : 2
                packet.WriteInt16(visibleChr.CurrentBattleGroundId); //value name : unk17 default value : -1Len : 2
                packet.WriteByte(visibleChr.Asda2FactionId == -1 ? 0 : visibleChr.Asda2FactionId); //value name : unk18 default value : 0Len : 2
                packet.WriteByte(visibleChr.IsAsda2BattlegroundInProgress ? 1 : 0); //value name : unk18 default value : 0Len : 2
                packet.WriteByte(visibleChr.Record.Zodiac); //{zodiac}default value : 5 Len : 1
                packet.WriteByte(visibleChr.HairStyle); //{hair}default value : 4 Len : 1
                packet.WriteByte(visibleChr.HairColor); //{color}default value : 3 Len : 1
                packet.WriteByte(visibleChr.Record.Face); //{face}default value : 13 Len : 1
                packet.WriteByte(visibleChr.EyesColor); //{colorEyes}default value : 0 Len : 1
                packet.WriteInt16(visibleChr.SessionId); //{jumpId}default value : 31 Len : 1
                packet.WriteInt32(-1); //value name : unk17 default value : -1Len : 2
                packet.WriteInt16(visibleChr.IsDead ? 200 : visibleChr.IsSitting ? 108 : 0); //value name : unk17 default value : -1Len : 2
                packet.WriteFloat(visibleChr.Asda2X); //{x0}default value : 12550 Len : 4
                packet.WriteFloat(visibleChr.Asda2Y); //{y0}default value : 39050 Len : 4
                packet.WriteFloat(visibleChr.IsMoving ? visibleChr.LastNewPosition.X : 0);
                //{xEnd}default value : 0 Len : 4
                packet.WriteFloat(visibleChr.IsMoving ? visibleChr.LastNewPosition.Y : 0);
                //{yEnd}default value : 0 Len : 4
                packet.WriteFloat(visibleChr.IsMoving ? visibleChr.Orientation : 0);
                //{direction}default value : 0 Len : 4
                packet.WriteSkip(stub80); //{stub80}default value : stub80 Len : 16
                packet.WriteInt32(0); //value name : unk4 default value : 0Len : 4
                for (int i = 0; i < 20; i += 1)
                {
                    var item = visibleChr.Asda2Inventory.Equipment[i];
                    packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemId}default value : 0 Len : 4
                    packet.WriteByte(item == null ? 0 : item.Enchant); //{enchan}default value : 0 Len : 1
                }
                client.Send(packet, addEnd: true);
            }
            if (visibleChr.IsMoving)
            {
                if (client.ActiveCharacter.Map != null)
                    Asda2MovmentHandler.SendStartMoveCommonToOneClienResponset(visibleChr, client, false);
            }
        }
        public static void SendSpeedChangedResponse(IRealmClient client)
        {
            if (client.ActiveCharacter == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SpeedChanged))//6071
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 134 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accid}default value : 361343 Len : 4
                packet.WriteFloat(client.ActiveCharacter.RunSpeed);//{speed}default value : 0,592 Len : 4
                client.ActiveCharacter.SendPacketToArea(packet, true, true);
            }
        }


        private static readonly byte[] stub80 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };


        public static void SendCharacterPlaceInTitleRatingResponse(IRealmClient client, Character chr)
        {
            using (var packet = CreateCharacterPlaceInRaitingPacket(chr)) //6068
            {
                client.Send(packet, addEnd: true);
            }
        }
        public static void BroadcastCharacterPlaceInTitleRatingResponse(Character chr)
        {
            using (var packet = CreateCharacterPlaceInRaitingPacket(chr)) //6068
            {
                chr.SendPacketToArea(packet, true, true);
            }
        }
        public static RealmPacketOut CreateCharacterPlaceInRaitingPacket(Character chr)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.CharacterPlaceInTitleRating); //6068
            packet.WriteInt16(chr.SessionId); //{sessId}default value : 62 Len : 2
            packet.WriteInt32(chr.AccId); //{accId}default value : 353032 Len : 4
            packet.WriteInt32(chr.PlaceInRating); //{placeInRaiting}default value : 4455 Len : 4
            packet.WriteInt16(chr.Record.PreTitleId); //value name : unk4 default value : -1Len : 4
            packet.WriteInt16(chr.Record.PostTitleId); //value name : unk4 default value : -1Len : 4
            return packet;
        }
        public static void SendCharacterFriendShipResponse(IRealmClient client, Character chr)
        {
            if (chr.SoulmateRecord == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterFriendShip))//6152
            {
                packet.WriteInt32(chr.AccId);//{accId}default value : 342250 Len : 4
                packet.WriteInt32((int)chr.SoulmateRecord.SoulmateRelationGuid);//{friendGuid}default value : 16373 Len : 4
                packet.WriteInt32(chr.SoulmateRecord.FriendAccId(chr.AccId));//{friendAccId}default value : 321713 Len : 4
                packet.WriteInt16(0);//value name : unk2 default value : 0Len : 2
                client.Send(packet);
            }
        }

        public static void SendCharacterInfoPetResponse(IRealmClient client, Character chr)
        {
            using (var p = CreateCharacterPetInfoPacket(chr))
            {
                client.Send(p, addEnd: true);
            }
        }
        public static void UpdateCharacterPetInfoToArea(Character chr)
        {
            using (var p = CreateCharacterPetInfoPacket(chr))
            {
                chr.SendPacketToArea(p);
            }
        }
        public static RealmPacketOut CreateCharacterPetInfoPacket(Character chr)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.CharacterInfoPet); //6120
            packet.WriteInt32(chr.AccId); //{accId}default value : 0 Len : 4
            packet.WriteInt16(chr.Asda2Pet == null ? -1 : chr.Asda2Pet.Id); //{petId}default value : 4 Len : 2
            packet.WriteByte(chr.Asda2Pet == null ? 0 : chr.Asda2Pet.Level); //{petSize}default value : 1 Len : 1
            packet.WriteFixedAsciiString(chr.Asda2Pet == null ? "" : chr.Asda2Pet.Name, 16);//{petName}default value :  Len : 16
            return packet;
        }

        public static void SendCharacterInfoClanNameResponse(IRealmClient client, Character chr)
        {
            if (chr.Guild == null)
                return;
            using (var p = CreateCharacterInfoClanName(chr))
            {
                client.Send(p, addEnd: true);
            }
        }

        public static void SendCharactrerInfoClanNameToAllNearbyCharacters(Character chr)
        {
            if (chr.Guild == null)
                return;
            using (var p = CreateCharacterInfoClanName(chr))
            {
                chr.SendPacketToArea(p);
            }
        }
        public static RealmPacketOut CreateCharacterInfoClanName(Character chr)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.CharacterInfoClanName); //6153

            packet.WriteInt32(chr.AccId); //{accId}default value : 0 Len : 4
            packet.WriteInt16(chr.GuildId); //{clanId}default value : 0 Len : 2
            packet.WriteInt16(chr.Guild.Level);
            packet.WriteFixedAsciiString(chr.Guild.Name, 16); //{clanName}default value :  Len : 16
            packet.WriteByte(0);
            packet.WriteByte(3);
            packet.WriteByte(1);
            packet.Write(chr.Guild.ClanCrest);
            return packet;

        }

        public static void SendCharacterFactionAndFactionRankResponse(IRealmClient client, Character chr)
        {
            using (var p = CreateCharacterFactionPacket(chr))
            {
                client.Send(p);
            }
        }
        public static void SendCharacterFactionToNearbyCharacters(Character chr)
        {
            using (var p = CreateCharacterFactionPacket(chr))
            {
                chr.SendPacketToArea(p, true, false);
            }
        }
        public static RealmPacketOut CreateCharacterFactionPacket(Character chr)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.CharacterFactionAndFactionRank); //6722

            packet.WriteInt32(chr.AccId); //{accId}default value : 0 Len : 4
            packet.WriteInt16(chr.SessionId); //{sessId}default value : 0 Len : 2
            packet.WriteInt16(chr.Asda2FactionId); //{factionId}default value : 0 Len : 2
            packet.WriteByte(chr.Asda2FactionRank); //{factionRank}default value : 1 Len : 1
            return packet;

        }

        public static void SendNpcVisiableNowResponse(IRealmClient client, GameObject npc)
        {
            if (npc.EntityId.Entry == 145)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.NpcVisiableNow)) //4067
            {
                {
                    packet.WriteInt16(npc.UniqIdOnMap); //{npcNum}default value : 1 Len : 2

                    packet.WriteInt16(npc.EntryId); //{npcId}default value : 3 Len : 2
                    packet.WriteInt32(npc.UniqWorldEntityId);
                    //{uniqIdOnMap}default value : 239 Len : 4
                    packet.WriteInt16((short)npc.Asda2X); //{x}default value : 170 Len : 2
                    packet.WriteInt16((short)npc.Asda2Y); //{y}default value : 396 Len : 2
                    packet.WriteSkip(stub12); //{stub12}default value : stub12 Len : 96
                }
                client.Send(packet, addEnd: false);
            }
        }

        private static readonly byte[] stub12 = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            };

        public static void SendMonstrVisibleNowResponse(IRealmClient client, NPC visibleNpc)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MonstVisible)) //4013
            {
                packet.WriteInt16(visibleNpc.UniqIdOnMap); //{monstrMapId}default value : 150 Len : 2
                packet.WriteInt16((ushort)visibleNpc.Entry.NPCId); //{monstrId}default value : 4 Len : 2
                packet.WriteInt32(91); //value name : unk6 default value : 91Len : 4
                packet.WriteByte(2); //value name : unk7 default value : 2Len : 1
                packet.WriteInt32(visibleNpc.Health); //{hp}default value : 93 Len : 2
                var speed = visibleNpc.Movement.MoveType == Constants.NPCs.AIMoveType.Walk ? visibleNpc.WalkSpeed : visibleNpc.RunSpeed;
                packet.WriteInt16((short)(1000f / speed)); //{movingSpeed}default value : 1666 Len : 2
                packet.WriteInt16(15000); //{animationSpeed}default value : 10000 Len : 2
                packet.WriteSkip(Stab23); //value name : stab23 default value : stab23Len : 2
                packet.WriteInt16((short)visibleNpc.Asda2X); //{xStart}default value : 48 Len : 2
                packet.WriteInt16((short)visibleNpc.Asda2Y); //{yStart}default value : 150 Len : 2
                if (visibleNpc.IsMoving)
                {
                    packet.WriteInt16((short)visibleNpc.Movement.Destination.X - ((int)visibleNpc.MapId * 1000)); //{xStop}default value : 50 Len : 2
                    packet.WriteInt16((short)visibleNpc.Movement.Destination.Y - ((int)visibleNpc.MapId * 1000)); //{yStop}default value : 150 Len : 2
                }
                else
                {
                    packet.WriteInt16((short)visibleNpc.Asda2X); //{xStop}default value : 50 Len : 2
                    packet.WriteInt16((short)visibleNpc.Asda2Y); //{yStop}default value : 150 Len : 2
                }
                packet.WriteSkip(Stab33); //value name : stab33 default value : stab33Len : 120
                packet.WriteInt32(91); //value name : unk16 default value : 91Len : 4
                packet.WriteInt16(47); //value name : unk19 default value : 47Len : 2
                packet.WriteInt16(150); //value name : unk20 default value : 150Len : 2
                packet.WriteInt32(0); //value name : unk21 default value : 0Len : 4
                packet.WriteInt16(1500); //value name : unk160 default value : 1500Len : 2
                packet.WriteInt16(1000); //value name : unk17 default value : 1000Len : 2
                packet.WriteInt16(1000); //value name : unk18 default value : 1000Len : 2
                client.Send(packet, addEnd: false);
            }
        }
        public static void SendBuffsOnCharacterInfoResponse(IRealmClient rcv, Character chr)
        {
            if (chr.Auras == null || chr.Auras.ActiveAuras.Length == 0)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.BuffsOnCharacterInfo))//6154
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 45 Len : 2
                var buffs = chr.Auras.ActiveAuras;
                var cnt = buffs.Length;
                for (int i = 0; i < cnt; i++)
                {
                    var aura = buffs[i];
                    packet.WriteInt16(0);
                    packet.WriteInt32(aura.Spell.RealId);//{buffId}default value : 202 Len : 4
                    packet.WriteInt32(aura.Duration / 1000);
                    packet.WriteInt16(0);
                }
                if (chr.IsOnTransport)
                {
                    packet.WriteInt16(2);
                    packet.WriteInt32(chr.TransportItemId);//{buffId}default value : 202 Len : 4
                    packet.WriteInt32(-1);
                    packet.WriteInt16(60);
                }
                rcv.Send(packet, addEnd: false);
            }
        }

        private static readonly byte[] Stab23 = new byte[] { 0x00, 0x00 };

        private static readonly byte[] Stab33 = new byte[]
            {
                0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

        public static void SendItemVisible(Character character, Asda2Loot loot)
        {
            foreach (var item in loot.Items)
            {
                if (item.Taken)
                    continue;
                using (var packet = new RealmPacketOut(RealmServerOpCode.ItemDroped)) //5005
                {
                    packet.WriteInt32(item.Template.Id); //{itemId}default value : 0
                    packet.WriteInt16(item.Position.X); //{x}default value : 0
                    packet.WriteInt16(item.Position.Y); //{y}default value : 0
                    packet.WriteInt32(character.SessionId); //{sessId}default value : 0
                    packet.WriteByte(10); //value name : _
                    character.Client.Send(packet, addEnd: false);
                }
            }
        }

        public static void SendRemoveItemResponse(Asda2LootItem item)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.RemoveItemFromWorld)) //5015
            {
                packet.WriteInt16(item.Position.X); //{x}default value : 0 Len : 2
                packet.WriteInt16(item.Position.Y); //{y}default value : 0 Len : 2
                item.Loot.SendPacketToArea(packet, false, true);
            }
        }

        public static void SendRemoveItemOnGuildWaveResponse(Asda2LootItem item, int guildid)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.RemoveItemFromWorld)) //5015
            {
                packet.WriteInt16(item.Position.X); //{x}default value : 0 Len : 2
                packet.WriteInt16(item.Position.Y); //{y}default value : 0 Len : 2
                //packet.WriteInt32((int)item.Template.ItemId);
                Asda2GuildWave.Asda2GuildWave guildwave = Asda2GuildWave.Asda2GuildWaveMgr.GetGuildWaveForId(guildid);
                if (guildwave != null)
                    guildwave.SendPacketToRegisteredOnGuildWavePlayers(packet);
            }
        }

        public static void SendRemoveItemResponse(IRealmClient receiver, short x, short y)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.RemoveItemFromWorld)) //5015
            {
                packet.WriteInt16(x); //{x}default value : 0 Len : 2
                packet.WriteInt16(y); //{y}default value : 0 Len : 2
                receiver.Send(packet, addEnd: true);
            }
        }

        public static void SendRemoveLootResponse(Character chr, Asda2Loot loot)
        {
            foreach (var item in loot.Items)
            {
                if (item.Taken)
                    continue;
                using (var packet = new RealmPacketOut(RealmServerOpCode.RemoveItemFromWorld)) //5015
                {
                    packet.WriteInt16(item.Position.X); //{x}default value : 0 Len : 2
                    packet.WriteInt16(item.Position.Y); //{y}default value : 0 Len : 2
                    loot.SendPacketToArea(packet, false, true);
                }
            }
        }

        #region who is here
        [PacketHandler(RealmServerOpCode.WhoIsHere)] //5480
        public static void WhoIsHereRequest(IRealmClient client, RealmPacketIn packet)
        {
            SendWhoIsHereListResponse(client);
            Asda2TitleChecker.OnWheIsHere(client.ActiveCharacter);
        }
        public static void SendWhoIsHereListResponse(IRealmClient client)
        {
            var characters =
                client.ActiveCharacter.Map.Characters.Where(
                    c => c.Level >= client.ActiveCharacter.Level - 9 && c.Level <= client.ActiveCharacter.Level + 9).
                    ToList();
            characters.Remove(client.ActiveCharacter);
            var lists = new List<List<Character>>();
            var cnt = characters.Count / 8;
            if (cnt == 0 && characters.Count != 0)
                cnt = 1;
            for (int i = 0; i < cnt; i++)
            {
                lists.Add(new List<Character>(characters.Skip(i * 8).Take(8)));


            }
            foreach (var list in lists)
            {

                using (var packet = new RealmPacketOut(RealmServerOpCode.WhoIsHereList)) //5481
                {
                    for (int j = 0; j < list.Count; j += 1)
                    {
                        var chr = list[j];
                        packet.WriteByte(chr.Level); //{level}default value : 41 Len : 1
                        packet.WriteByte(chr.ProfessionLevel); //{proffLevel}default value : 13 Len : 1
                        packet.WriteByte((byte)chr.Archetype.ClassId); //{class}default value : 5 Len : 1
                        packet.WriteFixedAsciiString(chr.Name, 20);
                        //{name}default value :  Len : 20
                        packet.WriteInt32(-1); //value name : unk9 default value : -1Len : 4
                        packet.WriteByte(0); //value name : unk10 default value : 0Len : 1
                        packet.WriteInt16(chr.SessionId); //{sessId}default value : 58 Len : 2
                        packet.WriteInt32(chr.AccId); //{accId}default value : 325276 Len : 4
                        packet.WriteByte(chr.CharNum); //{charNum}default value : 11 Len : 1
                    }
                    client.Send(packet);
                }
            }
            SendWhoIsHereListEndedResponse(client);
        }

        public static void SendWhoIsHereListEndedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WhoIsHereListEnded)) //5482
            {
                client.Send(packet, addEnd: true);
            }
        }

        #endregion

        #region friends
        [PacketHandler(RealmServerOpCode.AddFriend)]//4150
        public static void AddFriendRequest(IRealmClient client, RealmPacketIn packet)
        {
            var targetSessId = packet.ReadUInt16();//default : 2Len : 2
            var targetChr = World.GetCharacterBySessionId(targetSessId);
            if (targetChr == null)
            {
                client.ActiveCharacter.SendInfoMsg("Target character not found.");
                SendFriendAddedResponse(client, false, null);
                return;
            }
            targetChr.CurrentFriendInviter = client.ActiveCharacter;
            SendInviteFromeSomeoneFriendResponse(targetChr, client.ActiveCharacter);
        }
        public static void SendFriendAddedResponse(IRealmClient client, bool success, Character friend)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FriendAdded)) //4151
            {
                packet.WriteByte(success); //{status}default value : 1 Len : 1
                packet.WriteInt32(friend == null ? 0 : friend.AccId); //{friendAccId}default value : 370724 Len : 4
                packet.WriteByte(friend == null ? 0 : friend.CharNum); //{friendCharNum}default value : 10 Len : 1
                packet.WriteByte(0); //{chanel}default value : 2 Len : 1
                packet.WriteByte((byte)(friend == null ? 0 : friend.MapId)); //{locId}default value : 3 Len : 1
                packet.WriteByte(friend == null ? 0 : friend.Level); //{level}default value : 18 Len : 1
                packet.WriteByte(friend == null ? 0 : friend.ProfessionLevel); //{profLevel}default value : 23 Len : 1
                packet.WriteByte((byte)(friend == null ? 0 : friend.Archetype.ClassId)); //{class}default value : 7 Len : 1
                packet.WriteByte(1); //{isOnline}default value : 1 Len : 1
                packet.WriteFixedAsciiString(friend == null ? "" : friend.Name, 20); //{name}default value :  Len : 20
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendInviteFromeSomeoneFriendResponse(Character invitee, Character inviter)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.InviteFromeSomeoneFriend))//4152
            {
                packet.WriteInt32(-1);//value name : unk5 default value : -1Len : 4
                packet.WriteFixedAsciiString(inviter.Name, 20);//{inviterName}default value :  Len : 20
                packet.WriteInt16(inviter.SessionId);//{inviterSessId}default value : 143 Len : 2
                invitee.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.InviteFriendAnswer)]//4153
        public static void InviteFriendAnswerRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 16;//default : Len : 20
            var inviteSessId = packet.ReadUInt16();//default : 143Len : 2
            var isOk = packet.ReadByte() == 1;//default : 1Len : 1
            if (!isOk)
            {
                if (client.ActiveCharacter.CurrentFriendInviter != null)
                    client.ActiveCharacter.CurrentFriendInviter.SendInfoMsg("Failed to make friend with " + client.ActiveCharacter.Name);
            }
            else
            {
                var inviter = World.GetCharacterBySessionId(inviteSessId);
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
                        client.ActiveCharacter.CurrentFriendInviter = null;
                        return;
                    }
                    if (inviter != client.ActiveCharacter.CurrentFriendInviter)
                    {
                        if (client.ActiveCharacter.CurrentFriendInviter != null)
                            client.ActiveCharacter.CurrentFriendInviter.SendInfoMsg("Failed to make friend with " + client.ActiveCharacter.Name);
                    }
                    else
                    {
                        RealmServer.IOQueue.AddMessage(() =>
                        {

                            var newRec = new Asda2FriendshipRecord(client.ActiveCharacter, inviter);
                            newRec.CreateLater();
                            lock (client.ActiveCharacter.Friends)
                            {
                                client.ActiveCharacter.Friends.Add(inviter.AccId, inviter.Record);
                                client.ActiveCharacter.FriendRecords.Add(newRec);
                            }
                            lock (inviter.Friends)
                            {
                                inviter.Friends.Add(client.ActiveCharacter.AccId, client.ActiveCharacter.Record);
                                inviter.FriendRecords.Add(newRec);
                            }

                            Asda2TitleChecker.OnNewFriend(inviter);
                            Asda2TitleChecker.OnNewFriend(client.ActiveCharacter);

                        });
                        SendFriendAddedResponse(inviter.Client, true, client.ActiveCharacter);
                        SendFriendAddedResponse(client, true, inviter);
                    }
                }
            }
            client.ActiveCharacter.CurrentFriendInviter = null;
        }
        [PacketHandler(RealmServerOpCode.ShowFriendList)]//4156
        public static void ShowFriendListRequest(IRealmClient client, RealmPacketIn packet)
        {
            SendFriendListResponse(client);
        }


        public static void SendFriendListResponse(IRealmClient client)
        {
            lock (client.ActiveCharacter.Friends)
            {
                foreach (var chr in client.ActiveCharacter.Friends.Values)
                {
                    var character = World.GetCharacter(chr.EntityLowId);
                    using (var packet = new RealmPacketOut(RealmServerOpCode.FriendList)) //4157
                    {
                        packet.WriteUInt32(chr.AccountId); //{friendAccId}default value : 370724 Len : 4
                        packet.WriteByte(chr.CharNum); //{friendCharNum}default value : 10 Len : 1
                        packet.WriteByte(1); //{chanel}default value : 2 Len : 1
                        packet.WriteByte((byte)(character == null ? chr.MapId : character.MapId)); //{locId}default value : 3 Len : 1
                        packet.WriteByte(character == null ? chr.Level : character.Level); //{level}default value : 18 Len : 1
                        packet.WriteByte(character == null ? chr.ProfessionLevel : character.ProfessionLevel); //{profLevel}default value : 23 Len : 1
                        packet.WriteByte((byte)(character == null ? chr.Class : character.Archetype.ClassId)); //{class}default value : 7 Len : 1
                        packet.WriteByte(character == null ? 0 : 1); //{isOnline}default value : 1 Len : 1
                        packet.WriteFixedAsciiString(character == null ? chr.Name : character.Name, 20); //{name}default value :  Len : 20
                        client.Send(packet);
                    }
                }
            }
            SendFriendListEndedResponse(client);
        }
        public static void SendFriendListEndedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FriendListEnded)) //4158
            {
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.DeleteFromFriendList)]//4154
        public static void DeleteFromFriendListRequest(IRealmClient client, RealmPacketIn packet)
        {
            var targetAccId = packet.ReadUInt32();//default : 370455Len : 4
            var targetCharNum = packet.ReadByte();//default : 12Len : 1
            if (client.ActiveCharacter.Friends.ContainsKey(targetAccId))
            {
                lock (client.ActiveCharacter.Friends)
                {
                    var chr = World.GetCharacterByAccId(targetAccId);
                    if (chr != null)
                    {
                        if (chr.Friends.ContainsKey(client.ActiveCharacter.AccId))
                        {
                            chr.Friends.Remove(client.ActiveCharacter.AccId);
                            var rec01 = chr.FriendRecords.FirstOrDefault(f => f.FirstCharacterAccId == client.ActiveCharacter.EntityId.Low &&
                                                                    f.SecondCharacterAccId == targetAccId);
                            var rec02 = chr.FriendRecords.FirstOrDefault(f => f.FirstCharacterAccId == targetAccId &&
                                                                            f.SecondCharacterAccId == client.ActiveCharacter.EntityId.Low);
                            if (rec01 != null)
                            {
                                chr.FriendRecords.Remove(rec01);
                            }
                            if (rec02 != null)
                            {
                                chr.FriendRecords.Remove(rec02);
                            }
                        }
                        chr.SendInfoMsg(string.Format("{0} is no longer your friend.", client.ActiveCharacter.Name));
                    }
                    client.ActiveCharacter.Friends.Remove(targetAccId);
                    var rec1 = client.ActiveCharacter.FriendRecords.FirstOrDefault(f => f.FirstCharacterAccId == client.ActiveCharacter.EntityId.Low &&
                                                                    f.SecondCharacterAccId == targetAccId);
                    var rec2 = client.ActiveCharacter.FriendRecords.FirstOrDefault(f => f.FirstCharacterAccId == targetAccId &&
                                                                    f.SecondCharacterAccId == client.ActiveCharacter.EntityId.Low);
                    if (rec1 != null)
                    {
                        client.ActiveCharacter.FriendRecords.Remove(rec1);
                        rec1.DeleteLater();
                    }
                    if (rec2 != null)
                    {
                        client.ActiveCharacter.FriendRecords.Remove(rec2);
                        rec2.DeleteLater();
                    }
                }
            }
            SendDeletedFromFriendListResponse(client, targetAccId, targetCharNum);
        }

        public static void SendDeletedFromFriendListResponse(IRealmClient client, uint accId, byte charNum)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.DeletedFromFriendList))//4155
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt32(accId);//{accId}default value : 370455 Len : 4
                packet.WriteByte(charNum);//{charNum}default value : 12 Len : 1
                client.Send(packet, addEnd: true);
            }
        }

        #endregion
        #region teleporting
        [PacketHandler(RealmServerOpCode.TeleportByCristal)] //5477
        public static void TeleportByCristalRequest(IRealmClient client, RealmPacketIn packet)
        {
            var placeNum = packet.ReadByte(); //default : 1Len : 4
            if (!Asda2TeleportMgr.Teleports.ContainsKey(placeNum))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to use teleport crystal to unknown location.", 10);
                return;
            }
            var templ = Asda2TeleportMgr.Teleports[placeNum];
            if (!client.ActiveCharacter.SubtractMoney((uint)templ.Price))
            {
                client.ActiveCharacter.SendInfoMsg("Not enoght money to teleport.");
                return;
            }
            client.ActiveCharacter.SendMoneyUpdate();
            client.ActiveCharacter.TeleportTo(templ.ToMap, templ.To);
            Asda2TitleChecker.OnTeleportedByCrystal(client.ActiveCharacter);

        }

        public static void SendTeleportedByCristalResponse(IRealmClient client, MapId map, short x, short y, TeleportByCristalStaus staus = TeleportByCristalStaus.Ok)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TeleportedByCristal)) //5055
            {
                packet.WriteByte((byte)staus); //value name : unk4 default value : 1Len : 1
                if (client.AddrTemp.Contains("192.168."))
                    packet.WriteFixedAsciiString(RealmServerConfiguration.ExternalAddress, 20);
                else
                    packet.WriteFixedAsciiString(RealmServerConfiguration.RealExternalAddress, 20);
                //{ipAddr}default value :  Len : 20
                packet.WriteUInt16(RealmServer.Instance.Port); //{port}default value : 15602 Len : 2
                packet.WriteInt16((byte)map); //{locNum}default value : 1 Len : 2
                packet.WriteInt16(x); //{x}default value : 295 Len : 2
                packet.WriteInt16(y); //{y}default value : 235 Len : 2
                packet.WriteByte(0);
                if (map == MapId.Guildwave)
                    packet.WriteByte(1);
                else
                    packet.WriteByte(0);
                packet.WriteInt64(-1); //value name : unk10 default value : -1Len : 8
                packet.WriteInt64(-1); //value name : unk11 default value : -1Len : 8
                packet.WriteInt32(-1); //value name : unk12 default value : -1Len : 4
                if (map == MapId.BatleField)
                {
                    packet.WriteInt16(1);
                }
                else
                {
                    packet.WriteInt16(-1);
                }
                client.Send(packet, addEnd: true);
            }
        }

        #endregion


        #region Fighting

        public static void SendFightingModeChangedResponse(IRealmClient client, short rcvSessId, int rcvAccId,
                                                           short victimSessId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FightingModeChanged)) //4206
            {
                packet.WriteInt16(rcvSessId); //{rcvSessId}default value : 79 Len : 2
                packet.WriteInt32(rcvAccId); //{rcvAccId}default value : 354889 Len : 4
                packet.WriteByte(0); //value name : unk6 default value : 0Len : 1
                packet.WriteByte(victimSessId == -1 ? 0 : 1); //value name : unk7 default value : 0Len : 1
                packet.WriteInt32(victimSessId); //{victimSessId}default value : -1 Len : 2
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendFightingModeChangedOnWarResponse(IRealmClient client, short rcvSessId, int rcvAccId, int factionId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FightingModeChanged)) //4206
            {
                packet.WriteInt16(rcvSessId); //{rcvSessId}default value : 79 Len : 2
                packet.WriteInt32(rcvAccId); //{rcvAccId}default value : 354889 Len : 4
                packet.WriteByte(0); //value name : unk6 default value : 0Len : 1
                packet.WriteByte(4); //value name : unk7 default value : 0Len : 1
                packet.WriteInt32(factionId); //{victimSessId}default value : -1 Len : 2
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                client.Send(packet);
            }
        }

        #endregion

        #region weather/time
        public static void SendSetClientTimeResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SetClientTime))//6759
            {
                var hours = DateTime.Now.Hour;
                var mins = DateTime.Now.Minute;
                var minsToClient = 0;
                var hoursToClient = 0;
                if (hours < 6)
                {
                    minsToClient = hours * 10 + mins / 6;
                }
                else
                {
                    hoursToClient = hours / 6;
                    minsToClient = (hours - hoursToClient * 6) * 10 + mins / 6;
                }
                packet.WriteByte(hoursToClient);//{plus6Hours}default value : 2 Len : 1
                packet.WriteByte(minsToClient);//{plus6Mins}default value : 59 Len : 1
                client.Send(packet, addEnd: false);
            }
        }

        #endregion
        [PacketHandler(RealmServerOpCode.SaveBindLocation)]//5280
        public static void SaveLocationRequest(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.BindLocation = new Entities.WorldZoneLocation(client.ActiveCharacter);

        }


    }

    public static class Asda2TeleportMgr
    {
        public static Dictionary<int, Asda2TeleportCristalVector> Teleports = new Dictionary<int, Asda2TeleportCristalVector>();
        [Initialization(InitializationPass.Last, "Teleport manager.")]
        public static void Init()
        {
            Teleports.Add(1, new Asda2TeleportCristalVector { To = new Vector3(1295, 1235), Price = 0, ToMap = MapId.RainRiver });
            Teleports.Add(3, new Asda2TeleportCristalVector { To = new Vector3(3117, 3389), Price = 0, ToMap = MapId.Alpia });
            Teleports.Add(0, new Asda2TeleportCristalVector { To = new Vector3(393, 397), Price = 3000, ToMap = MapId.Silaris });
            Teleports.Add(7, new Asda2TeleportCristalVector { To = new Vector3(7135, 7188), Price = 10000, ToMap = MapId.Flamio });
            Teleports.Add(5, new Asda2TeleportCristalVector { To = new Vector3(5394, 5342), Price = 15000, ToMap = MapId.Aquaton });
            Teleports.Add(25, new Asda2TeleportCristalVector { To = new Vector3(25274, 25326), Price = 5000, ToMap = MapId.DesolatedMarsh });
            Teleports.Add(23, new Asda2TeleportCristalVector { To = new Vector3(23253, 23304), Price = 15000, ToMap = MapId.IceQuarry });
            Teleports.Add(2, new Asda2TeleportCristalVector { To = new Vector3(2303, 2309), Price = 1500, ToMap = MapId.ConquestLand });
            Teleports.Add(6, new Asda2TeleportCristalVector { To = new Vector3(6365, 6110), Price = 1500, ToMap = MapId.SunnyCoast });
            Teleports.Add(13, new Asda2TeleportCristalVector { To = new Vector3(13208, 13388), Price = 5000, ToMap = MapId.Flabis });
            Teleports.Add(24, new Asda2TeleportCristalVector { To = new Vector3(24318, 24310), Price = 5000, ToMap = MapId.BurnedoutForest });
            Teleports.Add(29, new Asda2TeleportCristalVector { To = new Vector3(29122, 29425), Price = 15000, ToMap = MapId.Fantagle });
        }
    }

    public static class Asda2TeleportHandler
    {
        [PacketHandler(RealmServerOpCode.SaveLocation)] //6220
        public static void SaveLocationRequest(IRealmClient client, RealmPacketIn packet)
        {
            var name = packet.ReadAsdaString(32, Locale.En); //default : Len : 32
            packet.Position += 1; //nk9 default : 0Len : 1
            var mapId = packet.ReadByte(); //default : 5Len : 1
            var pointNum = packet.ReadUInt16(); //default : 8Len : 2
            var x = packet.ReadInt16(); //default : 397Len : 2
            var y = packet.ReadInt16(); //default : 345Len : 2

            if (pointNum > 9)
            {
                client.ActiveCharacter.YouAreFuckingCheater(
                    "Trying to save teleportation point with id more than 9.", 50);
                SendLocationSavedResponse(client, LocationSavedStatus.Fail, null, 0);
                return;
            }
            if (client.ActiveCharacter.TeleportPoints[pointNum] == null)
            {
                RealmServer.IOQueue.AddMessage(() =>
                {

                    var newRec = Asda2TeleportingPointRecord.CreateRecord(client.ActiveCharacter.EntityId.Low,
                                                                          (short)client.ActiveCharacter.Position.X,
                                                                          (short)client.ActiveCharacter.Position.Y,
                                                                          client.ActiveCharacter.MapId);
                    newRec.CreateLater();
                    client.ActiveCharacter.TeleportPoints[pointNum] = newRec;
                    newRec.X = (short)client.ActiveCharacter.Position.X;
                    newRec.Y = (short)client.ActiveCharacter.Position.Y;
                    newRec.MapId = client.ActiveCharacter.MapId;
                    newRec.Name = name;
                    SendLocationSavedResponse(client, LocationSavedStatus.Ok, newRec, (short)pointNum);

                });
                return;
            }
            var point = client.ActiveCharacter.TeleportPoints[pointNum];
            point.X = (short)client.ActiveCharacter.Position.X;
            point.Y = (short)client.ActiveCharacter.Position.Y;
            point.MapId = client.ActiveCharacter.MapId;
            point.Name = name;
            point.SaveLater();
            SendLocationSavedResponse(client, LocationSavedStatus.Ok, point, (short)pointNum);
        }

        public static void SendLocationSavedResponse(IRealmClient client, LocationSavedStatus status,
                                                     Asda2TeleportingPointRecord rec, short pointNum)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.LocationSaved)) //6221
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteFixedAsciiString(rec == null ? "" : rec.Name, 32);
                //{name}default value :  Len : 32
                packet.WriteByte(0); //value name : unk7 default value : 0Len : 1
                var mapId = (byte)(rec == null ? 0 : rec.MapId);
                packet.WriteByte(mapId); //{mapId}default value : 5 Len : 1
                packet.WriteInt16(pointNum); //{pointNum}default value : 8 Len : 2
                packet.WriteInt16(rec == null ? 0 : rec.X - 1000 * mapId); //{x}default value : 397 Len : 2
                packet.WriteInt16(rec == null ? 0 : rec.Y - 1000 * mapId); //{y}default value : 345 Len : 2
                client.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.DeleteSavedLocation)] //6222
        public static void DeleteSavedLocationRequest(IRealmClient client, RealmPacketIn packet)
        {
            var pointNum = packet.ReadByte(); //default : 4Len : 1
            if (client.ActiveCharacter.TeleportPoints[pointNum] == null)
            {
                client.ActiveCharacter.SendInfoMsg("Can't delete, point not founded.");
                SendSavedLocationDeletedResponse(client, LocationSavedStatus.Fail);
                return;
            }
            var point = client.ActiveCharacter.TeleportPoints[pointNum];
            client.ActiveCharacter.TeleportPoints[pointNum] = null;
            point.DeleteLater();
            SendSavedLocationDeletedResponse(client, LocationSavedStatus.Ok, pointNum);
        }

        public static void SendSavedLocationDeletedResponse(IRealmClient client, LocationSavedStatus status,
                                                            short pointId = -1)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SavedLocationDeleted)) //6223
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt16(pointId); //{pointId}default value : 4 Len : 2
                client.Send(packet);
            }
        }

        public static void SendSavedLocationsInitResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SavedLocationsInit)) //6225
            {
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                for (int i = 0; i < client.ActiveCharacter.TeleportPoints.Length; i += 1)
                {
                    var point = client.ActiveCharacter.TeleportPoints[i];
                    packet.WriteFixedAsciiString(point == null ? "" : point.Name, 32);
                    //{name}default value :  Len : 32
                    packet.WriteByte(0); //value name : unk7 default value : 0Len : 1
                    var mapId = (byte)(point == null ? 0 : point.MapId);
                    packet.WriteByte(mapId); //{mapId}default value : 5 Len : 1
                    packet.WriteInt16(point == null ? -1 : i); //{pointNum}default value : 0 Len : 2
                    packet.WriteInt16(point == null ? 0 : point.X - 1000 * mapId); //{x}default value : 397 Len : 2
                    packet.WriteInt16(point == null ? 0 : point.Y - 1000 * mapId); //{y}default value : 345 Len : 2
                }
                client.Send(packet, addEnd: true);
            }
        }

        #region item displaying

        [PacketHandler(RealmServerOpCode.DisplayItem)] //6129
        public static void DisplayItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var toHowSessId = packet.ReadUInt16(); //default : -1Len : 2
            var invType = packet.ReadByte(); //default : 1Len : 1
            var slot = packet.ReadInt16(); //default : 26Len : 2

            var target = World.GetCharacterBySessionId(toHowSessId);
            var item = invType == 1
                           ? client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slot)
                           : client.ActiveCharacter.Asda2Inventory.GetRegularItem(slot);
            if (item == null)
            {
                client.ActiveCharacter.SendInfoMsg("Item not founded.");
                return;
            }
            SendItemDisplayedResponse(client.ActiveCharacter, item, target);
        }

        public static void SendItemDisplayedResponse(Character displayer, Asda2Item item, Character reciever)
        {
            if (reciever == null)
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.ItemDisplayed)) //6130
                {
                    packet.WriteInt32(displayer.AccId); //{displayerAccId}default value : 361343 Len : 4
                    packet.WriteFixedAsciiString(displayer.Name, 20);
                    //{displayerName}default value :  Len : 20
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item);
                    displayer.SendPacketToArea(packet);
                }
            }
            else
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.ItemDisplayed)) //6130
                {
                    packet.WriteInt32(displayer.AccId); //{displayerAccId}default value : 361343 Len : 4
                    packet.WriteFixedAsciiString(displayer.Name, 20);
                    //{displayerName}default value :  Len : 20
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item);
                    reciever.Send(packet);
                }
            }
        }

        #endregion

        #region get character info

        [PacketHandler(RealmServerOpCode.GetCharecterInfo)] //5478
        public static void GetCharecterInfoRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            var targetSessId = packet.ReadUInt16(); //default : 53Len : 2
            var target = World.GetCharacterBySessionId(targetSessId);
            if (target == null)
            {
                client.ActiveCharacter.SendInfoMsg("Character not founded.");
                return;
            }
            SendCharacterFullInfoResponse(client, target);
            SendCharacterRegularEquipmentInfoResponse(client, target);
            Asda2TitleChecker.OnGetCharacterInfo(client.ActiveCharacter);
        }

        public static void SendCharacterFullInfoResponse(IRealmClient client, Character target)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterFullInfo)) //5479
            {
                packet.WriteByte(target.Level); //{level}default value : 70 Len : 1
                packet.WriteByte(target.ProfessionLevel); //{proffLineId}default value : 26 Len : 1
                packet.WriteByte((byte)target.Archetype.ClassId)
                ; //{class}default value : 7 Len : 1
                packet.WriteFixedAsciiString(target.Guild == null ? "" : target.Guild.Name, 17); //{clanName}default value :  Len : 17
                packet.WriteSkip(unk9); //value name : unk9 default value : unk9Len : 96
                packet.WriteInt32(target.AccId); //{accId}default value : 64842 Len : 4
                packet.WriteByte(3); //value name : unk1 default value : 3Len : 1
                for (int i = 0; i < 9; i += 1)
                {
                    var item = target.Asda2Inventory.Equipment[i + 11];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item);
                }
                client.Send(packet, addEnd: true);
            }
        }

        private static readonly byte[] unk9 = new byte[]
                                                  {
                                                      0xC7, 0x4E, 0x00, 0x00, 0xD3, 0x4E, 0x00, 0x00, 0xD4, 0x4E, 0x00,
                                                      0x00, 0x86, 0x4E, 0x00, 0x00, 0x87, 0x4E, 0x00, 0x00, 0xBE, 0x4E,
                                                      0x00, 0x00, 0xBD, 0x4E, 0x00, 0x00, 0xAC, 0x4E, 0x00, 0x00, 0xBF,
                                                      0x4E, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                      0x05, 0x0A, 0x00, 0x00, 0xC7, 0x4E, 0xD3, 0x4E, 0xD4, 0x4E, 0x86,
                                                      0x4E, 0x87, 0x4E, 0xBE, 0x4E, 0xBD, 0x4E, 0xAC, 0x4E, 0xBF, 0x4E,
                                                      0xFF, 0xFF, 0xFF, 0xFF, 0x36, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                      0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                      0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00
                                                  };


        public static void SendCharacterRegularEquipmentInfoResponse(IRealmClient client, Character target)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterRegularEquipmentInfo)) //7006
            {
                packet.WriteInt32(target.AccId); //{accId}default value : 64842 Len : 4
                for (int i = 0; i < 11; i += 1)
                {
                    var item = target.Asda2Inventory.Equipment[i];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item);
                }
                client.Send(packet, addEnd: true);
            }
        }

        #endregion
    }
    [ActiveRecord("Asda2FriendshipRecord", Access = PropertyAccess.Property)]
    public class Asda2FriendshipRecord : WCellRecord<Asda2FriendshipRecord>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(Asda2FriendshipRecord), "Guid");

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid
        {
            get;
            set;
        }
        [Property]
        public uint FirstCharacterAccId { get; set; }
        [Property]
        public uint SecondCharacterAccId { get; set; }

        public static List<Asda2FriendshipRecord> LoadAll(uint characterId)
        {
            var first = FindAllByProperty("FirstCharacterAccId", characterId);
            var second = FindAllByProperty("SecondCharacterAccId", characterId);
            var r = new List<Asda2FriendshipRecord>();
            r.AddRange(first);
            r.AddRange(second);
            return r;
        }

        public Asda2FriendshipRecord() { }
        public Asda2FriendshipRecord(Character firstCharacter, Character secondCharacter)
        {
            Guid = _idGenerator.Next();
            FirstCharacterAccId = firstCharacter.EntityId.Low;
            SecondCharacterAccId = secondCharacter.EntityId.Low;
        }

        public uint GetFriendId(uint low)
        {
            if (FirstCharacterAccId == low)
                return SecondCharacterAccId;
            return FirstCharacterAccId;
        }
    }
    [ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2TeleportingPointRecord")]
    public class Asda2TeleportingPointRecord : WCellRecord<Asda2TeleportingPointRecord>
    {
        private static readonly NHIdGenerator IDGenerator =
            new NHIdGenerator(typeof(Asda2TeleportingPointRecord), "Guid");

        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IDGenerator.Next();
        }

        [Property(NotNull = true)]
        public string Name { get; set; }
        internal static Asda2TeleportingPointRecord CreateRecord()
        {
            try
            {
                var itemRecord = new Asda2TeleportingPointRecord
                {
                    Guid = (uint)IDGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new Asda2FastItemSlotRecord.");
            }
        }

        [Property(NotNull = true)]
        public uint OwnerId
        {
            get;
            set;
        }
        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public short X
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public short Y { get; set; }
        [Property(NotNull = true)]
        public MapId MapId { get; set; }
        #region Loading
        public static Asda2TeleportingPointRecord[] LoadItems(uint lowCharId)
        {
            return FindAll(Restrictions.Eq("OwnerId", lowCharId));
        }

        public static Asda2TeleportingPointRecord GetRecordByID(long id)
        {
            return FindOne(Restrictions.Eq("Guid", id));
        }
        #endregion

        public static Asda2TeleportingPointRecord CreateRecord(uint ownerAccId, short x, short y, MapId mapId)
        {
            var item = CreateRecord();
            item.OwnerId = ownerAccId;
            item.X = x;
            item.Y = y;
            return item;
        }

    }
    public enum LocationSavedStatus
    {
        Fail = 0,
        Ok = 1,
        MaxCount = 2
    }
    public class Asda2TeleportCristalVector
    {
        public Vector3 To { get; set; }
        public int Price { get; set; }
        public MapId ToMap { get; set; }
    }

    public enum TeleportByCristalStaus
    {
        Ok = 1,
        CantEnterWarCauseLowPlayersInOtherFaction = 22,
        WaveIsEnding = 31, // Wave is currently ending. Please try arain after it has completed
        NotEnterUntilPlayers = 32, // You cannot enter until there are enough players.
        RejoinNot = 33, // You are not allowed to rejoin the Guild Wave
        NotRegisterWave = 34, // You cannot enter the on going wave because you did not register.
        NoWaveInfo = 35, // No Wave Info
        NotGuildInfo = 36, // There is a problem with the guild information.
    }
}
