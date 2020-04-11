using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Handlers
{
    internal class  Asda2CharacterHandler
    {
        public static void SendUpdateDurabilityResponse(IRealmClient client, Asda2Item item)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateDurability))//6570
            {
                packet.WriteByte((byte)item.InventoryType);//{inv}default value : 3 Len : 1
                packet.WriteInt16(item.Slot);//{slot}default value : 9 Len : 2
                packet.WriteByte(item.Durability);//{durability}default value : 146 Len : 1
                client.Send(packet,addEnd: true);
            }
        }
        public static void SendPetBoxSizeInitResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PetBoxSizeInit))//6123
            {
                packet.WriteInt32(chr.AccId);//{accId}default value : 361343 Len : 4
                packet.WriteByte((chr.Record.PetBoxEnchants + 1) * 6);//{size}default value : 6 Len : 1
                chr.Send(packet,addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.RepairItem)]//6571
        public static void RepairItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var invs = new byte[82];
            var slots = new short[82];
            var itemsToRepair = new List<Asda2Item>();
            for (int i = 0; i < 80; i += 1)
            {
                invs[i] = packet.ReadByte();//default : 0Len : 1
            }
            var oneItemRepairSlot = packet.ReadInt16();
            for (int i = 0; i < 79; i += 1)
            {
                slots[i] = (short) (packet.ReadInt16()-1);//default : 0Len : 2
            }
            var isOneItemRepairMode = invs[0] != 3 && slots[0]<0;
            if (isOneItemRepairMode)
            {
                if (invs[0] == 0)
                {
                    Asda2Item item = null;
                    if (oneItemRepairSlot >= 0 && oneItemRepairSlot < 22)
                        item = client.ActiveCharacter.Asda2Inventory.Equipment[oneItemRepairSlot];
                    if (item != null && item.Durability < item.MaxDurability)
                        itemsToRepair.Add(item);
                }
                else
                {
                    var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(oneItemRepairSlot);
                    if (item != null && item.Durability < item.MaxDurability)
                        itemsToRepair.Add(item);
                }
            }
            else
            {
                var isEquipmentRepairMode = invs[0] != 3;
                if (isEquipmentRepairMode)
                {
                    for (int i = 0; i < 21; i++)
                    {
                        Asda2Item item = null;
                        item = client.ActiveCharacter.Asda2Inventory.Equipment[i];
                        if(item!=null&&item.Durability<item.MaxDurability)
                            itemsToRepair.Add(item);
                    }
                }
                else
                {
                    for (int i = 0; i < 21; i++)
                    {
                        Asda2Item item = null;
                        item = client.ActiveCharacter.Asda2Inventory.Equipment[i];
                        if (item != null && item.Durability < item.MaxDurability)
                            itemsToRepair.Add(item);
                    }
                    for (short i = 0; i < 60; i++)
                    {
                        Asda2Item item = null;
                        item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(i);
                        if (item != null && item.Durability < item.MaxDurability)
                            itemsToRepair.Add(item);
                    }
                }
            }
            var repairCost = 0u;
            foreach (Asda2Item item in itemsToRepair)
                repairCost = repairCost + (item == null ? 0 : item.RepairCost());
            client.ActiveCharacter.SendInfoMsg(string.Format("Total repair cost is {0}.",repairCost));
            var enoughtMoney = client.ActiveCharacter.SubtractMoney(repairCost);
            if(!enoughtMoney)
            {
                SendRepairItemResponseResponse(client, RepairStatus.Fail, itemsToRepair);
                return;
            }
            foreach (var asda2Item in itemsToRepair)
            {
               if(asda2Item != null)
                   asda2Item.RepairDurability();
            }
            SendRepairItemResponseResponse(client, RepairStatus.Ok, itemsToRepair);
            client.ActiveCharacter.SendMoneyUpdate();
        }
        public static void SendRepairItemResponseResponse(IRealmClient client,RepairStatus status,List<Asda2Item> repairedItems )
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.RepairItemResponse)) //6572
            {
                packet.WriteInt16((byte) status); //{status}default value : 0 Len : 2
                for (int i = 0; i < 82; i += 1)
                {
                    byte inv = 0;
                    /*if (i < repairedItems.Count && repairedItems[i] != null)
                        inv = (byte) repairedItems[i].InventoryType;*/
                    packet.WriteByte(inv); //{inv}default value : 0 Len : 1
                }
                for (int i = 0; i < 79; i += 1)
                {
                    short slot = -1;
                    /*if (i < repairedItems.Count&&repairedItems[i]!=null)
                        slot = repairedItems[i].Slot;*/
                    packet.WriteInt16(slot); //{slot}default value : 0 Len : 2
                }
                for (int i = 0; i < 80; i += 1)
                {
                    byte durability = 0;
                    /*if (i < repairedItems.Count && repairedItems[i] != null)
                        durability = repairedItems[i].Durability;*/
                    packet.WriteByte(durability); //{inv}default value : 0 Len : 1
                }

                
                packet.WriteInt32(client.ActiveCharacter.Money); //{money}default value : 0 Len : 4
                client.Send(packet,addEnd: true);
            }
            foreach (var repairedItem in repairedItems)
            {
                SendUpdateDurabilityResponse(client, repairedItem);
            }
        }

        internal enum RepairStatus
        {
            Fail=0,
            Ok=1
        }


        public static void SendResurectWithChangeLocationResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ResurectWithChangeLocation))//4055
            {
                /*packet.WriteInt16(sessId);//{sessId}default value : 53 Len : 2
                packet.WriteByte(map);//{map}default value : 3 Len : 1
                packet.WriteFixedAsciiString(ipAddr);//{ipAddr}default value :  Len : 20
                packet.WriteInt16(port);//{port}default value : 15604 Len : 2
                packet.WriteInt16(x);//{x}default value : 125 Len : 2
                packet.WriteInt16(y);//{y}default value : 390 Len : 2
                packet.WriteInt32(218);//value name : unk11 default value : 218Len : 4
                packet.WriteSkip(stub33);//{stub33}default value : stub33 Len : 13*/
                client.Send(packet);
            }
        }
        static readonly byte[] stub33 = new byte[] { 0x10, 0x01, 0xF0, 0x77, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCF, 0xCF, 0xDF };

        public static void SendExpGainedResponse(ushort npcId, Character chr, int xp,bool fromKillNpc = true)
        {
            if(xp==0)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.ExpGained))//4038
            {
                packet.WriteByte(fromKillNpc?0:1);//{status}default value : 0 Len : 1
                packet.WriteInt64(chr.Experience + XpGenerator.GetStartXpForLevel(chr.Level));//{allExp}default value : 95749 Len : 8
                packet.WriteInt64(xp);//{expAmount}default value : 23 Len : 8
                packet.WriteInt16(npcId);//{diedMonstrId}default value : 83 Len : 2
                chr.Send(packet,addEnd: true);
            }
        }

        public static void SendLvlUpResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.LvlUp)) //4036
            {
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 16 Len : 2
                packet.WriteByte(chr.Level); //{level}default value : 2 Len : 1
                packet.WriteInt64(chr.Experience + XpGenerator.GetStartXpForLevel(chr.Level)); //value name : unk6 default value : 40Len : 8
                packet.WriteInt16(0); //value name : unk7 default value : 0Len : 2
                packet.WriteInt16(chr.Asda2Strength); //{str}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Agility); //{dex}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Stamina); //{vit}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Spirit); //{energy}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Intellect); //{intelect}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Luck); //{luck}default value : 6 Len : 2
                packet.WriteInt16(0); //{plusStr}default value : 0 Len : 2
                packet.WriteInt16(0); //{plusAgi}default value : 0 Len : 2
                packet.WriteInt16(0); //{plusVit}default value : 0 Len : 2
                packet.WriteInt16(0); //{plusEnrg}default value : 0 Len : 2
                packet.WriteInt16(0); //{plusInt}default value : 0 Len : 2
                packet.WriteInt16(0); //{plusLuck}default value : 0 Len : 2
                packet.WriteInt16(chr.Asda2Strength); //{str}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Agility); //{dex}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Stamina); //{vit}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Spirit); //{energy}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Intellect); //{intelect}default value : 6 Len : 2
                packet.WriteInt16(chr.Asda2Luck); //{luck}default value : 6 Len : 2
                packet.WriteInt32(chr.MaxHealth); //{maxHp}default value : 109 Len : 4
                packet.WriteInt16(chr.MaxPower); //{maxMp}default value : 100 Len : 2
                packet.WriteInt32(chr.Health); //{hp}default value : 109 Len : 4
                packet.WriteInt16(chr.Power); //{mp}default value : 100 Len : 2
                packet.WriteInt16((short) chr.MinDamage); //{minAtack}default value : 10 Len : 2
                packet.WriteInt16((short) chr.MaxDamage); //{maxAtack}default value : 11 Len : 2
                packet.WriteInt16(chr.MinMagicDamage); //{minMtak}default value : 3 Len : 2
                packet.WriteInt16(chr.MaxMagicDamage); //{maxMatk}default value : 3 Len : 2
                packet.WriteInt32(chr.MagicDefence);
                packet.WriteSkip(Stub80); //{stub69}default value : stub69 Len : 65
                chr.SendPacketToArea(packet, true, true);
            }
        }
        static readonly byte[] Stub80 = new byte[] { 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x05, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendHealthUpdate(Character chr,bool animate = false,bool animateGreenDights = false)
        {
            if (chr == null || chr.Map == null || chr.IsDeleted || !chr.Client.IsGameServerConnection)
                return;
            
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharHpUpdate))
            //4058
            {
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 9 Len : 2
                packet.WriteInt32(chr.MaxHealth); //{maxHp}default value : 100 Len : 4
                packet.WriteInt32(chr.Health); //{CurHp}default value : 60 Len : 4
                packet.WriteByte(animate?4:animateGreenDights?1:0);//if one animates green number of regened HP
                packet.WriteInt16(animate?276:-1);
                packet.WriteInt32(chr.RegenHealth); //{curHp}default value : 54 Len : 2
                chr.SendPacketToArea(packet, true, true);
            }

        }


        public static void SendCharMpUpdateResponse(Character chr)
        {
            if (chr == null || chr.Map == null || chr.IsDeleted || !chr.Client.IsGameServerConnection)
                return;

            using (var packet = new RealmPacketOut(RealmServerOpCode.CharMpUpdate))
                //4059
            {
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 9 Len : 2
                packet.WriteInt16(chr.MaxPower); //{maxMp}default value : 100 Len : 2
                packet.WriteInt16(chr.Power); //{curMp}default value : 54 Len : 2
                packet.WriteByte(0);
                packet.WriteInt16(-1);
                packet.WriteInt16(chr.RegenMana); //{curMp}default value : 54 Len : 2
                chr.Send(packet, addEnd: true);
            }

        }




        public static void SendPowerUpdate(Unit unit, PowerType type, int value)
        {
            var chr = unit as Character;
            if (type == PowerType.Health)
            {
                if (chr != null)
                    SendHealthUpdate(chr);
                return;
            }
            if (chr != null)
                SendCharMpUpdateResponse(chr);


            /*using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_POWER_UPDATE, 17))
            {
                unit.EntityId.WritePacked(packet);
                packet.Write((byte)type);
                packet.Write(value);

                unit.SendPacketToArea(packet, true);
            }*/
        }


        public static void SendSelfDeathResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SelfDeath))//4053
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 30 Len : 2
                chr.SendPacketToArea(packet, true, true);
            }
        }

        [PacketHandler(RealmServerOpCode.ResurectOnDeathPlace)]//4054
        public static void ResurectOnDeathPlaceRequest(IRealmClient client, RealmPacketIn packet)
        {
            var toTown = packet.ReadByte();
            if(toTown==8&& client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.CurrentBattleGround.TeleportToWar(client.ActiveCharacter);
                client.ActiveCharacter.Resurrect();
                return;
            }
            if (toTown!=0 &&(client.ActiveCharacter.Level > 20 || client.ActiveCharacter.IsAlive) && !CheckResurectOnDeathPlaceAuraExists(client.ActiveCharacter))
                return;
            if(toTown == 0)
                client.ActiveCharacter.Map.CallDelayed(250,()=>client.ActiveCharacter.TeleportToBindLocation());
           client.ActiveCharacter.Resurrect();
        }

        private static bool CheckResurectOnDeathPlaceAuraExists(Character activeCharacter)
        {
            foreach (var visibleAura in activeCharacter.Auras.VisibleAuras)
            {
                if(visibleAura==null)
                    continue;
                if (visibleAura.Spell.RealId == 189 || visibleAura.Spell.RealId == 190)
                    return true;
            }
            return false;
        }

        public static void SendResurectResponse(Character chr)
        {
            SendPreResurectResponse(chr);
            chr.Map.CallDelayed(50, () =>
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.Resurect))//4056
                {
                    packet.WriteInt16(chr.SessionId);//{sessId}default value : 30 Len : 2
                    packet.WriteInt16((short)chr.Asda2X);//{x}default value : 97 Len : 2
                    packet.WriteInt16((short)chr.Asda2Y);//{y}default value : 247 Len : 2
                    packet.WriteInt32(chr.Health);//{hp}default value : 50 Len : 4
                    packet.WriteInt16(chr.Power);//{mp}default value : 50 Len : 2
                    packet.WriteUInt64(chr.Experience + XpGenerator.GetStartXpForLevel(chr.Level));
                    packet.WriteInt16(0);
                    chr.SendPacketToArea(packet, true, true);
                }
            });
            
        }
        public static void SendPreResurectResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PreResurect))//5306
            {
                chr.Send(packet,addEnd: true);
            }
        }



        public static void SendUpdateStatsResponse(IRealmClient client)
        {
            if(client==null || !client.IsGameServerConnection || client.ActiveCharacter == null)
            return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateStats))//4060
            {
                packet.WriteInt32(client.ActiveCharacter.MaxHealth);//{maxHp}default value : 1615 Len : 4
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.MaxPower));//{maxMp}default value : 227 Len : 2
                packet.WriteInt32(client.ActiveCharacter.Health);//{curHp}default value : 1530 Len : 4
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Power));//{curMp}default value : 227 Len : 2
                packet.WriteInt16(ProcessOwerFlow((int) client.ActiveCharacter.MinDamage));//{minAtk}default value : 101 Len : 2
                packet.WriteInt16(ProcessOwerFlow((int) client.ActiveCharacter.MaxDamage));//{maxAtk}default value : 101 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.MinMagicDamage));//{minMatk}default value : 45 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.MaxMagicDamage));//{maxMatk}default value : 45 Len : 2
                packet.WriteInt16(ProcessOwerFlow((int) client.ActiveCharacter.Asda2MagicDefence));//{mDef}default value : 68 Len : 2
                packet.WriteInt16(ProcessOwerFlow((int) client.ActiveCharacter.Asda2Defence));//{minDef}default value : 193 Len : 2
                packet.WriteInt16(ProcessOwerFlow((int) client.ActiveCharacter.Asda2Defence));//{maxDef}default value : 211 Len : 2
                packet.WriteInt32((int)client.ActiveCharacter.BlockChance);//{maxBlock}default value : 0 Len : 4
                packet.WriteInt32(client.ActiveCharacter.BlockValue);//{minBLock}default value : 0 Len : 4
                packet.WriteInt16(15);//value name : unk18 default value : 15Len : 2
                packet.WriteInt16(7);//value name : unk19 default value : 7Len : 2
                packet.WriteInt16(4);//value name : unk20 default value : 4Len : 2
                packet.WriteSkip(stub40);//{stub40}default value : stub40 Len : 28
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stub40 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendUpdateStatsOneResponse(IRealmClient client)
        {
            if (client == null || !client.IsGameServerConnection || client.ActiveCharacter == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateStatsOne)) //4064
            {
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Strength)); //{strength}default value : 166 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Agility)); //{dex}default value : 74 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Stamina)); //{stamina}default value : 120 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Spirit)); //{spirit}default value : 48 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Intellect)); //{int}default value : 74 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Luck)); //{skill5Id}default value : 48 Len : 2
                packet.WriteInt16(0); //{strPlus}default value : 249 Len : 2
                packet.WriteInt16(0); //{dexPlus}default value : 33 Len : 2
                packet.WriteInt16(0); //{stamPlus}default value : 18 Len : 2
                packet.WriteInt16(0); //{spiritPlus}default value : 0 Len : 2
                packet.WriteInt16(0); //{intPlus}default value : 0 Len : 2
                packet.WriteInt16(0); //{luckPlus}default value : 0 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Strength)); //{strength}default value : 166 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Agility)); //{dex}default value : 74 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Stamina)); //{stamina}default value : 120 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Spirit)); //{spirit}default value : 48 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Intellect)); //{int}default value : 74 Len : 2
                packet.WriteInt16(ProcessOwerFlow(client.ActiveCharacter.Asda2Luck)); //{skill5Id}default value : 48 Len : 2
                client.Send(packet, addEnd: true);
            }
        }

        private const int O1 = short.MaxValue*1000;

        public static short ProcessOwerFlow(int value)
        {
            if (value < short.MaxValue)
                return (short) value;
            if (value > O1)
                return (short) (value/1000000);
            return (short) (value/1000);
        }
        [PacketHandler(RealmServerOpCode.SelectCharacter)]//6538
        public static void SelectCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            var targetAccId = packet.ReadInt32();//default : 0Len : 4
            var sessId = packet.ReadUInt16();//default : 0Len : 2
            var chr = client.ActiveCharacter;
            var target = World.GetCharacterBySessionId(sessId);
            if(target==null)return;
            if(target.Map != chr.Map)
            {
                //chr.YouAreFuckingCheater("Selected character from another map.",50);
                return;
            }
            client.ActiveCharacter.Target = target;
            SendSelectCharacterResponse(client, target);
        }
        public static void SendSelectCharacterResponse(IRealmClient client,Character chr)
        {
            using (var packet = SelectCharacterInfo(chr))
            {
                client.Send(packet, addEnd: false);
            }
        }
        public static void SendSelectedCharacterInfoToMultipyTargets(Character chr,Character[] targets)
        {
            using (var packet = SelectCharacterInfo(chr))
            {
                foreach (var character in targets)
                {
                    character.Send(packet, addEnd: false);
                }
            }
        }
        public static RealmPacketOut SelectCharacterInfo(Character selectedChr)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.SelectCharacterRespone); //6539
            packet.WriteByte(1); //{result}default value : 1 Len : 1
            packet.WriteInt32(selectedChr.AccId); //{accId}default value : 0 Len : 4
            packet.WriteInt32(selectedChr.MaxHealth); //{hpMax}default value : 2000 Len : 4
            packet.WriteInt32(selectedChr.Health); //{hpMin}default value : 1000 Len : 4
            packet.WriteInt16(selectedChr.MaxPower); //{mpMan}default value : 100 Len : 2
            packet.WriteInt16(selectedChr.Power); //{mpMin}default value : 50 Len : 2
            packet.WriteInt32(0);
            return packet;

        }

        #region Create

        /// <summary>
        /// Handles an incoming character creation request.
        /// TODO: Add protection against char creation/deletion spam
        /// </summary>
        [PacketHandler(RealmServerOpCode.CreateCharacterRequest, IsGamePacket = false, RequiresLogin = false)] //1014
        public static void CreateCharacterRequest(IRealmClient client, RealmPacketIn packet)
        {
            var acc = client.Account;
            if (acc == null || client.ActiveCharacter != null)
                return;
            packet.ReadUInt32(); //default : 0 accId
            packet.Position += 2; //unknown default : 0
            var characterNum = packet.ReadByte(); //default : 0
            if (characterNum < 10 || characterNum > 12)
            {
                SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
                return;
            }
            var name = packet.ReadAsdaString(20,Locale.En); //default : "",20
            var gender = packet.ReadByte(); //unknown default : 2
            var hair = packet.ReadByte(); //default : 0
            var color = packet.ReadByte(); //default : 0
            var face = packet.ReadByte(); //default : 0
            var zodiac = packet.ReadByte(); //default : 0
            var exist = CharacterRecord.Exists(name);
            if (exist)
            {
                SendCreateCharacterResponse(client, CharecterCreateResult.AlreadyInUse);
                return;
            }
            if (!IsNameValid(name) || acc.Characters.Count() > 2)
            {
                SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
                return;
            }
            var record = CharacterRecord.CreateNewCharacterRecord(client.Account, name);

            if (record == null)
            {
                SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
                return;
            }

            record.Gender = (GenderType)gender;
            record.Skin = 0;
            record.Face = face;
            record.HairStyle = hair;
            record.HairColor = color;
            record.FacialHair = 0;
            record.Outfit = 0;
            record.GodMode = acc.Role.AppearAsGM;
            record.CharNum = characterNum;
            record.Zodiac = zodiac;
            record.EntityLowId = Character.CharacterIdFromAccIdAndCharNum((int) acc.AccountId,characterNum);

            record.SetupNewRecord(ArchetypeMgr.GetArchetype(RaceId.Human, ClassId.NoClass));

            var charCreateTask = new Message2<IRealmClient, CharacterRecord>
                                     {
                                         Callback = CharCreateCallback,
                                         Parameter1 = client,
                                         Parameter2 = record
                                     };

            ServerApp<RealmServer>.IOQueue.AddMessage(charCreateTask);
        }

        public static void SendCreateCharacterResponseOneResponse(IRealmClient client, CharacterRecord newCharecter)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CreateCharacterResponseOne)) //1015
            {
                packet.WriteInt32((int)newCharecter.AccountId); //default value : 0
                packet.WriteAsdaString(newCharecter.Name, 18); //default value : "",18
                packet.WriteInt16(0); //value name : _
                packet.WriteInt16(newCharecter.CharNum); //default value : 10
                packet.WriteByte(0); //value name : _
                packet.WriteByte((byte)(newCharecter.Gender)); //default value : 1
                packet.WriteInt16(0); //value name : _
                packet.WriteInt64(1); //value name : _
                packet.WriteInt32(0); //value name : _
                packet.WriteInt16(0); //value name : _
                packet.WriteInt32(131807896); //value name : _
                packet.WriteInt64(0); //value name : _
                packet.WriteByte(0); //value name : _
                packet.WriteInt32(7683); //value name : _
                packet.WriteInt16(0); //value name : _
                packet.WriteByte(0); //value name : _
                packet.WriteInt16(-1); //value name : _
                packet.WriteInt16(0); //value name : _
                packet.WriteByte(newCharecter.Zodiac); //default value : 0
                packet.WriteByte(newCharecter.HairStyle); //default value : 0
                packet.WriteByte(newCharecter.HairColor); //default value : 0
                packet.WriteByte(newCharecter.Face); //default value : 0
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendCreateCharacterResponse(IRealmClient client, CharecterCreateResult result)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CreateCharacterResponse)) //1026
            {
                packet.WriteByte((byte)result); //result
                client.Send(packet, addEnd: false);
            }
        }

        private static void CharCreateCallback(IRealmClient client, CharacterRecord newCharRecord)
        {
            // check again, to avoid people creating 2 chars with the same name at the same time screwing up the server
            if (CharacterRecord.Exists(newCharRecord.Name))
            {
                SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
            }
            else
            {
                try
                {
                    newCharRecord.CreateAndFlush();
                }
                catch (Exception e)
                {
                    //LogUtil.ErrorException(e, "Could not create Character \"{0}\" for: {1}", newCharRecord.Name, client.Account);
                    //SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_ERROR);
                    try
                    {
                        RealmDBMgr.OnDBError(e);
                        newCharRecord.CreateAndFlush();
                    }
                    catch (Exception)
                    {
                        SendCreateCharacterResponse(client, CharecterCreateResult.BadName);
                        return;
                    }
                }


                SendCreateCharacterResponseOneResponse(client, newCharRecord);
                SendCreateCharacterResponse(client, CharecterCreateResult.Ok);
                client.Account.Characters.Add(newCharRecord);

                //SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_SUCCESS);
            }
        }



        #endregion

        public static void SendSomeInitGSResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SomeInitGS)) //4046
            {
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendSomeInitGSOneResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SomeInitGSOne))//4049
            {
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendCharacterInfoSessIdPositionResponse(IRealmClient client)
        {
            var ac = client.ActiveCharacter;
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterInfoSessIdPosition))//4003
            {
                packet.WriteInt16(ac.SessionId);//default value : 0
                packet.WriteInt16(Convert.ToInt16(ac.Asda2X));//default value : 0
                packet.WriteInt16(Convert.ToInt16(ac.Asda2Y));//default value : 0
                packet.WriteInt16(-1);
                packet.WriteByte(client.ActiveCharacter.SettingsFlags[15]);
                packet.WriteByte(client.ActiveCharacter.AvatarMask);
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendMySessionIdResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MySessionId))//6066
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 22 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 354889 Len : 4
                packet.WriteInt16(0);//value name : unk6 default value : 0Len : 2
                packet.WriteSkip(stab14);//value name : stab14 default value : stab14Len : 3
                packet.WriteInt16((Int16) client.ActiveCharacter.Archetype.ClassId);//{profession}default value : 0 Len : 2
                packet.WriteSkip(stub13);//{stub13}default value : stub13 Len : 61
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stab14 = new byte[] { 0x00, 0x00, 0x00 };
        static readonly byte[] stub13 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };





      
        #region emote
        [PacketHandler(RealmServerOpCode.Emote)]//4024
        public static void EmoteRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 6;//default : md5Len : 16
            /*var sessId = packet.ReadInt16();//default : 15Len : 2
            var accId = packet.ReadInt32();*/
            var emoteType = packet.ReadInt16();//default : 113Len : 2
            if (emoteType == 108)
            {
                client.ActiveCharacter.IsSitting = true;
                return;
            }
            if (emoteType == 109)
            {
                client.ActiveCharacter.IsSitting = false;
                return;
            }
            if (emoteType == 131) //&& client.ActiveCharacter.IsMoving)
                return;

            var c = packet.ReadByte();//default : 144Len : 1
            var a = packet.ReadSingle();//default : 0,3246512Len : 4
            var b = packet.ReadSingle();//default : 0,9458339Len : 4
            SendEmoteResponse(client.ActiveCharacter,emoteType,c,a,b);
        }
        [NotVariable]
        static byte _emoteCnt;
        public static void SendEmoteResponse(Character chr, short emote,byte c = 1,float a = 0.0596617f,float b= -0.99822187f)
        {
            Asda2TitleChecker.EmoteChecker.OnEmote(emote, chr);
            using (var packet = new RealmPacketOut(RealmServerOpCode.EmoteResponse))//4025
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 25 Len : 2
                packet.WriteInt32(chr.AccId);//{sessId}default value : 25 Len : 2
                packet.WriteInt16(emote);//{emote}default value : 113 Len : 2
                packet.WriteByte(c);//{c}default value : 1 Len : 1
                packet.WriteFloat(a);//{a}default value : 0,05966117 Len : 4
                packet.WriteFloat(b);//{b}default value : -0,9982187 Len : 4
                packet.WriteByte(_emoteCnt++);
                chr.SendPacketToArea(packet,false, true);
            }
        }
        public static void SendEmoteResponseToOneTarget(Character chr, short emote, byte c, float a, float b,IRealmClient rcv)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.EmoteResponse))//4025
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 25 Len : 2
                packet.WriteInt32(chr.AccId);//{sessId}default value : 25 Len : 2
                packet.WriteInt16(emote);//{emote}default value : 113 Len : 2
                packet.WriteByte(c);//{c}default value : 1 Len : 1
                packet.WriteFloat(a);//{a}default value : 0,05966117 Len : 4
                packet.WriteFloat(b);//{b}default value : -0,9982187 Len : 4
                packet.WriteByte(_emoteCnt++);
                rcv.Send(packet, addEnd: true);
            }
        }
        public static void SendUpdateAvatarMaskResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.EmoteResponse))//4025
            {
                packet.WriteInt16(chr.SessionId);//{sessId}default value : 25 Len : 2
                packet.WriteInt32(-1);//{sessId}default value : 25 Len : 2
                packet.WriteInt16(111);//{emote}default value : 113 Len : 2
                packet.WriteByte(chr.SettingsFlags[15]);//{c}default value : 1 Len : 1
                packet.WriteInt32(chr.AvatarMask);//{a}default value : 0,05966117 Len : 4
                packet.WriteInt32(0);//{b}default value : -0,9982187 Len : 4
                chr.SendPacketToArea(packet, false, true);
            }
        }
        #endregion
        public static void SendLearnedSkillsInfo(Character character)
        {
            var SpellGroups = new List<List<Spell>>();
            var i = 0;
            bool createNewGroup = true;
            List<Spell> curSpellGroup = null;
            foreach (var spell in character.Spells)
            {
                if(createNewGroup)
                {
                    i = 0;
                    curSpellGroup = new List<Spell>();
                    SpellGroups.Add(curSpellGroup);
                    createNewGroup = false;
                }
                curSpellGroup.Add(spell);
                i++;
                if (i >= 18)
                    createNewGroup = true;
            }
            foreach (var spellGroup in SpellGroups)
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.SkillsInfo))//5251
                {
                    foreach (var spell in spellGroup)
                    {
                        packet.WriteUInt16(spell.RealId);//{id}default value : 500 Len : 2
                        packet.WriteByte(spell.Level);//{isLearned}default value : 0 Len : 1
                        packet.WriteByte(1);//value name : unk default value : 1Len : 1
                        packet.WriteInt32(spell.CooldownTime);//{cooldown}default value : -1 Len : 2
                        //packet.WriteInt16(-1);//value name : unk default value : -1Len : 2
                        packet.WriteInt16(256);//value name : unk2 default value : -1Len : 2
                        packet.WriteInt16(spell.PowerCost);//{mpCost}default value : -1 Len : 2
                        packet.WriteInt16(spell.Effect0_MiscValue);//{prcBoost}default value : -1 Len : 2
                        packet.WriteByte(100);//value name : unk default value : -1Len : 2
                        packet.WriteByte(100);//value name : unk default value : -1Len : 2
                        packet.WriteInt16(4);//value name : unk2 default value : -1Len : 2
                        packet.WriteInt32(150000);//value name : unk2 default value : -1Len : 2
                        packet.WriteInt16(0);//value name : unk2 default value : -1Len : 2
                        packet.WriteInt64(0);//value name : unk default value : -1Len : 8
                        packet.WriteInt16(0);//value name : unk default value : -1Len : 2
                    }
                    character.Send(packet, addEnd: false);
                }
            }
            
        }

        public static void SendFactionAndHonorPointsInitResponse(IRealmClient client)
        {
            if(!client.IsGameServerConnection)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.FactionAndHonorPointsInit))//6713
            {
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId);//{faction}default value : 0 Len : 2
                packet.WriteInt32(client.ActiveCharacter.Asda2HonorPoints);//{honorPoints}default value : 0 Len : 4
                packet.WriteByte(client.ActiveCharacter.Asda2FactionRank);//{rank}default value : 1 Len : 1
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendChangeProfessionResponse(IRealmClient client)
        {
            var chr = client.ActiveCharacter;
            if(chr.Spells == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.ChangeProfession))//5042
            {
                packet.WriteSkip(stab6);//value name : stab6 default value : stab6Len : 1
                packet.WriteInt32((int) chr.Account.AccountId);//{accId}default value : 131548 Len : 4
                packet.WriteByte(2);//value name : unk6 default value : 2Len : 1
                packet.WriteInt16(32);//value name : unk7 default value : 32Len : 2
                packet.WriteInt16(0);//value name : unk8 default value : 0Len : 2
                packet.WriteByte(3);//value name : unk9 default value : 3Len : 1
                packet.WriteByte(chr.ProfessionLevel);//{profLevel}default value : 23 Len : 1
                packet.WriteByte((byte) chr.Archetype.ClassId);//{Class}default value : 23 Len : 1
                packet.WriteByte(chr.Spells.AvalibleSkillPoints < 0 ? 0 : chr.Spells.AvalibleSkillPoints);//{skilPoints}default value : 11 Len : 1
                packet.WriteByte(0);//value name : unk13 default value : 0Len : 1
                packet.WriteInt32(2475);//value name : unk14 default value : 2475Len : 4
                packet.WriteInt16(260);//value name : unk15 default value : 260Len : 2
                packet.WriteInt32(0);//{harisma}default value : 33554692 Len : 4
                packet.WriteSkip(stab31);//value name : stab31 default value : stab31Len : 466
                packet.WriteInt32(chr.Money);//{money}default value : 583867710 Len : 4
                packet.WriteSkip(stab501);//value name : stab501 default value : stab501Len : 20
                chr.Send(packet, addEnd: false);
            }
        }
        static readonly byte[] stab6 = new byte[] { 0x00 };
        static readonly byte[] stab31 = new byte[] { 0x04, 0x01, 0x00, 0x02, 0xDC, 0x01, 0x01, 0x00, 0xEA, 0x69, 0x29, 0x51, 0x00, 0x00, 0x03, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x02, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x02, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0xFC, 0x5E, 0x00, 0x00, 0x01, 0x05, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x64, 0x7C, 0x02, 0x94, 0x21, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x54, 0x00, 0x09, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x69, 0x50, 0x00, 0x00, 0x02, 0x0D, 0x00, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x78, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] stab501 = new byte[] { 0x04, 0x01, 0x00, 0x02, 0xDC, 0x01, 0x01, 0x00, 0xEA, 0x69, 0x29, 0x51, 0x00, 0x00, 0x03, 0x00, 0xFF, 0xFF, 0xFF, 0xFF };
        [PacketHandler(RealmServerOpCode.IHaveLearnedTutorial)]//6184
        public static void IHaveLearnedTutorialRequest(IRealmClient client, RealmPacketIn packet)
        {
            /*var charNum = packet.ReadInt16();//default : 12Len : 2
            var tutorialId = packet.ReadInt16();//default : 24Len : 2*/
        }

        [PacketHandler(RealmServerOpCode.SettingsFlags)]//4070
        public static void SettingsFlagsRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 12;

            var characterFlags = new byte[16];
            for (int i = 0; i < 16; i += 1)
            {
                var charSettingsFlag = packet.ReadByte();//default : 1Len : 1
                characterFlags[i] = charSettingsFlag;
            }
            client.ActiveCharacter.SettingsFlags = characterFlags;
            client.ActiveCharacter.AvatarMask = packet.ReadInt32();//default : 0Len : 4
        }
        [PacketHandler(RealmServerOpCode.SelectFactionReq)]//6702
        public static void SelectFactionReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            var factionId = packet.ReadByte();//default : 1Len : 1
            if(factionId > 1)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to take wrong faction",50);
                return;
            }
            if (client.ActiveCharacter.Asda2FactionId != -1)
            {
                if (!client.ActiveCharacter.GodMode)
                {
                    SendSelectFactionResResponse(client, SelectFactionStatus.YouAlreadyHaveFaction);
                    return;
                }
            }
            if (client.ActiveCharacter.RealProffLevel < 1 && !client.ActiveCharacter.GodMode)
            {
                SendSelectFactionResResponse(client, SelectFactionStatus.AllowedOnlyFor2JobCharacters); return;
            }
            client.ActiveCharacter.Asda2FactionId = factionId;
            Asda2TitleChecker.OnSelectFaction(factionId, client.ActiveCharacter);
            SendSelectFactionResResponse(client, SelectFactionStatus.Ok);
            GlobalHandler.SendCharacterFactionToNearbyCharacters(client.ActiveCharacter);
        }
        public static void SendSelectFactionResResponse(IRealmClient client,SelectFactionStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SelectFactionRes))//6703
            {
                packet.WriteByte((byte) status);//{status}default value : 3 Len : 1
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId);//value name : unk5 default value : -1Len : 2
                client.Send(packet, addEnd: false);
            }
        }
        public static bool IsNameValid(string characterName)
        {
            if (characterName.Length == 0)
            {
                return false;
            }

            if (characterName.Length < 3)
            {
                return false;
            }

            if (characterName.Length > 18)
            {
                return false;
            }
            return Asda2EncodingHelper.IsPrueEnglishName(characterName);
        }


    }
    public enum SelectFactionStatus
    {
        Failed = 0,
        Ok =1,
        YouAlreadyHaveFaction =2,
        AllowedOnlyFor2JobCharacters = 3,
        FactionIsFull =4,
        AnotherFactionHasAlreadySelectThisBattleArea=5,
    }
    public enum CharacterSettingsFlag
    {
        EnableWishpers = 5,
        EnableSoulmateRequest = 7,
        EnableFriendRequest = 8,
        EnablePartyRequest = 9,
        EnableGuildRequest =10,
        EnableGeneralTradeRequest =11,
        EnableGearTradeRequest =12,
        DisplayMonstrHelath = 13,
        ShowSelfNameAndHealth = 14,
        DisplayHemlet = 15,
    }
}
