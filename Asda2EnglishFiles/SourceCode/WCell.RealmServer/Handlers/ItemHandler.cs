using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class ItemHandler
    {
        public static void HandleItemNameQuery(IRealmClient client, RealmPacketIn packet)
        {
            uint index = packet.ReadUInt32();
            ItemTemplate itemTemplate = ItemMgr.Templates.Get<ItemTemplate>(index);
            if (itemTemplate == null)
                return;
            ItemHandler.SendItemNameQueryResponse((IPacketReceiver) client, itemTemplate);
        }

        public static void SendItemNameQueryResponse(IPacketReceiver client, ItemTemplate item)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ITEM_NAME_QUERY_RESPONSE,
                    4 + item.DefaultName.Length))
            {
                packet.WriteInt(item.Id);
                packet.WriteCString(item.DefaultName);
                client.Send(packet, false);
            }
        }

        /// <summary>Socket an item</summary>
        public static void HandleSocketGem(IRealmClient client, RealmPacketIn packet)
        {
            PlayerInventory inventory = client.ActiveCharacter.Inventory;
            if (inventory.CheckInteract() != InventoryError.OK)
                return;
            EntityId id1 = packet.ReadEntityId();
            Item obj = inventory.GetItem(id1);
            if (obj == null)
                return;
            Item[] gems = new Item[3];
            for (int index = 0; index < 3; ++index)
            {
                EntityId id2 = packet.ReadEntityId();
                if (id2 != EntityId.Zero)
                    gems[index] = inventory.GetItem(id2);
            }

            obj.ApplyGems<Item>(gems);
        }

        /// <summary>
        /// Sends the Item's PushResult (required after adding items).
        /// </summary>
        public static void SendItemPushResult(Character owner, Item item, ItemTemplate templ, int amount,
            ItemReceptionType reception)
        {
        }

        /// <summary>Send a simple "Can't do that right now" message</summary>
        /// <param name="client"></param>
        public static void SendCantDoRightNow(IRealmClient client)
        {
            ItemHandler.SendInventoryError((IPacketReceiver) client, (Item) null, (Item) null,
                InventoryError.CANT_DO_RIGHT_NOW);
        }

        /// <summary>
        /// item1 and item2 can be null, but item1 must be set in case of YOU_MUST_REACH_LEVEL_N.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <param name="error"></param>
        public static void SendInventoryError(IPacketReceiver client, Item item1, Item item2, InventoryError error)
        {
            using (RealmPacketOut packet = new RealmPacketOut(
                (PacketId) RealmServerOpCode.SMSG_INVENTORY_CHANGE_FAILURE,
                error == InventoryError.YOU_MUST_REACH_LEVEL_N ? 22 : 18))
            {
                packet.WriteByte((byte) error);
                if (item1 != null)
                    packet.Write(item1.EntityId.Full);
                else
                    packet.Write(0L);
                if (item2 != null)
                    packet.Write(item2.EntityId.Full);
                else
                    packet.Write(0L);
                packet.Write((byte) 0);
                if (error == InventoryError.YOU_MUST_REACH_LEVEL_N && item1 != null)
                    packet.WriteUInt(item1.Template.RequiredLevel);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// item1 and item2 can be null, but item1 must be set in case of YOU_MUST_REACH_LEVEL_N.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="error"></param>
        public static void SendInventoryError(IPacketReceiver client, InventoryError error)
        {
            using (RealmPacketOut packet = new RealmPacketOut(
                (PacketId) RealmServerOpCode.SMSG_INVENTORY_CHANGE_FAILURE,
                error == InventoryError.YOU_MUST_REACH_LEVEL_N ? 22 : 18))
            {
                packet.WriteByte((byte) error);
                packet.WriteULong(0);
                packet.WriteULong(0);
                packet.WriteByte(0);
                client.Send(packet, false);
            }
        }

        public static void SendDurabilityDamageDeath(IPacketReceiver client)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_DURABILITY_DAMAGE_DEATH, 0))
                client.Send(packet, false);
        }

        public static void SendEnchantLog(IPacketReceivingEntity owner, Asda2ItemId entryId, uint enchantId)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ENCHANTMENTLOG, 25))
            {
                packet.Write((ulong) owner.EntityId);
                packet.Write((ulong) owner.EntityId);
                packet.Write((uint) entryId);
                packet.Write(enchantId);
                packet.Write((byte) 0);
                owner.Send(packet, false);
            }
        }

        public static void SendEnchantTimeUpdate(IPacketReceivingEntity owner, Item item, int duration)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ITEM_ENCHANT_TIME_UPDATE, 24))
            {
                packet.Write((ulong) item.EntityId);
                packet.Write(item.Slot);
                packet.Write(duration);
                packet.Write((ulong) owner.EntityId);
                owner.Send(packet, false);
            }
        }

        public static void HandleItemSingleQuery(IRealmClient client, RealmPacketIn packet)
        {
            uint index = packet.ReadUInt32();
            ItemTemplate itemTemplate = ItemMgr.Templates.Get<ItemTemplate>(index);
            if (itemTemplate == null)
                return;
            ItemHandler.SendItemQueryResponse(client, itemTemplate);
        }

        public static void SendItemQueryResponse(IRealmClient client, ItemTemplate item)
        {
            ClientLocale locale = client.Info.Locale;
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ITEM_QUERY_SINGLE_RESPONSE, 630))
            {
                packet.Write(item.Id);
                packet.Write((uint) item.Class);
                packet.Write((uint) item.SubClass);
                packet.Write(item.Unk0);
                packet.WriteCString(item.Names.Localize(locale));
                packet.Write((byte) 0);
                packet.Write((byte) 0);
                packet.Write((byte) 0);
                packet.Write(item.DisplayId);
                packet.Write((uint) item.Quality);
                packet.Write((uint) item.Flags);
                packet.Write((uint) item.Flags2);
                packet.Write(item.BuyPrice);
                packet.Write(item.SellPrice);
                packet.Write((uint) item.InventorySlotType);
                packet.Write((uint) item.RequiredClassMask);
                packet.Write((uint) item.RequiredRaceMask);
                packet.Write(item.Level);
                packet.Write(item.RequiredLevel);
                packet.Write(item.RequiredSkill != null ? (int) item.RequiredSkill.Id : 0);
                packet.Write(item.RequiredSkillValue);
                packet.Write(item.RequiredProfession != null ? item.RequiredProfession.Id : 0U);
                packet.Write(item.RequiredPvPRank);
                packet.Write(item.UnknownRank);
                packet.Write(item.RequiredFaction != null ? (int) item.RequiredFaction.Id : 0);
                packet.Write((uint) item.RequiredFactionStanding);
                packet.Write(item.UniqueCount);
                packet.Write(item.MaxAmount);
                packet.Write(item.ContainerSlots);
                packet.Write(item.Mods.Length);
                for (int index = 0; index < item.Mods.Length; ++index)
                {
                    packet.Write((uint) item.Mods[index].Type);
                    packet.Write(item.Mods[index].Value);
                }

                packet.Write(item.ScalingStatDistributionId);
                packet.Write(item.ScalingStatValueFlags);
                for (int index = 0; index < 2; ++index)
                {
                    if (index >= item.Damages.Length)
                    {
                        packet.WriteFloat(0.0f);
                        packet.WriteFloat(0.0f);
                        packet.WriteUInt(0U);
                    }
                    else
                    {
                        DamageInfo damage = item.Damages[index];
                        packet.Write(damage.Minimum);
                        packet.Write(damage.Maximum);
                        packet.Write((uint) damage.School);
                    }
                }

                for (int index = 0; index < 7; ++index)
                {
                    int resistance = item.Resistances[index];
                    packet.Write(resistance);
                }

                packet.Write(item.AttackTime);
                packet.Write((uint) item.ProjectileType);
                packet.Write(item.RangeModifier);
                for (int index = 0; index < 5; ++index)
                {
                    ItemSpell spell;
                    if (index < item.Spells.Length && (spell = item.Spells[index]) != null)
                    {
                        packet.Write((uint) spell.Id);
                        packet.Write((uint) spell.Trigger);
                        packet.Write((uint) Math.Abs(spell.Charges));
                        packet.Write(spell.Cooldown);
                        packet.Write(spell.CategoryId);
                        packet.Write(spell.CategoryCooldown);
                    }
                    else
                    {
                        packet.WriteUInt(0U);
                        packet.WriteUInt(0U);
                        packet.WriteUInt(0U);
                        packet.Write(-1);
                        packet.WriteUInt(0U);
                        packet.Write(-1);
                    }
                }

                packet.Write((uint) item.BondType);
                packet.WriteCString(item.Descriptions.Localize(locale));
                packet.Write(item.PageTextId);
                packet.Write((uint) item.LanguageId);
                packet.Write((uint) item.PageMaterial);
                packet.Write(item.QuestId);
                packet.Write(item.LockId);
                packet.Write((int) item.Material);
                packet.Write((uint) item.SheathType);
                packet.Write(item.RandomPropertiesId);
                packet.Write(item.RandomSuffixId);
                packet.Write(item.BlockValue);
                packet.Write((uint) item.SetId);
                packet.Write(item.MaxDurability);
                packet.Write((uint) item.ZoneId);
                packet.Write((uint) item.MapId);
                packet.Write((uint) item.BagFamily);
                packet.Write((uint) item.ToolCategory);
                for (int index = 0; index < 3; ++index)
                {
                    packet.Write((uint) item.Sockets[index].Color);
                    packet.Write(item.Sockets[index].Content);
                }

                packet.Write(item.SocketBonusEnchantId);
                packet.Write(item.GemPropertiesId);
                packet.Write(item.RequiredDisenchantingLevel);
                packet.Write(item.ArmorModifier);
                packet.Write(item.Duration);
                packet.Write(item.ItemLimitCategoryId);
                packet.Write(item.HolidayId);
                client.Send(packet, false);
            }
        }

        public static void SendEquipmentSetList(IPacketReceiver client, IList<EquipmentSet> setList)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_EQUIPMENT_SET_LIST))
            {
                packet.Write(setList.Count);
                foreach (EquipmentSet set in (IEnumerable<EquipmentSet>) setList)
                {
                    set.SetGuid.WritePacked((BinaryWriter) packet);
                    packet.Write(set.Id);
                    packet.Write(set.Name);
                    packet.Write(set.Icon);
                    IList<EquipmentSetItemMapping> equipmentSetItemMappingList =
                        set.Items ?? (IList<EquipmentSetItemMapping>) new EquipmentSetItemMapping[19];
                    for (int index = 0; index < 19; ++index)
                    {
                        EquipmentSetItemMapping equipmentSetItemMapping = equipmentSetItemMappingList[index];
                        if (equipmentSetItemMapping != null)
                            equipmentSetItemMapping.ItemEntityId.WritePacked((BinaryWriter) packet);
                        else
                            EntityId.Zero.WritePacked((BinaryWriter) packet);
                    }
                }

                client.Send(packet, false);
            }
        }

        public static void SendEquipmentSetSaved(IPacketReceiver client, EquipmentSet set)
        {
            if (set == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_EQUIPMENT_SET_SAVED))
            {
                packet.Write(set.Id);
                packet.Write((ulong) set.SetGuid);
                client.Send(packet, false);
            }
        }

        public static void SendUseEquipmentSetResult(IPacketReceiver client, UseEquipmentSetError error)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_USE_EQUIPMENT_SET_RESULT))
            {
                packet.Write((byte) error);
                client.Send(packet, false);
            }
        }
    }
}