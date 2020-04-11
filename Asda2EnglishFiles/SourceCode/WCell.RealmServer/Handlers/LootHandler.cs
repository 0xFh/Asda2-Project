using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Looting;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class LootHandler
    {
        /// <summary>A client wants to loot something (usually a corpse)</summary>
        public static void HandleLoot(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            Asda2LooterEntry looterEntry = activeCharacter.LooterEntry;
            WorldObject worldObject = activeCharacter.Map.GetObject(id);
            if (worldObject == null)
                return;
            looterEntry.TryLoot((IAsda2Lootable) worldObject);
        }

        /// <summary>
        /// Gold is given automatically in group-looting. Client can only request gold when looting alone.
        /// </summary>
        public static void HandleLootMoney(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            Asda2LooterEntry looterEntry = activeCharacter.LooterEntry;
            activeCharacter.SpellCast.Cancel(SpellFailedReason.Interrupted);
            Asda2Loot loot = looterEntry.Loot;
            if (loot == null)
                return;
            loot.GiveMoney();
        }

        /// <summary>Client finished looting</summary>
        public static void HandleLootRelease(IRealmClient client, RealmPacketIn packet)
        {
            Asda2LooterEntry looterEntry = client.ActiveCharacter.LooterEntry;
            if (looterEntry.Loot == null)
                return;
            looterEntry.Loot = (Asda2Loot) null;
        }

        public static void HandleRoll(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleMasterGive(IRealmClient client, RealmPacketIn packet)
        {
        }

        /// <summary>Sets the automatic PassOnLoot rolls option.</summary>
        public static void HandleOptOutOfLoot(IRealmClient client, RealmPacketIn packet)
        {
            bool flag = packet.ReadUInt32() > 0U;
            client.ActiveCharacter.PassOnLoot = flag;
        }

        public static void SendLootFail(Character looter, ILootable lootable)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_RESPONSE))
            {
                packet.Write((ulong) lootable.EntityId);
                packet.Write(0U);
                looter.Client.Send(packet, false);
            }
        }

        public static void SendLootResponse(Character looter, Loot loot)
        {
        }

        public static void SendLootReleaseResponse(Character looter, Loot loot)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_RELEASE_RESPONSE))
            {
                packet.Write((ulong) looter.EntityId);
                packet.WriteByte(1);
                looter.Client.Send(packet, false);
            }
        }

        public static void SendLootRemoved(Character looter, uint index)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_REMOVED))
            {
                packet.WriteByte(index);
                looter.Client.Send(packet, false);
            }
        }

        /// <summary>Your share of the loot is %d money.</summary>
        public static void SendMoneyNotify(Character looter, uint amount)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_MONEY_NOTIFY))
            {
                packet.WriteUInt(amount);
                packet.WriteByte(looter.IsInGroup ? 0 : 1);
                looter.Client.Send(packet, false);
            }
        }

        public static void SendClearMoney(Loot loot)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_CLEAR_MONEY))
            {
                foreach (LooterEntry looter in (IEnumerable<LooterEntry>) loot.Looters)
                {
                    if (looter.Owner != null)
                        looter.Owner.Client.Send(packet, false);
                }
            }
        }

        public static void SendStartRoll(Loot loot, LootItem item, IEnumerable<LooterEntry> looters, MapId mapid)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_START_ROLL))
            {
                packet.Write((ulong) loot.Lootable.EntityId);
                packet.WriteUInt((uint) mapid);
                packet.Write(item.Index);
                packet.Write(item.Template.Id);
                packet.Write(item.Template.RandomSuffixFactor);
                packet.Write(item.Template.RandomSuffixFactor > 0U
                    ? (int) -item.Template.RandomSuffixId
                    : (int) item.Template.RandomPropertiesId);
                packet.Write((byte) 15);
                foreach (LooterEntry looter in looters)
                {
                    if (looter.Owner != null)
                        looter.Owner.Client.Send(packet, false);
                }
            }
        }

        public static void SendRoll(Character looter, Loot loot, LootItem item, int rollNumber, LootRollType rollType)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ROLL))
            {
                packet.Write((ulong) loot.Lootable.EntityId);
                packet.Write(item.Index);
                packet.Write((ulong) looter.EntityId);
                packet.Write(item.Template.Id);
                packet.Write(item.Template.RandomSuffixFactor);
                packet.Write(item.Template.RandomSuffixFactor > 0U
                    ? (int) -item.Template.RandomSuffixId
                    : (int) item.Template.RandomPropertiesId);
                packet.Write(rollNumber);
                packet.Write((byte) rollType);
                foreach (LooterEntry looter1 in (IEnumerable<LooterEntry>) loot.Looters)
                {
                    if (looter1.Owner != null)
                        looter1.Owner.Client.Send(packet, false);
                }
            }
        }

        public static void SendItemNotify(Character looter, Loot loot)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ITEM_NOTIFY))
                looter.Client.Send(packet, false);
        }

        public static void SendAllPassed(Character looter, Loot loot, LootItem item)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ALL_PASSED))
                looter.Client.Send(packet, false);
        }

        public static void SendRollWon(Character looter, Loot loot, LootItem item, LootRollEntry entry)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ROLL_WON))
            {
                packet.Write((ulong) loot.Lootable.EntityId);
                packet.Write(item.Index);
                packet.Write((ulong) looter.EntityId);
                packet.Write(item.Template.Id);
                packet.Write(item.Template.RandomSuffixFactor);
                packet.Write(item.Template.RandomSuffixFactor > 0U
                    ? (int) -item.Template.RandomSuffixId
                    : (int) item.Template.RandomPropertiesId);
                packet.Write((ulong) looter.EntityId);
                packet.Write(entry.Number);
                packet.Write((int) entry.Type);
                foreach (LooterEntry looter1 in (IEnumerable<LooterEntry>) loot.Looters)
                {
                    if (looter1.Owner != null)
                        looter1.Owner.Client.Send(packet, false);
                }
            }
        }

        public static void SendMasterList(Character looter, Loot loot)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_MASTER_LIST))
                looter.Client.Send(packet, false);
        }
    }
}