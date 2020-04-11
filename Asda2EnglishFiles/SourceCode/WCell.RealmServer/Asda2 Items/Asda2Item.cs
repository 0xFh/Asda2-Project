using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Quests;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
    public class Asda2Item : ObjectBase, IOwned, IAsda2Weapon, INamed, ILockable, ILootable, IQuestHolder, IEntity,
        IAsda2MountableItem, IContextHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Item);
        public static readonly Item PlaceHolder = new Item();
        public static int AvatarLvlDefenceBonusDivider = 10;
        public static int ArmorLvlDefenceBonusDivider = 10;
        public static int AcceoryLvlDefenceBonusDivider = 10;
        public static int WeaponLvlDefenceBonusDivider = 10;
        public Dictionary<ItemBonusType, int> Bonuses = new Dictionary<ItemBonusType, int>();
        protected Asda2ItemTemplate m_template;
        protected internal bool m_isInWorld;

        /// <summary>
        /// Items are unknown when a creation update
        /// has not been sent to the Owner yet.
        /// </summary>
        internal bool m_unknown;

        protected internal Character m_owner;
        protected IProcHandler m_hitProc;
        protected Asda2ItemRecord m_record;
        private int _itemId;
        private short _slot;
        private byte _inventoryType;
        private DamageInfo[] _damages;

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return Asda2Item.UpdateFieldInfos; }
        }

        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Item; }
        }

        public static Asda2Item CreateItem(int templateId, Character owner, int amount)
        {
            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(templateId);
            if (template != null)
                return Asda2Item.CreateItem(template, owner, amount);
            return (Asda2Item) null;
        }

        public static Asda2Item CreateItem(Asda2ItemId templateId, Character owner, int amount)
        {
            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(templateId);
            if (template != null)
                return Asda2Item.CreateItem(template, owner, amount);
            return (Asda2Item) null;
        }

        public static Asda2Item CreateItem(Asda2ItemTemplate template, Character owner, int amount)
        {
            Asda2Item asda2Item = template.Create();
            asda2Item.InitItem(template, owner, amount);
            return asda2Item;
        }

        public static Asda2Item CreateItem(Asda2ItemRecord record, Character owner)
        {
            Asda2ItemTemplate template = record.Template;
            if (template == null)
            {
                Asda2Item.log.Warn("{0} had an ItemRecord with invalid ItemId: {1}", (object) owner, (object) record);
                return (Asda2Item) null;
            }

            Asda2Item asda2Item = template.Create();
            asda2Item.LoadItem(record, owner, template);
            return asda2Item;
        }

        public static Asda2Item CreateItem(Asda2ItemRecord record, Character owner, Asda2ItemTemplate template)
        {
            Asda2Item asda2Item = template.Create();
            asda2Item.LoadItem(record, owner, template);
            return asda2Item;
        }

        public static Asda2Item CreateItem(Asda2ItemRecord record, Asda2ItemTemplate template)
        {
            Asda2Item asda2Item = template.Create();
            asda2Item.LoadItem(record, template);
            return asda2Item;
        }

        protected internal Asda2Item()
        {
        }

        /// <summary>Initializes a new Item</summary>
        internal void InitItem(Asda2ItemTemplate template, Character owner, int amount)
        {
            this.m_record = Asda2ItemRecord.CreateRecord(template);
            this.Type |= ObjectTypes.Item;
            this.m_template = template;
            this.Durability = this.m_template.MaxDurability;
            this.MaxDurability = this.m_template.MaxDurability;
            this.Amount = amount;
            this.OwningCharacter = owner;
            this.ItemId = (int) template.ItemId;
            this.EntityId = new EntityId((uint) this.m_record.Guid, HighId.Item);
            this.GenerateNewOptions();
            this.RecalculateItemParametrs();
            template.NotifyCreated(this.m_record);
            this.OnInit();
        }

        /// <summary>Loads an already created item</summary>
        internal void LoadItem(Asda2ItemRecord record, Character owner, Asda2ItemTemplate template)
        {
            this.m_record = record;
            this.OwningCharacter = owner;
            this.LoadItem(record, template);
        }

        /// <summary>Loads an already created item without owner</summary>
        /// <param name="record"></param>
        /// <param name="template"></param>
        internal void LoadItem(Asda2ItemRecord record, Asda2ItemTemplate template)
        {
            this.m_record = record;
            this.m_template = template;
            this.EntryId = this.m_template.Id;
            this.ItemId = (int) template.ItemId;
            this.Type |= ObjectTypes.Item;
            this._slot = record.Slot;
            this._inventoryType = record.InventoryType;
            this.SetInt32((UpdateFieldId) ItemFields.DURABILITY, (int) record.Durability);
            this.SetInt32((UpdateFieldId) ItemFields.DURATION, record.Duration);
            this.SetInt32((UpdateFieldId) ItemFields.STACK_COUNT, record.Amount);
            this.MaxDurability = this.m_template.MaxDurability;
            this.RecalculateItemParametrs();
            this.OnLoad();
        }

        /// <summary>
        /// Called after initializing a newly created Item (Owner might be null)
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>Called after loading an Item (Owner might be null)</summary>
        protected virtual void OnLoad()
        {
        }

        public Asda2ItemTemplate Template
        {
            get { return this.m_template; }
        }

        public LockEntry Lock
        {
            get { return this.m_template.Lock; }
        }

        public override bool IsInWorld
        {
            get { return this.m_isInWorld; }
        }

        /// <summary>Whether this object has already been deleted.</summary>
        public bool IsDeleted { get; internal set; }

        /// <summary>Checks whether this Item can currently be used</summary>
        public bool CanBeUsed
        {
            get
            {
                if (this.MaxDurability == (byte) 0 || this.Durability > (byte) 0)
                    return this.m_loot == null;
                return false;
            }
        }

        /// <summary>The name of this item</summary>
        public string Name
        {
            get
            {
                if (this.m_template != null)
                    return this.m_template.Name;
                return "";
            }
        }

        public bool CanBeTraded
        {
            get
            {
                if (this.m_template.MaxDurability != (byte) 0)
                    return this.Durability > (byte) 0;
                return true;
            }
        }

        /// <summary>See IUsable.Owner</summary>
        public Unit Owner
        {
            get { return (Unit) this.m_owner; }
        }

        /// <summary>Whether this Item is currently equipped.</summary>
        public bool IsEquipped
        {
            get { return this.InventoryType == Asda2InventoryType.Equipment; }
        }

        Asda2InventoryError IAsda2MountableItem.CheckEquip(Character owner)
        {
            return this.Template.CheckEquip(owner);
        }

        /// <summary>
        /// Whether this Item is currently equipped and is not a kind of container.
        /// </summary>
        public bool IsEquippedItem
        {
            get { return this.InventoryType == Asda2InventoryType.Equipment; }
        }

        /// <summary>Wheter this item's bonuses are applied</summary>
        public bool IsApplied { get; private set; }

        /// <summary>
        /// Called when this Item was added to someone's inventory
        /// </summary>
        protected internal void OnAdd()
        {
            if (this.m_template.BondType != ItemBondType.OnPickup && this.m_template.BondType != ItemBondType.Quest)
                return;
            this.IsSoulbound = true;
        }

        /// <summary>
        /// Saves all recent changes that were made to this Item to the DB
        /// </summary>
        public void Save()
        {
            if (this.IsDeleted)
            {
                LogUtil.ErrorException(
                    (Exception) new InvalidOperationException("Trying to save deleted Item: " + (object) this));
            }
            else
            {
                try
                {
                    this.m_record.SaveAndFlush();
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex,
                        string.Format("failed to save item, item {0} acc {1}[{2}]", (object) this.Name,
                            this.OwningCharacter == null ? (object) "null" : (object) this.OwningCharacter.Name,
                            (object) (uint) (this.OwningCharacter == null ? 999 : (int) this.OwningCharacter.AccId)),
                        new object[0]);
                }
            }
        }

        /// <summary>
        /// Subtracts the given amount from this item and creates a new item, with that amount.
        /// WARNING: Make sure that this item is belonging to someone and that amount is valid!
        /// </summary>
        /// <param name="amount">The amount of the newly created item</param>
        public Asda2Item Split(int amount)
        {
            this.Amount -= amount;
            return Asda2Item.CreateItem(this.m_template, this.OwningCharacter, amount);
        }

        /// <summary>TODO: Random properties</summary>
        public bool CanStackWith(Asda2Item otherItem)
        {
            if (this.m_template.IsStackable)
                return this.m_template == otherItem.m_template;
            return false;
        }

        /// <summary>A chest was looted empty</summary>
        public override void OnFinishedLooting()
        {
            this.Destroy();
        }

        public override uint GetLootId(Asda2LootEntryType type)
        {
            return this.m_template.Id;
        }

        /// <summary>
        /// Called when this Item gets equipped.
        /// Requires map context.
        /// </summary>
        public void OnEquip()
        {
            if (this.IsApplied)
                return;
            this.IsApplied = true;
            this.RecalculateItemParametrs();
            int slot = (int) this.Slot;
            Character owningCharacter = this.OwningCharacter;
            if (this.Soul1Id != 0)
                this.ProcessAddSoul(this.Soul1Id);
            if (this.Soul2Id != 0)
                this.ProcessAddSoul(this.Soul2Id);
            if (this.Soul3Id != 0)
                this.ProcessAddSoul(this.Soul3Id);
            if (this.Soul4Id != 0)
                this.ProcessAddSoul(this.Soul4Id);
            if (this.Parametr1Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr1Type, (int) this.Parametr1Value);
            if (this.Parametr2Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr2Type, (int) this.Parametr2Value);
            if (this.Parametr3Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr3Type, (int) this.Parametr3Value);
            if (this.Parametr4Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr4Type, (int) this.Parametr4Value);
            if (this.Parametr5Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr5Type, (int) this.Parametr5Value);
            if (this.Category == Asda2ItemCategory.RodFishingSkill)
                this.OwningCharacter.ApplyStatMod(ItemModType.FishingSkill, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RodGauge)
                this.OwningCharacter.ApplyStatMod(ItemModType.FishingGauge, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RodFishingSkillAndGauge)
            {
                this.OwningCharacter.ApplyStatMod(ItemModType.FishingSkill, this.Template.ValueOnUse);
                this.OwningCharacter.ApplyStatMod(ItemModType.FishingGauge, this.Template.ValueOnUse);
            }
            else if (this.Category == Asda2ItemCategory.NacklessMDef || this.Category == Asda2ItemCategory.RingMDef)
                this.OwningCharacter.ApplyStatMod(ItemModType.Asda2MagicDefence,
                    (int) ((double) this.Template.ValueOnUse *
                           (double) CharacterFormulas.ItemsMagicDeffenceMultiplier));
            else if (this.Category == Asda2ItemCategory.NacklessCriticalChance)
            {
                this.OwningCharacter.ApplyStatMod(ItemModType.CriticalStrikeRating, this.Template.ValueOnUse);
                this.OwningCharacter.ApplyStatMod(ItemModType.SpellCriticalStrikeRating, this.Template.ValueOnUse);
            }
            else if (this.Category == Asda2ItemCategory.NacklessHealth)
                this.OwningCharacter.ApplyStatMod(ItemModType.Health, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.NacklessMana)
                this.OwningCharacter.ApplyStatMod(ItemModType.Power, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RingMaxDef)
                this.OwningCharacter.ApplyStatMod(ItemModType.Asda2Defence, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RingMaxMAtack)
                this.OwningCharacter.ApplyStatMod(ItemModType.MagicDamage, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RingMaxAtack)
                this.OwningCharacter.ApplyStatMod(ItemModType.Damage, this.Template.ValueOnUse);

            this.IsSoulbound = true;
            if (this.m_template.EquipmentSlot == Asda2EquipmentSlots.Shild)
                owningCharacter.UpdateBlockChance();
            SetItemDataRecord setItemRecord = SetItemManager.GetSetItemRecord(this.ItemId);
            if (setItemRecord != null)
            {
                if (!this.OwningCharacter.AppliedSets.ContainsKey(setItemRecord.Id))
                {
                    this.OwningCharacter.AppliedSets.Add(setItemRecord.Id, (byte) 1);
                }
                else
                {
                    Dictionary<int, byte> appliedSets;
                    int id;
                    (appliedSets = this.OwningCharacter.AppliedSets)[id = setItemRecord.Id] =
                        (byte) ((uint) appliedSets[id] + 1U);
                }

                this.AddSetBonus(setItemRecord.GetBonus(this.OwningCharacter.AppliedSets[setItemRecord.Id]));
            }

            Asda2CharacterHandler.SendUpdateStatsOneResponse(this.OwningCharacter.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(this.OwningCharacter.Client);
            this.m_template.NotifyEquip(this);
        }

        /// <summary>
        /// Called when this Item gets unequipped.
        /// Requires map context.
        /// </summary>
        public void OnUnEquip()
        {
            if (!this.IsApplied)
                return;
            this.IsApplied = false;
            if (this.Soul1Id != 0)
                this.ProcessRemoveSoul(this.Soul1Id);
            if (this.Soul2Id != 0)
                this.ProcessRemoveSoul(this.Soul2Id);
            if (this.Soul3Id != 0)
                this.ProcessRemoveSoul(this.Soul3Id);
            if (this.Soul4Id != 0)
                this.ProcessRemoveSoul(this.Soul4Id);
            if (this.Parametr1Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr1Type, (int) -this.Parametr1Value);
            if (this.Parametr2Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr2Type, (int) -this.Parametr2Value);
            if (this.Parametr3Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr3Type, (int) -this.Parametr3Value);
            if (this.Parametr4Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr4Type, (int) -this.Parametr4Value);
            if (this.Parametr5Type != Asda2ItemBonusType.None)
                this.ModifyStat(this.Parametr5Type, (int) -this.Parametr5Value);
            if (this.Category == Asda2ItemCategory.RodFishingSkill)
                this.OwningCharacter.RemoveStatMod(ItemModType.FishingSkill, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RodGauge)
                this.OwningCharacter.RemoveStatMod(ItemModType.FishingGauge, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RodFishingSkillAndGauge)
            {
                this.OwningCharacter.RemoveStatMod(ItemModType.FishingSkill, this.Template.ValueOnUse);
                this.OwningCharacter.RemoveStatMod(ItemModType.FishingGauge, this.Template.ValueOnUse);
            }
            else if (this.Category == Asda2ItemCategory.NacklessMDef || this.Category == Asda2ItemCategory.RingMDef)
                this.OwningCharacter.RemoveStatMod(ItemModType.Asda2MagicDefence,
                    (int) ((double) this.Template.ValueOnUse *
                           (double) CharacterFormulas.ItemsMagicDeffenceMultiplier));
            else if (this.Category == Asda2ItemCategory.NacklessCriticalChance)
            {
                this.OwningCharacter.RemoveStatMod(ItemModType.CriticalStrikeRating, this.Template.ValueOnUse);
                this.OwningCharacter.RemoveStatMod(ItemModType.SpellCriticalStrikeRating, this.Template.ValueOnUse);
            }
            else if (this.Category == Asda2ItemCategory.NacklessHealth)
                this.OwningCharacter.RemoveStatMod(ItemModType.Health, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.NacklessMana)
                this.OwningCharacter.RemoveStatMod(ItemModType.Power, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RingMaxDef)
                this.OwningCharacter.RemoveStatMod(ItemModType.Asda2Defence, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RingMaxMAtack)
                this.OwningCharacter.RemoveStatMod(ItemModType.MagicDamage, this.Template.ValueOnUse);
            else if (this.Category == Asda2ItemCategory.RingMaxAtack)
                this.OwningCharacter.RemoveStatMod(ItemModType.Damage, this.Template.ValueOnUse);

            if (this.m_template.EquipmentSlot == Asda2EquipmentSlots.Shild)
                this.m_owner.UpdateBlockChance();
            SetItemDataRecord setItemRecord = SetItemManager.GetSetItemRecord(this.ItemId);
            if (setItemRecord != null && this.OwningCharacter.AppliedSets.ContainsKey(setItemRecord.Id))
            {
                this.RemoveSetBonus(setItemRecord.GetBonus(this.OwningCharacter.AppliedSets[setItemRecord.Id]));
                Dictionary<int, byte> appliedSets;
                int id;
                (appliedSets = this.OwningCharacter.AppliedSets)[id = setItemRecord.Id] =
                    (byte) ((uint) appliedSets[id] - 1U);
            }

            if (this.m_hitProc != null)
            {
                this.m_owner.RemoveProcHandler(this.m_hitProc);
                this.m_hitProc = (IProcHandler) null;
            }

            this.m_template.NotifyUnequip(this);
            this.RecalculateItemParametrs();
            Asda2CharacterHandler.SendUpdateStatsOneResponse(this.OwningCharacter.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(this.OwningCharacter.Client);
        }

        private void AddSetBonus(Asda2SetBonus bonus)
        {
            if (bonus == null)
                return;
            this.ModifyStat((Asda2ItemBonusType) bonus.Type, bonus.Value);
        }

        private void RemoveSetBonus(Asda2SetBonus bonus)
        {
            if (bonus == null)
                return;
            this.ModifyStat((Asda2ItemBonusType) bonus.Type, -bonus.Value);
        }

        private void ProcessAddSoul(int sowelId)
        {
            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(sowelId);
            if (template.SowelBonusType == ItemBonusType.WeaponAtack ||
                template.SowelBonusType == ItemBonusType.WaponMAtack)
                return;
            this.ModifyStatBySowel(template.SowelBonusType, template.SowelBonusValue);
        }

        private void ModifyStat(Asda2ItemBonusType type, int value)
        {
            value = (int) ((double) value * (double) CharacterFormulas.CalculateEnchantMultiplier(this.Enchant));
            switch (type)
            {
                case Asda2ItemBonusType.MaxAtack:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Damage,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MaxMAtak:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MaxDef:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.NormalAtackCrit:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.CritChance, value);
                    break;
                case Asda2ItemBonusType.MaxHp:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Health, value);
                    break;
                case Asda2ItemBonusType.MaxMp:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Power, value);
                    break;
                case Asda2ItemBonusType.HpPotionRecovery:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Health, value);
                    break;
                case Asda2ItemBonusType.MpPotionRecovery:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Power, value);
                    break;
                case Asda2ItemBonusType.RecoverBadCondition:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Health, value);
                    break;
                case Asda2ItemBonusType.HpRegeneration:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.HealthRegen, value);
                    break;
                case Asda2ItemBonusType.MpRegeneration:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.PowerRegen, value);
                    break;
                case Asda2ItemBonusType.FireAttribute:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.FireAttribute, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.WaterAttribue:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.WaterAttribute, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.EarthAttribute:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.EarthAttribute, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.ClimateAtribute:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.ClimateAttribute, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.LightAttribute:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.LightAttribute, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.DarkAttribute:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.DarkAttribute, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.FireResistance:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.FireResist, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.WaterResistance:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.WaterResist, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.EarthResistance:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.EarthResit, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.ClimateResistance:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.ClimateResist, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.LightResistance:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.LightResist, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.DarkResistance:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.DarkResit, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.CraftingChance:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.CraftingChance, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.OhsSkillDamage:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.OHS)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.SpearSkillDamage:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Spear)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.ThsSkillDamage:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.THS)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.CrossbowSkillDamage:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Crossbow)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.BowSkillDamage:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Bow)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.BalistaSkillDamage:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Balista)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.StaffSkillDamage:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.AtackMage &&
                        this.OwningCharacter.Archetype.ClassId != ClassId.HealMage &&
                        this.OwningCharacter.Archetype.ClassId != ClassId.SupportMage)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
                    break;
                case Asda2ItemBonusType.OhsSkillCrit:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.OHS)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.SpearSkillCrit:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Spear)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.ThsSkillCrit:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.THS)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.CrossbowSkillCrit:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Crossbow)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.BowSkillCrit:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Bow)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.BalistaSkillCrit:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Balista)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.StaffSkillCrit:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.AtackMage &&
                        this.OwningCharacter.Archetype.ClassId != ClassId.HealMage &&
                        this.OwningCharacter.Archetype.ClassId != ClassId.SupportMage)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
                    break;
                case Asda2ItemBonusType.HuntingExp:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.HuntingExpMinus:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount,
                        (float) (-(double) value / 100.0));
                    break;
                case Asda2ItemBonusType.QuestExp:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.QuestExpMinus:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount,
                        (float) (-(double) value / 100.0));
                    break;
                case Asda2ItemBonusType.SkillRange:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellRange, value);
                    break;
                case Asda2ItemBonusType.RecoveryAmount:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.HealthRegen, value);
                    break;
                case Asda2ItemBonusType.DropRate:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.Asda2DropChance, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.ExpItem:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.MinAtack:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Damage,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MinMAtack:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MinDef:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.Atack:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Damage, value);
                    break;
                case Asda2ItemBonusType.MAtack:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage, value);
                    break;
                case Asda2ItemBonusType.Deffence:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence, value);
                    break;
                case Asda2ItemBonusType.DodgePrc:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.DodgeChance, value);
                    break;
                case Asda2ItemBonusType.MinBlockRatePrc:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.BlockChance,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.MaxBlockRatePrc:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.BlockChance,
                        (int) ((double) value * (double) CharacterFormulas.MaxToTotalMultiplier));
                    break;
                case Asda2ItemBonusType.BlockRatePrc:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.BlockChance, value);
                    break;
                case Asda2ItemBonusType.BlockedDamadgeReduction:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.BlockValue, value);
                    break;
                case Asda2ItemBonusType.OhsSubEffectChange:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.OHS)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.SpearSubEffectChange:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Spear)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.ThsSubEffectChange:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.THS)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.CrossbowSubEffectChange:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Crossbow)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.BowSubEffectChange:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Bow)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.BalistaSubEffectChange:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.Balista)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.StaffSubEffectChange:
                    if (this.OwningCharacter.Archetype.ClassId != ClassId.AtackMage &&
                        this.OwningCharacter.Archetype.ClassId != ClassId.HealMage &&
                        this.OwningCharacter.Archetype.ClassId != ClassId.SupportMage)
                        break;
                    this.OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
                    break;
                case Asda2ItemBonusType.HealSkill:
                    this.OwningCharacter.HealingDoneMod += value;
                    break;
                case Asda2ItemBonusType.RecoverySkill:
                    this.OwningCharacter.HealingDoneMod += value;
                    break;
                case Asda2ItemBonusType.HealRecoverySkill:
                    this.OwningCharacter.HealingDoneMod += value;
                    break;
                case Asda2ItemBonusType.RecoveryAmountByHealRecoveryPrc:
                    this.OwningCharacter.HealingDoneModPct += (float) value;
                    break;
                case Asda2ItemBonusType.ExpPenaltyMinusPrc:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.ExpPenaltyReductionPrc, value);
                    break;
                case Asda2ItemBonusType.PvpDeffense:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence, value);
                    break;
                case Asda2ItemBonusType.PvpPenetration:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Damage, value);
                    break;
                case Asda2ItemBonusType.FishingSkill:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Asda2FishingSkill, value);
                    break;
                case Asda2ItemBonusType.FishingGauge:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Asda2FishingGauge, value);
                    break;
                case Asda2ItemBonusType.AtackSpeedPrc:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.MeleeAttackTime, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.MovementSpeedPrc:
                    this.OwningCharacter.ChangeModifier(StatModifierFloat.Speed, (float) value / 100f);
                    break;
                case Asda2ItemBonusType.MagicDeffence:
                    this.OwningCharacter.ChangeModifier(StatModifierInt.Asda2MagicDefence, value);
                    break;
            }
        }

        private void ModifyStatBySowel(ItemBonusType type, int value)
        {
            value = (int) ((double) value * (double) CharacterFormulas.CalculateEnchantMultiplier(this.Enchant));
            switch (type)
            {
                case ItemBonusType.Defence:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Asda2Defence,
                        CharacterFormulas.GetSowelDeffence(value, this.Template.RequiredProfession));
                    break;
                case ItemBonusType.Strength:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Strength, value);
                    break;
                case ItemBonusType.Agility:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Agility, (int) ((double) value * (77.0 / 64.0)));
                    break;
                case ItemBonusType.Stamina:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Stamina, (int) ((double) value * 1.5));
                    break;
                case ItemBonusType.Energy:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Spirit, (int) ((double) value * 1.5));
                    break;
                case ItemBonusType.Intelect:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Intellect, value);
                    break;
                case ItemBonusType.Luck:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Luck, (int) ((double) value * 2.625));
                    break;
                case ItemBonusType.AtackSpeedByPrc:
                    this.OwningCharacter.ApplyStatMod(ItemModType.AtackTimePrc, value);
                    break;
                case ItemBonusType.PhysicalDamageReduceByPrc:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Luck, value);
                    break;
                case ItemBonusType.DropGoldByPrc:
                    this.OwningCharacter.ApplyStatMod(ItemModType.DropGoldByPrc, value);
                    break;
                case ItemBonusType.Expirience:
                    this.OwningCharacter.ApplyStatMod(ItemModType.Asda2Expirience, value);
                    break;
                case ItemBonusType.DropByPrc:
                    this.OwningCharacter.ApplyStatMod(ItemModType.DropChance, value);
                    break;
            }
        }

        private void ProcessRemoveSoul(int sowelId)
        {
            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(sowelId);
            if (template.SowelBonusType == ItemBonusType.WeaponAtack ||
                template.SowelBonusType == ItemBonusType.WaponMAtack)
                return;
            this.ModifyStatBySowel(template.SowelBonusType, -template.SowelBonusValue);
        }

        /// <summary>
        /// Called whenever an item is used.
        /// Make sure to only call on Items whose Template has a UseSpell.
        /// </summary>
        internal void OnUse()
        {
            if (this.m_template.BondType == ItemBondType.OnUse)
                this.IsSoulbound = true;
            this.m_template.NotifyUsed(this);
        }

        public void Destroy()
        {
            this.DoDestroy();
        }

        /// <summary>Called by the container to</summary>
        protected internal virtual void DoDestroy()
        {
            Asda2ItemRecord record = this.m_record;
            if (this.m_owner != null)
                this.m_owner.Asda2Inventory.RemoveItemFromInventory(this);
            if (record == null)
                return;
            record.OwnerId = 0U;
            record.DeleteLater();
            this.m_record = (Asda2ItemRecord) null;
            this.Dispose();
        }

        public QuestHolderInfo QuestHolderInfo
        {
            get { return this.m_template.QuestHolderInfo; }
        }

        public bool CanGiveQuestTo(Character chr)
        {
            return this.m_owner == chr;
        }

        public void OnQuestGiverStatusQuery(Character chr)
        {
        }

        public override void Dispose(bool disposing)
        {
            this.m_owner = (Character) null;
            this.m_isInWorld = false;
            this.IsDeleted = true;
        }

        public override string ToString()
        {
            return string.Format("[{0}]{1} Amount {2} Category {3}", (object) this.ItemId,
                (object) (Asda2ItemId) this.ItemId, (object) this.Amount, (object) this.Category);
        }

        public bool IsInContext
        {
            get
            {
                Unit owner = this.Owner;
                if (owner != null)
                {
                    IContextHandler contextHandler = owner.ContextHandler;
                    if (contextHandler != null)
                        return contextHandler.IsInContext;
                }

                return false;
            }
        }

        public bool IsWeapon
        {
            get { return this.Template.IsWeapon; }
            set { }
        }

        public bool IsAccessory
        {
            get { return this.Template.IsAccessory; }
            set { }
        }

        public int BoosterId
        {
            get { return this.Template.BoosterId; }
        }

        public int PackageId
        {
            get { return this.Template.PackageId; }
        }

        public bool IsArmor
        {
            get { return this.Template.IsArmor; }
        }

        public int Deffence { get; set; }

        public int MagicDeffence { get; set; }

        protected int SocketsCount
        {
            get { return (int) this.Template.SowelSocketsCount; }
        }

        public void AddMessage(IMessage message)
        {
            Unit owner = this.Owner;
            if (owner == null)
                return;
            owner.AddMessage(message);
        }

        public void AddMessage(Action action)
        {
            Unit owner = this.Owner;
            if (owner == null)
                return;
            owner.AddMessage(action);
        }

        public bool ExecuteInContext(Action action)
        {
            Unit owner = this.Owner;
            if (owner != null)
                return owner.ExecuteInContext(action);
            return false;
        }

        public void EnsureContext()
        {
            Unit owner = this.Owner;
            if (owner == null)
                return;
            owner.EnsureContext();
        }

        public List<Asda2ItemTemplate> InsertedSowels
        {
            get
            {
                List<Asda2ItemTemplate> asda2ItemTemplateList = new List<Asda2ItemTemplate>();
                if (this.Soul1Id != 0)
                {
                    Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(this.Soul1Id);
                    if (template != null)
                        asda2ItemTemplateList.Add(template);
                }

                if (this.Soul2Id != 0)
                {
                    Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(this.Soul2Id);
                    if (template != null)
                        asda2ItemTemplateList.Add(template);
                }

                if (this.Soul3Id != 0)
                {
                    Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(this.Soul3Id);
                    if (template != null)
                        asda2ItemTemplateList.Add(template);
                }

                if (this.Soul4Id != 0)
                {
                    Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(this.Soul4Id);
                    if (template != null)
                        asda2ItemTemplateList.Add(template);
                }

                return asda2ItemTemplateList;
            }
        }

        public byte RequiredLevel
        {
            get { return (byte) this.Template.RequiredLevel; }
        }

        public bool IsRod
        {
            get { return this.Template.IsRod; }
        }

        public bool SetParametr(Asda2ItemBonusType type, short value, byte slot)
        {
            if (slot > (byte) 4)
                return false;
            switch (slot)
            {
                case 0:
                    this.Parametr1Type = type;
                    this.Parametr1Value = value;
                    break;
                case 1:
                    this.Parametr2Type = type;
                    this.Parametr2Value = value;
                    break;
                case 2:
                    this.Parametr3Type = type;
                    this.Parametr3Value = value;
                    break;
                case 3:
                    this.Parametr4Type = type;
                    this.Parametr4Value = value;
                    break;
                case 4:
                    this.Parametr5Type = type;
                    this.Parametr5Value = value;
                    break;
            }

            this.RecalculateItemParametrs();
            return true;
        }

        private void RecalculateItemParametrs()
        {
            if (!this.IsWeapon)
                return;
            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(this.Soul1Id);
            if (template == null)
            {
                this.Damages = new DamageInfo[1]
                {
                    new DamageInfo(DamageSchoolMask.Physical, 1f, 3f)
                };
            }
            else
            {
                float enchantMultiplier = CharacterFormulas.CalculateEnchantMultiplier(this.Enchant);
                float num = CharacterFormulas.CalcWeaponTypeMultiplier(this.Category,
                    this.OwningCharacter == null ? ClassId.NoClass : this.OwningCharacter.Archetype.ClassId);
                this.Damages = new DamageInfo[1]
                {
                    new DamageInfo(
                        this.Category == Asda2ItemCategory.Staff ? DamageSchoolMask.Magical : DamageSchoolMask.Physical,
                        (float) template.SowelBonusValue * enchantMultiplier * num,
                        (float) ((double) template.SowelBonusValue * (double) enchantMultiplier * 1.10000002384186) *
                        num)
                };
            }
        }

        private float OprionValueMultiplier
        {
            get
            {
                float num = 1f;
                if (this.Record.IsCrafted)
                    num += 0.5f;
                switch (this.Template.Quality)
                {
                    case Asda2ItemQuality.Yello:
                        num += 0.1f;
                        break;
                    case Asda2ItemQuality.Purple:
                        num += 0.2f;
                        break;
                    case Asda2ItemQuality.Green:
                        num += 0.35f;
                        break;
                    case Asda2ItemQuality.Orange:
                        num += 0.5f;
                        break;
                }

                return num;
            }
        }

        public void GenerateNewOptions()
        {
            float oprionValueMultiplier = this.OprionValueMultiplier;
            ItemStatBonus bonus1 = this.Template.StatGeneratorCommon.GetBonus();
            this.Parametr1Type = bonus1.Type;
            this.Parametr1Value = (short) ((double) bonus1.GetValue() * (double) oprionValueMultiplier);
            ItemStatBonus bonus2 = this.Template.StatGeneratorCommon.GetBonus();
            this.Parametr2Type = bonus2.Type;
            this.Parametr2Value = (short) ((double) bonus2.GetValue() * (double) oprionValueMultiplier);
            if (this.Record.IsCrafted)
                this.GenerateOptionsByCraft();
            if ((int) this.Enchant < CharacterFormulas.OptionStatStartsWithEnchantValue)
                return;
            this.GenerateOptionsByUpgrade();
        }

        public void GenerateOptionsByCraft()
        {
            ItemStatBonus bonus = this.Template.StatGeneratorCraft.GetBonus();
            this.Parametr3Type = bonus.Type;
            this.Parametr3Value = (short) ((double) bonus.GetValue() * (double) this.OprionValueMultiplier);
        }

        public void GenerateOptionsByUpgrade()
        {
            ItemStatBonus bonus = this.Template.StatGeneratorEnchant.GetBonus();
            this.Parametr4Type = bonus.Type;
            this.Parametr4Value = (short) ((double) bonus.GetValue() * (double) this.OprionValueMultiplier *
                                           (double) CharacterFormulas.CalculateEnchantMultiplierNotDamageItemStats(
                                               this.Enchant));
        }

        public void SetRandomAdvancedEnchant()
        {
            ItemStatBonus bonus = this.Template.StatGeneratorAdvanced.GetBonus();
            this.Parametr5Type = bonus.Type;
            this.Parametr5Value = (short) ((double) bonus.GetValue() * (double) this.OprionValueMultiplier);
        }

        public int ItemId
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.ItemId;
                return this._itemId;
            }
            set
            {
                if (this.m_record != null)
                    this.m_record.ItemId = value;
                this._itemId = value;
            }
        }

        public Character OwningCharacter
        {
            get { return this.m_owner; }
            internal set
            {
                if (this.m_owner == value)
                    return;
                this.m_owner = value;
                if (this.m_owner != null)
                {
                    this.m_isInWorld = this.m_unknown = true;
                    this.m_record.OwnerId = value.EntityId.Low;
                    this.m_record.OwnerName = value.Name;
                }
                else
                {
                    this.m_record.OwnerId = 0U;
                    this.m_record.OwnerName = "No owner.";
                }
            }
        }

        public int CountForNextSell { get; set; }

        /// <summary>The life-time of this Item in seconds</summary>
        public EntityId Creator
        {
            get { return new EntityId((ulong) this.m_record.CreatorEntityId); }
            set { this.m_record.CreatorEntityId = (long) value.Full; }
        }

        /// <summary>
        /// The Slot of this Item within its <see cref="T:WCell.RealmServer.Entities.Container">Container</see>.
        /// </summary>
        public short Slot
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.Slot;
                return this._slot;
            }
            internal set
            {
                this.m_record.Slot = value;
                this._slot = value;
            }
        }

        /// <summary>
        /// Modifies the amount of this item (size of this stack).
        /// Ensures that new value won't exceed UniqueCount.
        /// Returns how many items actually got added.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ModAmount(int value)
        {
            if (value == 0)
                return 0;
            this.Amount += value;
            return value;
        }

        /// <summary>
        /// Current amount of items in this stack.
        /// Setting the Amount to 0 will destroy the Item.
        /// Keep in mind that this is uint and thus can never become smaller than 0!
        /// </summary>
        public int Amount
        {
            get { return this.IsDeleted ? -1 : this.m_record.Amount; }
            set
            {
                if (value <= 0)
                {
                    this.m_record.Amount = 0;
                    this.Destroy();
                }
                else
                {
                    if (value - this.m_record.Amount == 0)
                        return;
                    this.m_record.Amount = value;
                }
            }
        }

        public uint Duration
        {
            get
            {
                if (!this.IsDeleted)
                    return (uint) this.m_record.Duration;
                return 0;
            }
            set { this.m_record.Duration = (int) value; }
        }

        /// <summary>
        /// Charges of the <c>UseSpell</c> of this Item.
        /// </summary>
        public bool IsAuctioned
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.IsAuctioned;
                return false;
            }
            set { this.m_record.IsAuctioned = true; }
        }

        public int AuctionPrice
        {
            get { return this.Record.AuctionPrice; }
            set { this.Record.AuctionPrice = value; }
        }

        public bool IsSoulbound
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.IsSoulBound;
                return false;
            }
            set { this.m_record.IsSoulBound = value; }
        }

        public byte Durability
        {
            get { return this.IsDeleted ? (byte) 0 : this.m_record.Durability; }
            set { this.m_record.Durability = value; }
        }

        public byte MaxDurability
        {
            get { return this.IsDeleted ? (byte) 0 : this.Template.MaxDurability; }
            protected set { this.Template.MaxDurability = value; }
        }

        public void RepairDurability()
        {
            this.Durability = this.MaxDurability;
        }

        public DamageInfo[] Damages
        {
            get { return this._damages; }
            private set { this._damages = value; }
        }

        public int BonusDamage { get; set; }

        public bool IsRanged
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_template.IsRangedWeapon;
                return false;
            }
        }

        public bool IsMelee
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_template.IsMeleeWeapon;
                return false;
            }
        }

        /// <summary>
        /// The minimum Range of this weapon
        /// TODO: temporary values
        /// </summary>
        public float MinRange
        {
            get { return 0.0f; }
        }

        /// <summary>
        /// The maximum Range of this Weapon
        /// TODO: temporary values
        /// </summary>
        public float MaxRange
        {
            get { return this.IsDeleted ? 0.0f : (float) this.m_template.AtackRange; }
        }

        /// <summary>The time in milliseconds between 2 attacks</summary>
        public int AttackTime
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_template.AttackTime;
                return 0;
            }
        }

        public Asda2ItemRecord Record
        {
            get { return this.m_record; }
        }

        public override ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Object | ObjectTypeCustom.Item; }
        }

        public Asda2InventoryType InventoryType
        {
            get
            {
                if (!this.IsDeleted)
                    return (Asda2InventoryType) this.m_record.InventoryType;
                return (Asda2InventoryType) this._inventoryType;
            }
            set
            {
                this.m_record.InventoryType = (byte) value;
                this._inventoryType = this.m_record.InventoryType;
            }
        }

        public int Soul1Id
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.Soul1Id;
                return 0;
            }
            set { this.m_record.Soul1Id = value; }
        }

        public int Soul2Id
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.Soul2Id;
                return 0;
            }
            set { this.m_record.Soul2Id = value; }
        }

        public int Soul3Id
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.Soul3Id;
                return 0;
            }
            set { this.m_record.Soul3Id = value; }
        }

        public int Soul4Id
        {
            get
            {
                if (!this.IsDeleted)
                    return this.m_record.Soul4Id;
                return 0;
            }
            set { this.m_record.Soul4Id = value; }
        }

        private bool IsValidSowel(int id)
        {
            return this.IsValidSowel(Asda2ItemMgr.GetTemplate(id));
        }

        private bool IsValidSowel(Asda2ItemTemplate sowel)
        {
            return sowel != null && sowel.Category == Asda2ItemCategory.Sowel &&
                   (this.IsValidSowelEquipSlot(sowel) && (long) sowel.RequiredLevel <= (long) this.Owner.Level);
        }

        public bool InsertSowel(Asda2Item sowel, byte slot)
        {
            if (!this.IsValidSowel(sowel.Template) || (int) slot > this.SocketsCount - 1)
                return false;
            switch (slot)
            {
                case 0:
                    this.Soul1Id = sowel.ItemId;
                    break;
                case 1:
                    this.Soul2Id = sowel.ItemId;
                    break;
                case 2:
                    this.Soul3Id = sowel.ItemId;
                    break;
                case 3:
                    this.Soul4Id = sowel.ItemId;
                    break;
            }

            this.RecalculateItemParametrs();
            return true;
        }

        private bool IsValidSowelEquipSlot(Asda2ItemTemplate sowel)
        {
            int sowelEquipmentType = (int) sowel.SowelEquipmentType;
            return this.Template.EquipmentSlot == (Asda2EquipmentSlots) sowel.SowelEquipmentType;
        }

        public byte Enchant
        {
            get { return this.IsDeleted ? (byte) 0 : this.m_record.Enchant; }
            set
            {
                if ((int) value == (int) this.Enchant)
                    return;
                this.m_record.Enchant = value;
                if ((int) this.Enchant >= CharacterFormulas.OptionStatStartsWithEnchantValue)
                    this.GenerateOptionsByUpgrade();
                this.RecalculateItemParametrs();
            }
        }

        public Asda2ItemBonusType Parametr1Type
        {
            get
            {
                if (!this.IsDeleted)
                    return (Asda2ItemBonusType) this.m_record.Parametr1Type;
                return Asda2ItemBonusType.None;
            }
            set { this.m_record.Parametr1Type = (short) value; }
        }

        public short Parametr1Value
        {
            get { return this.IsDeleted ? (short) 0 : this.m_record.Parametr1Value; }
            set { this.m_record.Parametr1Value = value; }
        }

        public Asda2ItemBonusType Parametr2Type
        {
            get
            {
                if (!this.IsDeleted)
                    return (Asda2ItemBonusType) this.m_record.Parametr2Type;
                return Asda2ItemBonusType.None;
            }
            set { this.m_record.Parametr2Type = (short) value; }
        }

        public short Parametr2Value
        {
            get { return this.IsDeleted ? (short) 0 : this.m_record.Parametr2Value; }
            set { this.m_record.Parametr2Value = value; }
        }

        public Asda2ItemBonusType Parametr3Type
        {
            get
            {
                if (!this.IsDeleted)
                    return (Asda2ItemBonusType) this.m_record.Parametr3Type;
                return Asda2ItemBonusType.None;
            }
            set { this.m_record.Parametr3Type = (short) value; }
        }

        public short Parametr3Value
        {
            get { return this.IsDeleted ? (short) 0 : this.m_record.Parametr1Value; }
            set { this.m_record.Parametr1Value = value; }
        }

        public Asda2ItemBonusType Parametr4Type
        {
            get
            {
                if (!this.IsDeleted)
                    return (Asda2ItemBonusType) this.m_record.Parametr4Type;
                return Asda2ItemBonusType.None;
            }
            set { this.m_record.Parametr4Type = (short) value; }
        }

        public short Parametr4Value
        {
            get { return this.IsDeleted ? (short) 0 : this.m_record.Parametr4Value; }
            set { this.m_record.Parametr4Value = value; }
        }

        public Asda2ItemBonusType Parametr5Type
        {
            get
            {
                if (!this.IsDeleted)
                    return (Asda2ItemBonusType) this.m_record.Parametr5Type;
                return Asda2ItemBonusType.None;
            }
            set { this.m_record.Parametr5Type = (short) value; }
        }

        public short Parametr5Value
        {
            get { return this.IsDeleted ? (short) 0 : this.m_record.Parametr5Value; }
            set { this.m_record.Parametr5Value = value; }
        }

        public ushort Weight
        {
            get { return this.IsDeleted ? (ushort) 0 : this.m_record.Weight; }
            set { this.m_record.Weight = value; }
        }

        public byte SealCount
        {
            get { return this.IsDeleted ? (byte) 0 : this.m_record.SealCount; }
            set { this.m_record.SealCount = value; }
        }

        public Asda2ItemCategory Category
        {
            get
            {
                if (!this.IsDeleted)
                    return this.Template.Category;
                return (Asda2ItemCategory) 0;
            }
        }

        public byte SowelSlots
        {
            get { return this.IsDeleted ? (byte) 0 : this.Template.SowelSocketsCount; }
        }

        public int AuctionId
        {
            get { return (int) this.Record.Guid; }
        }

        public uint RepairCost()
        {
            return CharacterFormulas.CalculteItemRepairCost(this.MaxDurability, this.Durability,
                this.Template.SellPrice, this.Enchant, (byte) this.Template.AuctionLevelCriterion,
                (byte) this.Template.Quality);
        }

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { return UpdateFieldHandler.DynamicItemFieldHandlers; }
        }

        protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
        {
            return UpdateType.Create;
        }

        public override UpdateFlags UpdateFlags
        {
            get { return UpdateFlags.Flag_0x10; }
        }

        public override void RequestUpdate()
        {
        }

        public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
        {
            return chr == this.m_owner
                ? UpdateFieldFlags.Public | UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly |
                  UpdateFieldFlags.GroupOnly
                : UpdateFieldFlags.Public;
        }

        protected override void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(2f);
        }

        public void DecreaseDurability(byte i, bool silent = false)
        {
            if ((int) this.Durability < (int) i)
            {
                this.Durability = (byte) 0;
                this.OnUnEquip();
            }
            else
                this.Durability -= i;

            if (silent)
                return;
            Asda2CharacterHandler.SendUpdateDurabilityResponse(this.OwningCharacter.Client, this);
        }
    }
}