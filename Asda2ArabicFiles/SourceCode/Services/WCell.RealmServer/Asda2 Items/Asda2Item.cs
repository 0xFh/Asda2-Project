using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
    public partial class Asda2Item : ObjectBase, IOwned, IAsda2Weapon, INamed, ILockable, IQuestHolder, IAsda2MountableItem, IContextHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Item);

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return UpdateFieldInfos; }
        }

        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Item; }
        }

        public static readonly Item PlaceHolder = new Item();

        protected Asda2ItemTemplate m_template;
        protected internal bool m_isInWorld;

        /// <summary>
        /// Items are unknown when a creation update
        /// has not been sent to the Owner yet.
        /// </summary>
        internal bool m_unknown;

        protected internal Character m_owner;
        //protected ItemEnchantment[] m_enchantments;
        protected IProcHandler m_hitProc;
        protected Asda2ItemRecord m_record;

        #region CreateItem
        public static Asda2Item CreateItem(int templateId, Character owner, int amount, int Enchant = 0, Asda2Item item = null)
        {
            var template = Asda2ItemMgr.GetTemplate(templateId);
            template.Enchant = Enchant;
            if (template != null)
            {
                if (item == null)
                    return CreateItem(template, owner, amount);
                else
                    return CreateItem(template, owner, amount, item);
            }
            return null;
        }

        public static Asda2Item CreateItem(Asda2ItemId templateId, Character owner, int amount)
        {
            var template = Asda2ItemMgr.GetTemplate(templateId);
            if (template != null)
            {
                return CreateItem(template, owner, amount);
            }
            return null;
        }

        public static Asda2Item CreateItem(Asda2ItemTemplate template, Character owner, int amount, Asda2Item item = null)
        {
            var createdItem = template.Create();
            if (item == null)
                createdItem.InitItem(template, owner, amount);
            else
                createdItem.InitItem(template, owner, amount, item);
            return createdItem;
        }

        public static Asda2Item CreateItem(Asda2ItemRecord record, Character owner)
        {
            var template = record.Template;
            if (template == null)
            {
                log.Warn("{0} had an ItemRecord with invalid ItemId: {1}", owner, record);
                return null;
            }

            var item = template.Create();
            item.LoadItem(record, owner, template);
            return item;
        }

        public static Asda2Item CreateItem(Asda2ItemRecord record, Character owner, Asda2ItemTemplate template)
        {
            var item = template.Create();
            item.LoadItem(record, owner, template);
            return item;
        }

        public static Asda2Item CreateItem(Asda2ItemRecord record, Asda2ItemTemplate template)
        {
            var item = template.Create();
            item.LoadItem(record, template);
            return item;
        }
        #endregion

        protected internal Asda2Item()
        {
        }

        #region Init & Load
        /// <summary>
        /// Initializes a new Item
        /// </summary>
        internal void InitItem(Asda2ItemTemplate template, Character owner, int amount, Asda2Item item = null)
        {
            //if (item != null)
            //{
            // Parametr1Type = item.Parametr1Type;
            // Parametr1Value = item.Parametr1Value;
            // Parametr2Type = item.Parametr2Type;
            // Parametr2Value = item.Parametr2Value;
            // Parametr3Type = item.Parametr3Type;
            //  Parametr3Value = item.Parametr3Value;
            // Parametr4Type = item.Parametr4Type;
            //Parametr4Value = item.Parametr4Value;
            // Parametr5Type = item.Parametr5Type;
            // Parametr5Value = item.Parametr5Value;
            //   Enchant = item.Enchant;
            //}

            m_record = Asda2ItemRecord.CreateRecord(template);

            Type |= ObjectTypes.Item;

            m_template = template;
            Durability = m_template.MaxDurability;
            MaxDurability = m_template.MaxDurability;
            Amount = amount;
            OwningCharacter = owner;
            ItemId = (int)template.ItemId;
            EntityId = new EntityId((uint)m_record.Guid, HighId.Item);
            if (item != null)
            {
                Enchant = (byte)template.Enchant;
                Parametr1Type = item.Parametr1Type;
                Parametr1Value = item.Parametr1Value;
                Parametr2Type = item.Parametr2Type;
                Parametr2Value = item.Parametr2Value;
                Parametr3Type = item.Parametr3Type;
                Parametr3Value = item.Parametr3Value;
                Parametr4Type = item.Parametr4Type;
                Parametr4Value = item.Parametr4Value;
                Parametr5Type = item.Parametr5Type;
                Parametr5Value = item.Parametr5Value;
            }
            else
            {
                GenerateNewOptions();
                RecalculateItemParametrs();
                template.NotifyCreated(m_record);
            }

            //   
            //  RecalculateItemParametrs();
            template.NotifyCreated(m_record);
            OnInit();
        }

        /// <summary>
        /// Loads an already created item
        /// </summary>
        internal void LoadItem(Asda2ItemRecord record, Character owner, Asda2ItemTemplate template)
        {
            m_record = record;
            OwningCharacter = owner;

            LoadItem(record, template);
        }

        /// <summary>
        /// Loads an already created item without owner
        /// </summary>
        /// <param name="record"></param>
        /// <param name="template"></param>
        internal void LoadItem(Asda2ItemRecord record, Asda2ItemTemplate template)
        {
            m_record = record;
            //EntityId = record.EntityId;

            m_template = template;
            EntryId = m_template.Id;
            ItemId = (int)template.ItemId;
            Type |= ObjectTypes.Item;
            _slot = record.Slot;
            _inventoryType = record.InventoryType;
            //SetUInt32(ItemFields.FLAGS, (uint)record.Flags);
            SetInt32(ItemFields.DURABILITY, record.Durability);
            SetInt32(ItemFields.DURATION, record.Duration);
            SetInt32(ItemFields.STACK_COUNT, record.Amount);
            //SetInt32(ItemFields.PROPERTY_SEED, record.RandomSuffix);
            //SetInt32(ItemFields.RANDOM_PROPERTIES_ID, record.RandomProperty);
            //SetInt64(ItemFields.CREATOR, record.CreatorEntityId);
            //SetInt64(ItemFields.GIFTCREATOR, record.GiftCreatorEntityId);

            //ItemText = record.ItemText;

            /*if (m_template.UseSpell != null)
            {
                SetSpellCharges(m_template.UseSpell.Index, (int)record.Charges);
            }*/
            MaxDurability = m_template.MaxDurability;

            // add enchants
            /*if (record.EnchantIds != null)
            {
                for (var enchSlot = 0; enchSlot < record.EnchantIds.Length; enchSlot++)
                {
                    var enchant = record.EnchantIds[enchSlot];
                    if (enchSlot == (int)EnchantSlot.Temporary)
                    {
                        ApplyEnchant(enchant, (EnchantSlot)enchSlot, record.EnchantTempTime, 0, false);
                    }
                    else
                    {
                        ApplyEnchant(enchant, (EnchantSlot)enchSlot, 0, 0, false);
                    }
                }
                //item.CheckSocketColors();
            }*/
            RecalculateItemParametrs();
            OnLoad();
        }

        /// <summary>
        /// Called after initializing a newly created Item (Owner might be null)
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// Called after loading an Item (Owner might be null)
        /// </summary>
        protected virtual void OnLoad()
        {

        }
        #endregion

        #region Properties
        public Asda2ItemTemplate Template
        {
            get { return m_template; }
        }

        public LockEntry Lock
        {
            get
            {
                return m_template.Lock;
            }
        }

        public override bool IsInWorld
        {
            get { return m_isInWorld; }
        }

        /// <summary>
        /// Whether this object has already been deleted.
        /// </summary>
        public bool IsDeleted
        {
            get;
            internal set;
        }

        /// <summary>
        /// Checks whether this Item can currently be used
        /// </summary>
        public bool CanBeUsed
        {
            get { return (MaxDurability == 0 || Durability > 0) && m_loot == null; }
        }

        /// <summary>
        /// The name of this item
        /// </summary>
        public string Name
        {
            get
            {
                if (m_template != null)
                {
                    return m_template.Name;
                }
                return "";
            }
        }

        public bool CanBeTraded
        {
            get
            {
                /*if (Owner != null && Owner is Character)
                {
                    var chr = (Character)Owner;
                    if (chr.IsLooting && chr.Loot.Lootable.MonstrId == EntityId)
                    {
                        // item is currently being looted (eg a strongbox etc)
                        return false;
                    }
                }*/
                return m_template.MaxDurability == 0 || Durability > 0;
            }
        }
        /// <summary>
        /// See IUsable.Owner
        /// </summary>
        public Unit Owner
        {
            get { return m_owner; }
        }

        /// <summary>
        /// Whether this Item is currently equipped.
        /// </summary>
        public bool IsEquipped
        {
            get { return InventoryType == Asda2InventoryType.Equipment; }
        }

        Asda2InventoryError IAsda2MountableItem.CheckEquip(Character owner)
        {
            return Template.CheckEquip(owner);
        }


        /// <summary>
        /// Whether this Item is currently equipped and is not a kind of container.
        /// </summary>
        public bool IsEquippedItem
        {
            get
            {
                return InventoryType == Asda2InventoryType.Equipment;
            }
        }

        /// <summary>
        /// Wheter this item's bonuses are applied
        /// </summary>
        public bool IsApplied
        {
            get;
            private set;
        }


        #endregion

        /// <summary>
        /// Called when this Item was added to someone's inventory
        /// </summary>
        protected internal void OnAdd()
        {
            if (m_template.BondType == ItemBondType.OnPickup ||
                m_template.BondType == ItemBondType.Quest)
            {
                //Flags |= ItemFlags.Soulbound;
                IsSoulbound = true;
            }
        }

        /// <summary>
        /// Saves all recent changes that were made to this Item to the DB
        /// </summary>
        public void Save()
        {
            if (IsDeleted)
            {
                LogUtil.ErrorException(new InvalidOperationException("Trying to save deleted Item: " + this));
                return;
            }

            try
            {
                m_record.SaveAndFlush();
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, string.Format("failed to save item, item {0} acc {1}[{2}]", Name, OwningCharacter == null ? "null" : OwningCharacter.Name, OwningCharacter == null ? 999 : OwningCharacter.AccId));
            }
        }

        /// <summary>
        /// Subtracts the given amount from this item and creates a new item, with that amount.
        /// WARNING: Make sure that this item is belonging to someone and that amount is valid!
        /// </summary>
        /// <param name="amount">The amount of the newly created item</param>
        public Asda2Item Split(int amount)
        {
            Amount -= amount;
            return CreateItem(m_template, OwningCharacter, amount);
        }


        /// <summary>
        /// TODO: Random properties
        /// </summary>
        public bool CanStackWith(Asda2Item otherItem)
        {
            return m_template.IsStackable && m_template == otherItem.m_template;
        }

        /// <summary>
        /// A chest was looted empty
        /// </summary>
        public override void OnFinishedLooting()
        {
            Destroy();
        }

        public override uint GetLootId(Asda2LootEntryType type)
        {
            return m_template.Id;
        }


        #region Equipping


        public static int AvatarLvlDefenceBonusDivider = 10;
        public static int ArmorLvlDefenceBonusDivider = 10;
        public static int AcceoryLvlDefenceBonusDivider = 10;
        public static int WeaponLvlDefenceBonusDivider = 10;
        /// <summary>
        /// Called when this Item gets equipped.
        /// Requires map context.
        /// </summary>
        public void OnEquip()
        {
            if (IsApplied) return;
            IsApplied = true;

            RecalculateItemParametrs();
            var slot = (Asda2EquipmentSlots)Slot;
            var chr = OwningCharacter;
            if (Soul1Id != 0)
                ProcessAddSoul(Soul1Id);
            if (Soul2Id != 0)
                ProcessAddSoul(Soul2Id);
            if (Soul3Id != 0)
                ProcessAddSoul(Soul3Id);
            if (Soul4Id != 0)
                ProcessAddSoul(Soul4Id);
            if (Parametr1Type != 0)
                ModifyStat(Parametr1Type, Parametr1Value);
            if (Parametr2Type != 0)
                ModifyStat(Parametr2Type, Parametr2Value);
            if (Parametr3Type != 0)
                ModifyStat(Parametr3Type, Parametr3Value);
            if (Parametr4Type != 0)
                ModifyStat(Parametr4Type, Parametr4Value);
            if (Parametr5Type != 0)
                ModifyStat(Parametr5Type, Parametr5Value);
            if (Category == Asda2ItemCategory.RodFishingSkill)
                OwningCharacter.ApplyStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
            else if (Category == Asda2ItemCategory.RodGauge)
                OwningCharacter.ApplyStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
            else if (Category == Asda2ItemCategory.RodFishingSkillAndGauge)
            {
                OwningCharacter.ApplyStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
                OwningCharacter.ApplyStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.NacklessMDef || Category == Asda2ItemCategory.RingMDef)
            {
                OwningCharacter.ApplyStatMod(ItemModType.Asda2MagicDefence, (int)(Template.ValueOnUse * CharacterFormulas.ItemsMagicDeffenceMultiplier));
            }
            else if (Category == Asda2ItemCategory.NacklessCriticalChance)
            {
                OwningCharacter.ApplyStatMod(ItemModType.CriticalStrikeRating, Template.ValueOnUse);
                OwningCharacter.ApplyStatMod(ItemModType.SpellCriticalStrikeRating, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.NacklessHealth)
            {
                OwningCharacter.ApplyStatMod(ItemModType.Health, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.NacklessMana)
            {
                OwningCharacter.ApplyStatMod(ItemModType.Power, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.RingMaxDef)
            {
                OwningCharacter.ApplyStatMod(ItemModType.Asda2Defence, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.RingMaxMAtack)
            {
                OwningCharacter.ApplyStatMod(ItemModType.MagicDamage, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.RingMaxAtack)
            {
                OwningCharacter.ApplyStatMod(ItemModType.Damage, Template.ValueOnUse);
            }
            // binding
            IsSoulbound = true;

            if (m_template.EquipmentSlot == Asda2EquipmentSlots.Shild)
            {
                chr.UpdateBlockChance();
            }


            #region set items bonuses
            var setRec = SetItemManager.GetSetItemRecord(ItemId);
            if (setRec != null)
            {
                if (!OwningCharacter.AppliedSets.ContainsKey(setRec.Id))
                    OwningCharacter.AppliedSets.Add(setRec.Id, 1);
                else
                {
                    OwningCharacter.AppliedSets[setRec.Id]++;
                }
                AddSetBonus(setRec.GetBonus(OwningCharacter.AppliedSets[setRec.Id]));
            }
            #endregion

            Asda2CharacterHandler.SendUpdateStatsOneResponse(OwningCharacter.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(OwningCharacter.Client);
            m_template.NotifyEquip(this);

        }

        /// <summary>
        /// Called when this Item gets unequipped.
        /// Requires map context.
        /// </summary>
        public void OnUnEquip()
        {
            if (!IsApplied) return;
            IsApplied = false;

            if (Soul1Id != 0)
                ProcessRemoveSoul(Soul1Id);
            if (Soul2Id != 0)
                ProcessRemoveSoul(Soul2Id);
            if (Soul3Id != 0)
                ProcessRemoveSoul(Soul3Id);
            if (Soul4Id != 0)
                ProcessRemoveSoul(Soul4Id);
            if (Parametr1Type != 0)
                ModifyStat(Parametr1Type, -Parametr1Value);
            if (Parametr2Type != 0)
                ModifyStat(Parametr2Type, -Parametr2Value);
            if (Parametr3Type != 0)
                ModifyStat(Parametr3Type, -Parametr3Value);
            if (Parametr4Type != 0)
                ModifyStat(Parametr4Type, -Parametr4Value);
            if (Parametr5Type != 0)
                ModifyStat(Parametr5Type, -Parametr5Value);
            if (Category == Asda2ItemCategory.RodFishingSkill)
                OwningCharacter.RemoveStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
            else if (Category == Asda2ItemCategory.RodGauge)
                OwningCharacter.RemoveStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
            else if (Category == Asda2ItemCategory.RodFishingSkillAndGauge)
            {
                OwningCharacter.RemoveStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
                OwningCharacter.RemoveStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.NacklessMDef || Category == Asda2ItemCategory.RingMDef)
            {
                OwningCharacter.RemoveStatMod(ItemModType.Asda2MagicDefence, (int)(Template.ValueOnUse * CharacterFormulas.ItemsMagicDeffenceMultiplier));
            }
            else if (Category == Asda2ItemCategory.NacklessCriticalChance)
            {
                OwningCharacter.RemoveStatMod(ItemModType.CriticalStrikeRating, Template.ValueOnUse);
                OwningCharacter.RemoveStatMod(ItemModType.SpellCriticalStrikeRating, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.NacklessHealth)
            {
                OwningCharacter.RemoveStatMod(ItemModType.Health, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.NacklessMana)
            {
                OwningCharacter.RemoveStatMod(ItemModType.Power, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.RingMaxDef)
            {
                OwningCharacter.RemoveStatMod(ItemModType.Asda2Defence, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.RingMaxMAtack)
            {
                OwningCharacter.RemoveStatMod(ItemModType.MagicDamage, Template.ValueOnUse);
            }
            else if (Category == Asda2ItemCategory.RingMaxAtack)
            {
                OwningCharacter.RemoveStatMod(ItemModType.Damage, Template.ValueOnUse);
            }
            if (m_template.EquipmentSlot == Asda2EquipmentSlots.Shild)
            {
                m_owner.UpdateBlockChance();
            }

            #region set items bonuses
            var setRec = SetItemManager.GetSetItemRecord(ItemId);
            if (setRec != null)
            {
                if (OwningCharacter.AppliedSets.ContainsKey(setRec.Id))
                {
                    RemoveSetBonus(setRec.GetBonus(OwningCharacter.AppliedSets[setRec.Id]));
                    OwningCharacter.AppliedSets[setRec.Id]--;
                }
            }
            #endregion
            if (m_hitProc != null)
            {
                m_owner.RemoveProcHandler(m_hitProc);
                m_hitProc = null;
            }


            m_template.NotifyUnequip(this);

            RecalculateItemParametrs();
            Asda2CharacterHandler.SendUpdateStatsOneResponse(OwningCharacter.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(OwningCharacter.Client);
        }
        private void AddSetBonus(Asda2SetBonus bonus)
        {
            if (bonus == null) return;
            ModifyStat((Asda2ItemBonusType)bonus.Type, bonus.Value);
        }

        private void RemoveSetBonus(Asda2SetBonus bonus)
        {
            if (bonus == null) return;
            ModifyStat((Asda2ItemBonusType)bonus.Type, -bonus.Value);
        }

        private void ProcessAddSoul(int sowelId)
        {
            var sowel = Asda2ItemMgr.GetTemplate(sowelId);
            if (sowel.SowelBonusType == ItemBonusType.WeaponAtack || sowel.SowelBonusType == ItemBonusType.WaponMAtack)
                return;
            ModifyStatBySowel(sowel.SowelBonusType, sowel.SowelBonusValue);
        }
        private void ModifyStat(Asda2ItemBonusType type, int value)
        {
            value = (int)(value * CharacterFormulas.CalculateEnchantMultiplier(Enchant));
            switch (type)
            {
                case Asda2ItemBonusType.None:
                    break;
                case Asda2ItemBonusType.MaxAtack:
                    OwningCharacter.ChangeModifier(StatModifierInt.Damage, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MaxMAtak:
                    OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MaxDef:
                    OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.NormalAtackCrit:
                    OwningCharacter.ChangeModifier(StatModifierInt.CritChance, value);
                    break;
                case Asda2ItemBonusType.MaxHp:
                    OwningCharacter.ChangeModifier(StatModifierInt.Health, value);
                    break;
                case Asda2ItemBonusType.MaxMp:
                    OwningCharacter.ChangeModifier(StatModifierInt.Power, value);
                    break;
                case Asda2ItemBonusType.HpPotionRecovery:
                    OwningCharacter.ChangeModifier(StatModifierInt.Health, value);
                    break;
                case Asda2ItemBonusType.MpPotionRecovery:
                    OwningCharacter.ChangeModifier(StatModifierInt.Power, value);
                    break;
                case Asda2ItemBonusType.RecoverBadCondition:
                    OwningCharacter.ChangeModifier(StatModifierInt.Health, value);
                    break;
                case Asda2ItemBonusType.HpRegeneration:
                    OwningCharacter.ChangeModifier(StatModifierInt.HealthRegen, value);
                    break;
                case Asda2ItemBonusType.MpRegeneration:
                    OwningCharacter.ChangeModifier(StatModifierInt.PowerRegen, value);
                    break;
                case Asda2ItemBonusType.FireAttribute:
                    OwningCharacter.ChangeModifier(StatModifierFloat.FireAttribute, (float)value / 100);
                    break;
                case Asda2ItemBonusType.WaterAttribue:
                    OwningCharacter.ChangeModifier(StatModifierFloat.WaterAttribute, (float)value / 100);
                    break;
                case Asda2ItemBonusType.EarthAttribute:
                    OwningCharacter.ChangeModifier(StatModifierFloat.EarthAttribute, (float)value / 100);
                    break;
                case Asda2ItemBonusType.ClimateAtribute:
                    OwningCharacter.ChangeModifier(StatModifierFloat.ClimateAttribute, (float)value / 100);
                    break;
                case Asda2ItemBonusType.LightAttribute:
                    OwningCharacter.ChangeModifier(StatModifierFloat.LightAttribute, (float)value / 100);
                    break;
                case Asda2ItemBonusType.DarkAttribute:
                    OwningCharacter.ChangeModifier(StatModifierFloat.DarkAttribute, (float)value / 100);
                    break;
                case Asda2ItemBonusType.FireResistance:
                    OwningCharacter.ChangeModifier(StatModifierFloat.FireResist, (float)value / 100);
                    break;
                case Asda2ItemBonusType.WaterResistance:
                    OwningCharacter.ChangeModifier(StatModifierFloat.WaterResist, (float)value / 100);
                    break;
                case Asda2ItemBonusType.EarthResistance:
                    OwningCharacter.ChangeModifier(StatModifierFloat.EarthResit, (float)value / 100);
                    break;
                case Asda2ItemBonusType.ClimateResistance:
                    OwningCharacter.ChangeModifier(StatModifierFloat.ClimateResist, (float)value / 100);
                    break;
                case Asda2ItemBonusType.LightResistance:
                    OwningCharacter.ChangeModifier(StatModifierFloat.LightResist, (float)value / 100);
                    break;
                case Asda2ItemBonusType.DarkResistance:
                    OwningCharacter.ChangeModifier(StatModifierFloat.DarkResit, (float)value / 100);
                    break;
                case Asda2ItemBonusType.CraftingChance:
                    OwningCharacter.ChangeModifier(StatModifierFloat.CraftingChance, (float)value / 100);
                    break;
                case Asda2ItemBonusType.OhsSkillDamage:
                    if (OwningCharacter.Archetype.ClassId == ClassId.OHS)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.SpearSkillDamage:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Spear)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.ThsSkillDamage:
                    if (OwningCharacter.Archetype.ClassId == ClassId.THS)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.CrossbowSkillDamage:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Crossbow)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.BowSkillDamage:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Bow)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.BalistaSkillDamage:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Balista)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.StaffSkillDamage:
                    if (OwningCharacter.Archetype.ClassId == ClassId.AtackMage || OwningCharacter.Archetype.ClassId == ClassId.HealMage || OwningCharacter.Archetype.ClassId == ClassId.SupportMage)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.ProtectorSkillDamage:
                    break;
                case Asda2ItemBonusType.KuckleSkillDamage:
                    break;
                case Asda2ItemBonusType.ClawSkillDamage:
                    break;
                case Asda2ItemBonusType.BackpackSkillDamage:
                    break;
                case Asda2ItemBonusType.TwoHandDaggerSkillDamage:
                    break;
                case Asda2ItemBonusType.DualWeildSkillDamage:
                    break;
                case Asda2ItemBonusType.CristalBeadSkillDamage:
                    break;
                case Asda2ItemBonusType.OhsSkillCrit:
                    if (OwningCharacter.Archetype.ClassId == ClassId.OHS)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.SpearSkillCrit:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Spear)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.ThsSkillCrit:
                    if (OwningCharacter.Archetype.ClassId == ClassId.THS)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.CrossbowSkillCrit:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Crossbow)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.BowSkillCrit:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Bow)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.BalistaSkillCrit:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Balista)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.StaffSkillCrit:
                    if (OwningCharacter.Archetype.ClassId == ClassId.AtackMage || OwningCharacter.Archetype.ClassId == ClassId.HealMage || OwningCharacter.Archetype.ClassId == ClassId.SupportMage)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.ProtectorSkillCrit:
                    break;
                case Asda2ItemBonusType.KuckleSkillCrit:
                    break;
                case Asda2ItemBonusType.ClawSkillCrit:
                    break;
                case Asda2ItemBonusType.BackpackSkillCrit:
                    break;
                case Asda2ItemBonusType.TwoHandDaggerSkillCrit:
                    break;
                case Asda2ItemBonusType.DualWeildSkillCrit:
                    break;
                case Asda2ItemBonusType.CristalBeadSkillCrit:
                    break;
                case Asda2ItemBonusType.HuntingExp:
                    OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float)value / 100);
                    break;
                case Asda2ItemBonusType.HuntingExpMinus:
                    OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, -(float)value / 100);
                    break;
                case Asda2ItemBonusType.QuestExp:
                    OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float)value / 100);
                    break;
                case Asda2ItemBonusType.QuestExpMinus:
                    OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, -(float)value / 100);
                    break;
                case Asda2ItemBonusType.SkillRange:
                    OwningCharacter.ChangeModifier(StatModifierInt.SpellRange, value);
                    break;
                case Asda2ItemBonusType.CraftingLevelPlus:
                    break;
                case Asda2ItemBonusType.CraftingLevelMinus:
                    break;
                case Asda2ItemBonusType.RecoveryAmount:
                    OwningCharacter.ChangeModifier(StatModifierInt.HealthRegen, value);
                    break;
                case Asda2ItemBonusType.DropRate:
                    OwningCharacter.ChangeModifier(StatModifierFloat.Asda2DropChance, (float)value / 100);
                    break;
                case Asda2ItemBonusType.ExpItem:
                    OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float)value / 100);
                    break;
                case Asda2ItemBonusType.EmptySlot:
                    break;
                case Asda2ItemBonusType.MinAtack:
                    OwningCharacter.ChangeModifier(StatModifierInt.Damage, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MinMAtack:
                    OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MinDef:
                    OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.Atack:
                    OwningCharacter.ChangeModifier(StatModifierInt.Damage, value);
                    break;
                case Asda2ItemBonusType.MAtack:
                    OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage, value);
                    break;
                case Asda2ItemBonusType.Deffence:
                    OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence, value);
                    break;
                case Asda2ItemBonusType.DodgePrc:
                    OwningCharacter.ChangeModifier(StatModifierInt.DodgeChance, value);
                    break;
                case Asda2ItemBonusType.MinBlockRatePrc:
                    OwningCharacter.ChangeModifier(StatModifierInt.BlockChance, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MaxBlockRatePrc:
                    OwningCharacter.ChangeModifier(StatModifierInt.BlockChance, (int)(value * CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.BlockRatePrc:
                    OwningCharacter.ChangeModifier(StatModifierInt.BlockChance, value);
                    break;
                case Asda2ItemBonusType.BlockedDamadgeReduction:
                    OwningCharacter.ChangeModifier(StatModifierInt.BlockValue, value);
                    break;
                case Asda2ItemBonusType.OhsSubEffectChange:
                    if (OwningCharacter.Archetype.ClassId == ClassId.OHS)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.SpearSubEffectChange:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Spear)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.ThsSubEffectChange:
                    if (OwningCharacter.Archetype.ClassId == ClassId.THS)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.CrossbowSubEffectChange:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Crossbow)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.BowSubEffectChange:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Bow)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.BalistaSubEffectChange:
                    if (OwningCharacter.Archetype.ClassId == ClassId.Balista)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.StaffSubEffectChange:
                    if (OwningCharacter.Archetype.ClassId == ClassId.AtackMage || OwningCharacter.Archetype.ClassId == ClassId.HealMage || OwningCharacter.Archetype.ClassId == ClassId.SupportMage)
                        OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.HealSkill:
                    OwningCharacter.HealingDoneMod += value;
                    break;
                case Asda2ItemBonusType.RecoverySkill:
                    OwningCharacter.HealingDoneMod += value;
                    break;
                case Asda2ItemBonusType.ProtectiveShieldSkillPrc:
                    break;
                case Asda2ItemBonusType.MonsterAtackFalureRatePrc:
                    break;
                case Asda2ItemBonusType.MonsterCritFailureRate:
                    break;
                case Asda2ItemBonusType.HealRecoverySkill:
                    OwningCharacter.HealingDoneMod += value;
                    break;
                case Asda2ItemBonusType.RecoveryAmountByHealRecoveryPrc:
                    OwningCharacter.HealingDoneModPct += value;
                    break;
                case Asda2ItemBonusType.RepeatedShotSkillDamage:
                    break;
                case Asda2ItemBonusType.RisingDragonSkillDamage:
                    break;
                case Asda2ItemBonusType.ThreadRatioMinusPrc:
                    break;
                case Asda2ItemBonusType.ProtectiveShieldIncrease:
                    break;
                case Asda2ItemBonusType.ExpPenaltyMinusPrc:
                    OwningCharacter.ChangeModifier(StatModifierInt.ExpPenaltyReductionPrc, value);
                    break;
                case Asda2ItemBonusType.PvpDeffense:
                    OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence, value);
                    break;
                case Asda2ItemBonusType.PvpPenetration:
                    OwningCharacter.ChangeModifier(StatModifierInt.Damage, value);
                    break;
                case Asda2ItemBonusType.FishingSkill:
                    OwningCharacter.ChangeModifier(StatModifierInt.Asda2FishingSkill, value);
                    break;
                case Asda2ItemBonusType.FishingGauge:
                    OwningCharacter.ChangeModifier(StatModifierInt.Asda2FishingGauge, value);
                    break;
                case Asda2ItemBonusType.AtackSpeedPrc:
                    OwningCharacter.ChangeModifier(StatModifierFloat.MeleeAttackTime, (float)value / 100);
                    break;
                case Asda2ItemBonusType.MovementSpeedPrc:
                    OwningCharacter.ChangeModifier(StatModifierFloat.Speed, (float)value / 100);
                    break;
                case Asda2ItemBonusType.MagicDeffence:
                    OwningCharacter.ChangeModifier(StatModifierInt.Asda2MagicDefence, value);
                    break;

            }
        }
        private void ModifyStatBySowel(ItemBonusType type, int value)
        {
            value = (int)(value * CharacterFormulas.CalculateEnchantMultiplier(Enchant));
            switch (type)
            {
                case ItemBonusType.Agility:
                    OwningCharacter.ApplyStatMod(ItemModType.Agility, (int)(value * 1.203125f));
                    break;
                case ItemBonusType.AtackSpeedByPrc:
                    OwningCharacter.ApplyStatMod(ItemModType.AtackTimePrc, value);
                    break;
                case ItemBonusType.Defence:
                    OwningCharacter.ApplyStatMod(ItemModType.Asda2Defence, CharacterFormulas.GetSowelDeffence(value, Template.RequiredProfession));
                    break;
                case ItemBonusType.DropByPrc:
                    OwningCharacter.ApplyStatMod(ItemModType.DropChance, value);
                    break;
                case ItemBonusType.DropGoldByPrc:
                    OwningCharacter.ApplyStatMod(ItemModType.DropGoldByPrc, value);
                    break;
                case ItemBonusType.Energy:
                    OwningCharacter.ApplyStatMod(ItemModType.Spirit, (int)(value * 1.5f));
                    break;
                case ItemBonusType.ErengyByPrc:
                    break;
                case ItemBonusType.Expirience:
                    OwningCharacter.ApplyStatMod(ItemModType.Asda2Expirience, value);
                    break;
                case ItemBonusType.FishSizeAndFishingAblilityByPrc:
                    break;
                case ItemBonusType.Intelect:
                    OwningCharacter.ApplyStatMod(ItemModType.Intellect, value);
                    break;
                case ItemBonusType.IntelegenceByPrc:
                    break;
                case ItemBonusType.Luck:
                    OwningCharacter.ApplyStatMod(ItemModType.Luck, (int)(value * 2.625f));
                    break;
                case ItemBonusType.LuckByPrc:
                    break;
                case ItemBonusType.MagicalDamageReduceByPrc:
                    break;
                case ItemBonusType.MovementSpeedByPrc:
                    break;
                case ItemBonusType.PhysicalDamageReduceByPrc:
                    OwningCharacter.ApplyStatMod(ItemModType.Luck, value);
                    break;
                case ItemBonusType.SellingCostBonusType:
                    break;
                case ItemBonusType.Stamina:
                    OwningCharacter.ApplyStatMod(ItemModType.Stamina, (int)(value * 1.5f));
                    break;
                case ItemBonusType.StaminaByPrc:
                    break;
                case ItemBonusType.Strength:
                    OwningCharacter.ApplyStatMod(ItemModType.Strength, value);
                    break;
                case ItemBonusType.StrengthByPrc:
                    break;
                case ItemBonusType.WeaponAtack:
                    //OwningCharacter.ApplyStatMod(ItemModType.Damage, (int)(value * CharacterFormulas.PhysicalAtackSowelMultiplier));
                    break;
                case ItemBonusType.WaponMAtack:
                    //OwningCharacter.ApplyStatMod(ItemModType.MagicDamage, (int) (value*CharacterFormulas.MagicAtackSowelMultiplier));
                    break;
            }
        }
        private void ProcessRemoveSoul(int sowelId)
        {
            var sowel = Asda2ItemMgr.GetTemplate(sowelId);
            if (sowel.SowelBonusType == ItemBonusType.WeaponAtack || sowel.SowelBonusType == ItemBonusType.WaponMAtack)
                return;
            ModifyStatBySowel(sowel.SowelBonusType, -sowel.SowelBonusValue);
            /*switch (sowel.SowelBonusType)
            {
                case ItemBonusType.Agility:
                    OwningCharacter.RemoveStatMod(ItemModType.Agility, sowel.SowelBonusValue);
                    break;
                case ItemBonusType.AtackSpeedByPrc:
                    OwningCharacter.RemoveStatMod(ItemModType.AtackTimePrc,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.Defence:
                    OwningCharacter.RemoveStatMod(ItemModType.Asda2Defence,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.DropByPrc:
                    OwningCharacter.RemoveStatMod(ItemModType.DropChance,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.DropGoldByPrc:
                    OwningCharacter.RemoveStatMod(ItemModType.DropGoldByPrc,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.Energy:
                    OwningCharacter.RemoveStatMod(ItemModType.Spirit, sowel.SowelBonusValue);
                    break;
                case ItemBonusType.ErengyByPrc:
                    break;
                case ItemBonusType.Expirience:
                    OwningCharacter.RemoveStatMod(ItemModType.Asda2Expirience,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.FishSizeAndFishingAblilityByPrc:
                    break;
                case ItemBonusType.Intelect:
                    OwningCharacter.RemoveStatMod(ItemModType.Intellect,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.IntelegenceByPrc:
                    break;
                case ItemBonusType.Luck:
                    OwningCharacter.RemoveStatMod(ItemModType.Luck,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.LuckByPrc:
                    break;
                case ItemBonusType.MagicalDamageReduceByPrc:
                    break;
                case ItemBonusType.MovementSpeedByPrc:
                    break;
                case ItemBonusType.PhysicalDamageReduceByPrc:
                    OwningCharacter.RemoveStatMod(ItemModType.Luck,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.SellingCostBonusType:
                    break;
                case ItemBonusType.Stamina:
                    OwningCharacter.RemoveStatMod(ItemModType.Stamina,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.StaminaByPrc:
                    break;
                case ItemBonusType.Strength:
                    OwningCharacter.RemoveStatMod(ItemModType.Strength,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.StrengthByPrc:
                    break;
                case ItemBonusType.WeaponAtack:
                    OwningCharacter.RemoveStatMod(ItemModType.DamagePrc,  sowel.SowelBonusValue);
                    break;
                case ItemBonusType.WaponMAtack:
                    OwningCharacter.RemoveStatMod(ItemModType.MagicDamagePrc,  sowel.SowelBonusValue);
                    break;
            }*/
        }


        #endregion

        #region Using
        /// <summary>
        /// Called whenever an item is used.
        /// Make sure to only call on Items whose Template has a UseSpell.
        /// </summary>
        internal void OnUse()
        {
            if (m_template.BondType == ItemBondType.OnUse)
            {
                //Flags |= ItemFlags.Soulbound;
                IsSoulbound = true;
            }

            /*if (m_template.UseSpell != null)
            {
                // consume a charge
                if (m_template.UseSpell.HasCharges)
                {
                    SpellCharges = SpellCharges < 0 ? SpellCharges+1 : SpellCharges-1;
                }
            }*/

            m_template.NotifyUsed(this);
        }

        #endregion

        #region Destroy / Remove
        public void Destroy()
        {
            DoDestroy();
        }

        /// <summary>
        /// Called by the container to 
        /// </summary>
        protected internal virtual void DoDestroy()
        {
            var record = m_record;
            if (m_owner != null)
                m_owner.Asda2Inventory.RemoveItemFromInventory(this);
            if (record != null)
            {
                record.OwnerId = 0;
                record.DeleteLater();
                m_record = null;

                Dispose();
            }
        }

        #endregion

        #region QuestHolder


        public QuestHolderInfo QuestHolderInfo
        {
            get { return m_template.QuestHolderInfo; }
        }

        public bool CanGiveQuestTo(Character chr)
        {
            return m_owner == chr;
        }

        public void OnQuestGiverStatusQuery(Character chr)
        {
            // do nothing
        }
        #endregion

        public override void Dispose(bool disposing)
        {
            m_owner = null;
            m_isInWorld = false;
            IsDeleted = true;
        }

        public override string ToString()
        {
            return string.Format("[{0}]{1} Amount {2} Category {3}",
                ItemId, (Asda2ItemId)ItemId, Amount, Category);
        }

        public bool IsInContext
        {
            get
            {
                var owner = Owner;
                if (owner != null)
                {
                    var context = owner.ContextHandler;
                    if (context != null)
                    {
                        return context.IsInContext;
                    }
                }
                return false;
            }
        }

        public bool IsWeapon
        {
            get { return Template.IsWeapon; }
            set { }
        }

        public int BoosterId
        {
            get { return Template.BoosterId; }
        }

        public int PackageId { get { return Template.PackageId; } }
        public bool IsArmor
        {
            get { return Template.IsArmor; }
        }

        public int Deffence { get; set; }
        public int MagicDeffence { get; set; }

        protected int SocketsCount
        {
            get { return Template.SowelSocketsCount; }
        }

        public void AddMessage(IMessage message)
        {
            var owner = Owner;
            if (owner != null)
            {
                owner.AddMessage(message);
            }
        }

        public void AddMessage(Action action)
        {
            var owner = Owner;
            if (owner != null)
            {
                owner.AddMessage(action);
            }
        }

        public bool ExecuteInContext(Action action)
        {
            var owner = Owner;
            if (owner != null)
            {
                return owner.ExecuteInContext(action);
            }
            return false;
        }

        public void EnsureContext()
        {
            var owner = Owner;
            if (owner != null)
            {
                owner.EnsureContext();
            }
        }

        public List<Asda2ItemTemplate> InsertedSowels
        {
            get
            {
                var sowels = new List<Asda2ItemTemplate>();
                if (Soul1Id != 0)
                {
                    var s = Asda2ItemMgr.GetTemplate(Soul1Id);
                    if (s != null)
                        sowels.Add(s);
                }
                if (Soul2Id != 0)
                {
                    var s = Asda2ItemMgr.GetTemplate(Soul2Id);
                    if (s != null)
                        sowels.Add(s);
                }
                if (Soul3Id != 0)
                {
                    var s = Asda2ItemMgr.GetTemplate(Soul3Id);
                    if (s != null)
                        sowels.Add(s);
                }
                if (Soul4Id != 0)
                {
                    var s = Asda2ItemMgr.GetTemplate(Soul4Id);
                    if (s != null)
                        sowels.Add(s);
                }
                return sowels;
            }
        }

        public byte RequiredLevel
        {
            get { return (byte)Template.RequiredLevel; }
        }

        public bool IsRod
        {
            get { return Template.IsRod; }
        }

        public bool SetParametr(Asda2ItemBonusType type, short value, byte slot)
        {
            if (slot > 4)
                return false;
            switch (slot)
            {
                case 0:
                    Parametr1Type = type;
                    Parametr1Value = value;
                    break;
                case 1:
                    Parametr2Type = type;
                    Parametr2Value = value;
                    break;
                case 2:
                    Parametr3Type = type;
                    Parametr3Value = value;
                    break;
                case 3:
                    Parametr4Type = type;
                    Parametr4Value = value;
                    break;
                case 4:
                    Parametr5Type = type;
                    Parametr5Value = value;
                    break;
            }
            RecalculateItemParametrs();
            return true;
        }

        public Dictionary<ItemBonusType, int> Bonuses = new Dictionary<ItemBonusType, int>();
        private void RecalculateItemParametrs()
        {
            if (IsWeapon)
            {
                var s = Asda2ItemMgr.GetTemplate(Soul1Id);
                if (s == null)
                    Damages = new[] { new DamageInfo(DamageSchoolMask.Physical, 1, 3) };
                else
                {
                    var enchantMultiplier = CharacterFormulas.CalculateEnchantMultiplier(Enchant);
                    var typeMultiplier = CharacterFormulas.CalcWeaponTypeMultiplier(Category, OwningCharacter == null ? ClassId.NoClass : OwningCharacter.Archetype.ClassId);
                    Damages = new[] { new DamageInfo(Category == Asda2ItemCategory.Staff
                        ? DamageSchoolMask.Magical : DamageSchoolMask.Physical,
                        s.SowelBonusValue * enchantMultiplier*typeMultiplier, s.SowelBonusValue * enchantMultiplier*1.1f*typeMultiplier) };

                }
            }
        }

        private float OprionValueMultiplier
        {
            get
            {
                var b = 1f;
                if (Record.IsCrafted)
                    b += 0.5f;
                switch (Template.Quality)
                {
                    case Asda2ItemQuality.White:
                        break;
                    case Asda2ItemQuality.Yello:
                        b += 0.1f;
                        break;
                    case Asda2ItemQuality.Purple:
                        b += 0.2f;
                        break;
                    case Asda2ItemQuality.Green:
                        b += 0.35f;
                        break;
                    case Asda2ItemQuality.Orange:
                        b += 0.5f;
                        break;
                }
                return b;
            }
        }

        public void GenerateNewOptions()
        {
            var mult = OprionValueMultiplier;
            var cmn = Template.StatGeneratorCommon.GetBonus();
            Parametr1Type = cmn.Type;
            Parametr1Value = (short)(cmn.GetValue() * mult);
            cmn = Template.StatGeneratorCommon.GetBonus();
            Parametr2Type = cmn.Type;
            Parametr2Value = (short)(cmn.GetValue() * mult);
            if (Record.IsCrafted)
                GenerateOptionsByCraft();
            if (Enchant >= CharacterFormulas.OptionStatStartsWithEnchantValue)
                GenerateOptionsByUpgrade();
        }

        public void GenerateOptionsByCraft()
        {
            var craft = Template.StatGeneratorCraft.GetBonus();
            Parametr3Type = craft.Type;
            Parametr3Value = (short)(craft.GetValue() * OprionValueMultiplier);
        }

        public void GenerateOptionsByUpgrade()
        {
            var upgrade = Template.StatGeneratorEnchant.GetBonus();
            Parametr4Type = upgrade.Type;
            Parametr4Value = (short)(upgrade.GetValue() * OprionValueMultiplier * CharacterFormulas.CalculateEnchantMultiplierNotDamageItemStats(Enchant));
        }
        //advanced - 5
        //upgrade - 4
        //craft - 3
        //normal - 2
        //normal - 1
        public void SetRandomAdvancedEnchant()
        {
            var advanced = Template.StatGeneratorAdvanced.GetBonus();
            Parametr5Type = advanced.Type;
            Parametr5Value = (short)(advanced.GetValue() * OprionValueMultiplier);
        }
    }
    public enum Asda2ItemBonusType
    {
        None = 0,
        MaxAtack = 1,
        MaxMAtak = 2,
        MaxDef = 3,
        NormalAtackCrit = 4,
        MaxHp = 5,
        MaxMp = 6,
        HpPotionRecovery = 7,
        MpPotionRecovery = 8,
        RecoverBadCondition = 9,
        HpRegeneration = 10,
        MpRegeneration = 11,
        FireAttribute = 12,
        WaterAttribue = 13,
        EarthAttribute = 14,
        ClimateAtribute = 15,
        LightAttribute = 16,
        DarkAttribute = 17,
        FireResistance = 18,
        WaterResistance = 19,
        EarthResistance = 20,
        ClimateResistance = 21,
        LightResistance = 22,
        DarkResistance = 23,
        CraftingChance = 24,
        OhsSkillDamage = 25,
        SpearSkillDamage = 26,
        ThsSkillDamage = 27,
        CrossbowSkillDamage = 28,
        BowSkillDamage = 29,
        BalistaSkillDamage = 30,
        StaffSkillDamage = 31,
        ProtectorSkillDamage = 32,
        KuckleSkillDamage = 33,
        ClawSkillDamage = 34,
        BackpackSkillDamage = 35,
        TwoHandDaggerSkillDamage = 36,
        DualWeildSkillDamage = 37,
        CristalBeadSkillDamage = 38,
        OhsSkillCrit = 39,
        SpearSkillCrit = 40,
        ThsSkillCrit = 41,
        CrossbowSkillCrit = 42,
        BowSkillCrit = 43,
        BalistaSkillCrit = 44,
        StaffSkillCrit = 45,
        ProtectorSkillCrit = 46,
        KuckleSkillCrit = 47,
        ClawSkillCrit = 48,
        BackpackSkillCrit = 49,
        TwoHandDaggerSkillCrit = 50,
        DualWeildSkillCrit = 51,
        CristalBeadSkillCrit = 52,
        HuntingExp = 53,
        HuntingExpMinus = 54,
        QuestExp = 55,
        QuestExpMinus = 56,
        SkillRange = 57,
        CraftingLevelPlus = 58,
        CraftingLevelMinus = 59,
        RecoveryAmount = 60,
        DropRate = 61,
        ExpItem = 62,
        EmptySlot = 63,
        MinAtack = 64,
        MinMAtack = 65,
        MinDef = 66,
        Atack = 67,
        MAtack = 68,
        Deffence = 69,
        DodgePrc = 70,
        MinBlockRatePrc = 71,
        MaxBlockRatePrc = 72,
        BlockRatePrc = 73,
        BlockedDamadgeReduction = 74,
        OhsSubEffectChange = 75,
        SpearSubEffectChange = 76,
        ThsSubEffectChange = 77,
        CrossbowSubEffectChange = 78,
        BowSubEffectChange = 79,
        BalistaSubEffectChange = 80,
        StaffSubEffectChange = 81,
        HealSkill = 82,
        RecoverySkill = 83,
        ProtectiveShieldSkillPrc = 84,
        MonsterAtackFalureRatePrc = 85,
        MonsterCritFailureRate = 86,
        HealRecoverySkill = 87,
        RecoveryAmountByHealRecoveryPrc = 88,
        RepeatedShotSkillDamage = 89,
        RisingDragonSkillDamage = 90,
        ThreadRatioMinusPrc = 91,
        ProtectiveShieldIncrease = 92,
        ExpPenaltyMinusPrc = 93,
        PvpDeffense = 94,
        PvpPenetration = 95,
        FishingSkill = 96,
        FishingGauge = 97,
        AtackSpeedPrc = 98,
        MovementSpeedPrc = 99,
        MagicDeffence = 115,
        End
    }
}