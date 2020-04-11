using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
    public static class NPCHandler
    {
        public static void HandleBankActivate(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC npc = activeCharacter.Map.GetObject(id) as NPC;
            if (npc == null || !npc.IsBanker || !npc.CheckVendorInteraction(activeCharacter))
                return;
            activeCharacter.OpenBank((WorldObject) npc);
        }

        /// <summary>Auto-move item from Bank to Inventory</summary>
        public static void SendBankSlotResult(Character chr, BuyBankBagResponse response)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_BUY_BANK_SLOT_RESULT, 4))
            {
                packet.Write((uint) response);
                chr.Send(packet, false);
            }
        }

        public static void HandlePetitionerShowList(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            NPC petitioner = client.ActiveCharacter.Map.GetObject(id) as NPC;
            if (petitioner == null || !petitioner.NPCFlags.HasFlag((Enum) NPCFlags.Petitioner))
                return;
            petitioner.SendPetitionList(client.ActiveCharacter);
        }

        public static void SendPetitionList(this NPC petitioner, Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PETITION_SHOWLIST, 32))
            {
                packet.Write((ulong) petitioner.EntityId);
                if (petitioner.IsGuildPetitioner)
                {
                    packet.Write(1);
                    packet.Write((uint) PetitionerEntry.GuildPetitionEntry.ItemId);
                    packet.Write(PetitionerEntry.GuildPetitionEntry.DisplayId);
                    packet.Write(PetitionerEntry.GuildPetitionEntry.Cost);
                    packet.Write(0);
                    packet.Write(PetitionerEntry.GuildPetitionEntry.RequiredSignatures);
                }
                else if (petitioner.IsArenaPetitioner)
                {
                    packet.Write(PetitionerEntry.ArenaPetition2v2Entry.Index);
                    packet.Write((uint) PetitionerEntry.ArenaPetition2v2Entry.ItemId);
                    packet.Write(PetitionerEntry.ArenaPetition2v2Entry.DisplayId);
                    packet.Write(PetitionerEntry.ArenaPetition2v2Entry.Cost);
                    packet.Write((uint) PetitionerEntry.ArenaPetition2v2Entry.RequiredSignatures);
                    packet.Write(PetitionerEntry.ArenaPetition3v3Entry.Index);
                    packet.Write((uint) PetitionerEntry.ArenaPetition3v3Entry.ItemId);
                    packet.Write(PetitionerEntry.ArenaPetition3v3Entry.DisplayId);
                    packet.Write(PetitionerEntry.ArenaPetition3v3Entry.Cost);
                    packet.Write((uint) PetitionerEntry.ArenaPetition3v3Entry.RequiredSignatures);
                    packet.Write(PetitionerEntry.ArenaPetition5v5Entry.Index);
                    packet.Write((uint) PetitionerEntry.ArenaPetition5v5Entry.ItemId);
                    packet.Write(PetitionerEntry.ArenaPetition5v5Entry.DisplayId);
                    packet.Write(PetitionerEntry.ArenaPetition5v5Entry.Cost);
                    packet.Write((uint) PetitionerEntry.ArenaPetition5v5Entry.RequiredSignatures);
                }

                chr.Client.Send(packet, false);
            }
        }

        public static void HandlePetitionSign(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadEntityId();
        }

        public static void HandlePetitionDecline(IRealmClient client, RealmPacketIn packet)
        {
            PetitionRecord record = PetitionRecord.LoadRecordByItemId(packet.ReadEntityId().Low);
            NPCHandler.SendPetitionDecline((IPacketReceiver) client, client.ActiveCharacter, record);
        }

        public static void SendPetitionSignatures(IPacketReceiver client, PetitionCharter charter)
        {
            if (charter.Petition == null)
                return;
            List<uint> signedIds = charter.Petition.SignedIds;
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PETITION_SHOW_SIGNATURES,
                    21 + signedIds.Count * 12))
            {
                packet.WriteULong(charter.EntityId.Full);
                packet.WriteULong(charter.Owner.EntityId.Full);
                packet.WriteUInt(charter.EntityId.Low);
                packet.WriteByte(signedIds.Count);
                foreach (uint val in signedIds)
                {
                    packet.WriteULong(val);
                    packet.WriteUInt(0);
                }

                client.Send(packet, false);
            }
        }

        public static void SendPetitionSignResults(IPacketReceiver client, GuildTabardResult result)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PETITION_SIGN_RESULTS))
                client.Send(packet, false);
        }

        public static void SendPetitionDecline(IPacketReceiver client, Character chr, PetitionRecord record)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.MSG_PETITION_DECLINE, 8))
            {
                Character character = WCell.RealmServer.Global.World.GetCharacter(record.OwnerId);
                if (character == null)
                    return;
                packet.WriteULong(chr.EntityId.Full);
                character.Client.Send(packet, false);
            }
        }

        public static void SendPetitionTurnInResults(IPacketReceiver client, PetitionTurns result)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TURN_IN_PETITION_RESULTS, 4))
            {
                packet.WriteUInt((uint) result);
                client.Send(packet, false);
            }
        }

        public static void SendPetitionRename(IPacketReceiver client, PetitionCharter petition)
        {
            string name = petition.Petition.Name;
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.MSG_PETITION_RENAME, 8 + name.Length + 1))
            {
                packet.WriteULong(petition.EntityId.Full);
                packet.WriteCString(name);
                client.Send(packet, false);
            }
        }

        public static void SendPetitionQueryResponse(IPacketReceiver client, PetitionCharter charter)
        {
            string name = charter.Petition.Name;
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PETITION_QUERY_RESPONSE,
                12 + name.Length + 1 + 1 + 48 + 2 + 10))
            {
                packet.WriteUInt(charter.EntityId.Low);
                packet.WriteULong(charter.Owner.EntityId.Full);
                packet.WriteCString(name);
                packet.WriteByte(0);
                uint type = (uint) charter.Petition.Type;
                if (type == 9U)
                {
                    packet.WriteUInt(type);
                    packet.WriteUInt(type);
                    packet.WriteUInt(0);
                }
                else
                {
                    packet.WriteUInt(type - 1U);
                    packet.WriteUInt(type - 1U);
                    packet.WriteUInt(type);
                }

                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUShort(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                for (int index = 0; index < 10; ++index)
                    packet.WriteByte(0);
                packet.WriteUInt(0);
                if (type == 9U)
                    packet.WriteUInt(0);
                else
                    packet.WriteUInt(1);
                client.Send(packet, false);
            }
        }

        public static void HandleVendorListInventory(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            NPC npc = client.ActiveCharacter.Map.GetObject(id) as NPC;
            if (npc == null || !npc.IsVendor)
                return;
            npc.VendorEntry.UseVendor(client.ActiveCharacter);
        }

        public static void SendNPCError(IPacketReceiver client, IEntity vendor, VendorInventoryError error)
        {
            using (RealmPacketOut realmPacketOut =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_LIST_INVENTORY, 10))
            {
                realmPacketOut.Write((ulong) vendor.EntityId);
                realmPacketOut.Write((byte) 0);
                realmPacketOut.Write((byte) error);
            }
        }

        /// <summary>
        /// Send the vendor's list of items for sale to the client.
        /// *All allowable race and class checks should be done prior to calling this method.
        /// *All checks on number-limited items should also be done prior to calling this method.
        /// *This method can handle up to 256 items per vendor. If you try to send more items than that,
        /// this method will send only the first 256.
        /// </summary>
        /// <param name="client">The client to send the packet to.</param>
        /// <param name="itemsForSale">An array of items to send to the client.</param>
        public static void SendVendorInventoryList(Character buyer, NPC vendor, List<VendorItemEntry> itemsForSale)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_LIST_INVENTORY,
                10 + 28 * itemsForSale.Count<VendorItemEntry>()))
            {
                packet.Write((ulong) vendor.EntityId);
                long position = packet.Position;
                packet.WriteByte(0);
                int val = 0;
                foreach (VendorItemEntry vendorItemEntry in itemsForSale.Where<VendorItemEntry>(
                    (Func<VendorItemEntry, bool>) (item => item != null)))
                {
                    if (buyer.GodMode ||
                        (vendorItemEntry.Template.RequiredClassMask.HasAnyFlag(buyer.Class) ||
                         vendorItemEntry.Template.BondType != ItemBondType.OnPickup) &&
                        (!vendorItemEntry.Template.Flags2.HasAnyFlag(ItemFlags2.HordeOnly) ||
                         !buyer.Faction.IsAlliance) &&
                        (!vendorItemEntry.Template.Flags2.HasAnyFlag(ItemFlags2.AllianceOnly) ||
                         !buyer.Faction.IsHorde))
                    {
                        ++val;
                        if (val <= (int) byte.MaxValue)
                        {
                            packet.Write(val);
                            uint discountedCost = buyer.Reputations.GetDiscountedCost(vendor.Faction.ReputationIndex,
                                vendorItemEntry.Template.BuyPrice);
                            packet.Write(vendorItemEntry.Template.Id);
                            packet.Write(vendorItemEntry.Template.DisplayId);
                            packet.Write(vendorItemEntry.RemainingStockAmount);
                            packet.Write(discountedCost);
                            packet.Write(vendorItemEntry.Template.MaxDurability);
                            packet.Write(vendorItemEntry.BuyStackSize);
                            packet.Write(vendorItemEntry.ExtendedCostId);
                        }
                        else
                            break;
                    }
                }

                packet.Position = position;
                packet.WriteByte(val);
                if (val == 0)
                    packet.Write((byte) 0);
                buyer.Send(packet, false);
            }
        }

        /// <summary>Sends a sell-error packet to the client</summary>
        /// <param name="client">The IRealmClient to send the error to.</param>
        /// <param name="error">A SellItemError</param>
        public static void SendSellError(IPacketReceiver client, EntityId vendorId, EntityId itemId,
            SellItemError error)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SELL_ITEM, 17))
            {
                packet.Write((ulong) vendorId);
                packet.Write((ulong) itemId);
                packet.Write((byte) error);
                client.Send(packet, false);
            }
        }

        public static void SendBuyError(IPacketReceiver client, IEntity vendor, Asda2ItemId itemEntryId,
            BuyItemError error)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_BUY_FAILED, 13))
            {
                packet.Write((ulong) vendor.EntityId);
                packet.Write((uint) itemEntryId);
                packet.Write((byte) error);
                client.Send(packet, false);
            }
        }

        public static void SendBuyItem(IPacketReceiver client, IEntity vendor, Asda2ItemId itemId,
            int numItemsPurchased)
        {
            NPCHandler.SendBuyItem(client, vendor, itemId, numItemsPurchased, 0);
        }

        public static void SendBuyItem(IPacketReceiver client, IEntity vendor, Asda2ItemId itemId,
            int numItemsPurchased, int remainingAmount)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_BUY_ITEM, 20))
            {
                packet.Write((ulong) vendor.EntityId);
                packet.Write((uint) itemId);
                packet.Write(numItemsPurchased);
                packet.Write(remainingAmount);
                client.Send(packet, false);
            }
        }

        public static void HandleListTrainerSpells(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            NPC trainer = client.ActiveCharacter.Map.GetObject(id) as NPC;
            if (trainer == null)
                return;
            trainer.TalkToTrainer(client.ActiveCharacter);
        }

        public static void HandleBuyTrainerSpell(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            SpellId spellEntryId = (SpellId) packet.ReadUInt32();
            NPC trainer = client.ActiveCharacter.Map.GetObject(id) as NPC;
            if (trainer == null)
                return;
            trainer.BuySpell(client.ActiveCharacter, spellEntryId);
        }

        public static void SendTrainerBuyFailed(this NPC trainer, IRealmClient client, int serviceType,
            TrainerBuyError error)
        {
            using (RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_TRAINER_BUY_FAILED))
            {
                realmPacketOut.Write((ulong) trainer.EntityId);
                realmPacketOut.Write(serviceType);
                realmPacketOut.Write((int) error);
            }
        }

        public static void SendTrainerList(this NPC trainer, Character chr, IEnumerable<TrainerSpellEntry> spells,
            string msg)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TRAINER_LIST, 1156 + msg.Length + 1))
            {
                packet.Write((ulong) trainer.EntityId);
                packet.Write((uint) trainer.TrainerEntry.TrainerType);
                long position = packet.Position;
                packet.Position += 4L;
                int num = 0;
                foreach (TrainerSpellEntry spell1 in spells)
                {
                    if (spell1.Spell != null)
                    {
                        Spell spell2 = spell1.Spell;
                        if (spell2.IsTeachSpell)
                            spell2 = spell2.LearnSpell;
                        packet.Write(spell1.Spell.Id);
                        packet.Write((byte) spell1.GetTrainerSpellState(chr));
                        packet.Write(spell1.GetDiscountedCost(chr, trainer));
                        packet.Write(spell2.Talent != null ? 1U : 0U);
                        packet.Write(!spell1.Spell.IsProfession || !spell2.TeachesApprenticeAbility ? 0 : 1);
                        packet.Write((byte) spell1.RequiredLevel);
                        packet.Write((uint) spell1.RequiredSkillId);
                        packet.Write(spell1.RequiredSkillAmount);
                        packet.Write((uint) spell1.RequiredSpellId);
                        packet.Write(0U);
                        packet.Write(0U);
                        ++num;
                    }
                }

                packet.Write(msg);
                packet.Position = position;
                packet.Write(num);
                chr.Send(packet, false);
            }
        }

        public static void SendTrainerBuySucceeded(IPacketReceiver client, NPC trainer, TrainerSpellEntry spell)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TRAINER_BUY_SUCCEEDED, 12))
            {
                packet.Write((ulong) trainer.EntityId);
                packet.Write(spell.Spell.Id);
                client.Send(packet, false);
            }
        }

        public static void HandleBinderActivate(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            NPC innKeeper = client.ActiveCharacter.Map.GetObject(id) as NPC;
            if (innKeeper == null)
                return;
            client.ActiveCharacter.TryBindTo(innKeeper);
        }

        public static void SendBindConfirm(Character chr, WorldObject binder, ZoneId zone)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_BINDER_CONFIRM, 12))
            {
                packet.Write((ulong) binder.EntityId);
                packet.Write((uint) zone);
                chr.Client.Send(packet, false);
            }
        }

        public static void SendPlayerBound(Character chr, WorldObject binder, ZoneId zone)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PLAYERBOUND, 12))
            {
                packet.Write((ulong) binder.EntityId);
                packet.Write((uint) zone);
                chr.Client.Send(packet, false);
            }
        }
    }
}