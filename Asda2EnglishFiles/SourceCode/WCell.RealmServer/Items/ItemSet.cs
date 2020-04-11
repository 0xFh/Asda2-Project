using Cell.Core;
using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Items
{
    /// <summary>
    /// Represents a collectable Set of Items
    /// 
    /// TODO: ID - enum
    /// </summary>
    public class ItemSet
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>The bag to be used when creating a new set</summary>
        private const Asda2ItemId BagTemplateId = Asda2ItemId.End;

        /// <summary>Maximum amount of items per set</summary>
        private const int MaxBonusCount = 8;

        /// <summary>offset of items in the DBC file</summary>
        private const int ItemsOffset = 18;

        /// <summary>End of items in the DBC file</summary>
        private const int ItemsEnd = 34;

        /// <summary>Offset of Set-Boni in the DBC file</summary>
        private const int BoniOffset = 35;

        /// <summary>Offset of the item-order in the DBC file</summary>
        private const int BoniOrderOffset = 43;

        public uint Id;
        public string Name;

        /// <summary>The templates of items that belong to this set</summary>
        public ItemTemplate[] Templates;

        /// <summary>
        /// An array of array of spells that get applied for each (amount-1) of items of this set.
        /// Eg. all spells to be applied when equipping the first item would be at Boni[0] etc
        /// </summary>
        public Spell[][] Boni;

        /// <summary>We need this skill in order to wear items of this set</summary>
        public SkillLine RequiredSkill;

        /// <summary>
        /// We need at least this much of the RequiredSkill, in order to wear items of this set
        /// </summary>
        public uint RequiredSkillValue;

        /// <summary>
        /// If there is a free equippable bag slot: Adds all items of this set to a new bag in that slot
        /// </summary>
        /// <returns>False if their was no space left or an internal error occured and not all items could be added</returns>
        public static bool CreateSet(Character owner, ItemSetId id)
        {
            if ((long) id >= (long) ItemMgr.Sets.Length)
                return false;
            ItemSet set = ItemMgr.Sets[(uint) id];
            if (set == null)
                return false;
            return set.Create(owner);
        }

        /// <summary>
        /// If there is a free equippable bag slot: Adds all items of this set to a new bag in that slot
        /// </summary>
        /// <returns>False if their was no space left or an internal error occured and not all items could be added</returns>
        public bool Create(Character owner)
        {
            EquippedContainerInventory equippedContainers = owner.Inventory.EquippedContainers;
            int freeSlot = equippedContainers.FindFreeSlot();
            if (freeSlot != (int) byte.MaxValue)
            {
                Container container = Item.CreateItem(Asda2ItemId.End, owner, 1) as Container;
                if (container == null)
                {
                    ItemSet.log.Error("Invalid container template id for ItemSet: " + (object) Asda2ItemId.End);
                }
                else
                {
                    InventoryError inventoryError =
                        equippedContainers.TryAdd(freeSlot, (Item) container, true, ItemReceptionType.Receive);
                    if (inventoryError == InventoryError.OK)
                    {
                        BaseInventory baseInventory = container.BaseInventory;
                        for (int index = 0; index < this.Templates.Length; ++index)
                        {
                            int amount = 1;
                            InventoryError error = baseInventory.TryAdd(this.Templates[index], ref amount,
                                ItemReceptionType.Receive);
                            if (error != InventoryError.OK)
                            {
                                ItemHandler.SendInventoryError((IPacketReceiver) owner.Client, (Item) null, (Item) null,
                                    error);
                                ItemSet.log.Error("Failed to add item (Template: {0}) to bag on {1} ({2})",
                                    (object) this.Templates[index], (object) owner, (object) error);
                                return false;
                            }
                        }

                        return true;
                    }

                    ItemSet.log.Error("Failed to add ItemSet-bag to owner {0} ({1})", (object) owner,
                        (object) inventoryError);
                    return false;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return this.Name + " (Id: " + (object) this.Id + ")";
        }

        public class ItemSetDBCConverter : AdvancedDBCRecordConverter<ItemSet>
        {
            public override ItemSet ConvertTo(byte[] rawData, ref int id)
            {
                ItemSet itemSet = new ItemSet();
                id = (int) (itemSet.Id = rawData.GetUInt32(0U));
                itemSet.Name = this.GetString(rawData, 1);
                itemSet.Templates = new ItemTemplate[10];
                Spell[] spellArray = new Spell[8];
                for (uint field = 35; field < 43U; ++field)
                {
                    uint uint32 = rawData.GetUInt32(field);
                    Spell spell;
                    if (uint32 != 0U && (spell = SpellHandler.Get(uint32)) != null)
                    {
                        uint num = field - 35U;
                        spellArray[num] = spell;
                    }
                }

                List<Spell>[] spellListArray = new List<Spell>[8];
                uint num1 = 0;
                for (uint field = 43; field < 51U; ++field)
                {
                    uint uint32 = rawData.GetUInt32(field);
                    if (uint32 > 0U)
                    {
                        uint num2 = uint32 - 1U;
                        if (num1 < num2)
                            num1 = num2;
                        List<Spell> spellList = spellListArray[num2];
                        if (spellList == null)
                            spellListArray[num2] = spellList = new List<Spell>(3);
                        uint num3 = field - 43U;
                        Spell spell = spellArray[num3];
                        if (spell != null)
                            spellList.Add(spell);
                    }
                }

                itemSet.Boni = new Spell[num1 + 1U][];
                for (int index = 0; (long) index <= (long) num1; ++index)
                {
                    if (spellListArray[index] != null)
                        itemSet.Boni[index] = spellListArray[index].ToArray();
                }

                SkillId uint32_1 = (SkillId) rawData.GetUInt32(51U);
                if (uint32_1 > SkillId.None)
                {
                    SkillLine skillLine = SkillHandler.Get(uint32_1);
                    if (skillLine != null)
                    {
                        itemSet.RequiredSkill = skillLine;
                        itemSet.RequiredSkillValue = rawData.GetUInt32(52U);
                    }
                }

                return itemSet;
            }
        }
    }
}