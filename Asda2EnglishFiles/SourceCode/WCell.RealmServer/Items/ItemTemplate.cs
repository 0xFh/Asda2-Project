using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Pets;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [Serializable]
    public class ItemTemplate : IDataHolder, IMountableItem, IQuestHolderEntry
    {
        [Persistent(8)] public string[] Names;
        [NotPersistent] public Asda2ItemId ItemId;
        public ItemClass Class;
        public ItemSubClass SubClass;
        public int Unk0;
        public uint DisplayId;
        public ItemQuality Quality;
        public ItemFlags Flags;
        public ItemFlags2 Flags2;
        public uint BuyPrice;
        public uint SellPrice;
        public InventorySlotType InventorySlotType;
        public ClassMask RequiredClassMask;
        public RaceMask RequiredRaceMask;
        public uint Level;
        public uint RequiredLevel;
        public SkillId RequiredSkillId;
        public uint RequiredSkillValue;
        public SpellId RequiredProfessionId;
        public uint RequiredPvPRank;
        public uint UnknownRank;
        public FactionId RequiredFactionId;
        public StandingLevel RequiredFactionStanding;
        public int UniqueCount;
        public uint ScalingStatDistributionId;
        public uint ScalingStatValueFlags;
        public uint ItemLimitCategoryId;
        public uint HolidayId;

        /// <summary>The size of a stack of this item.</summary>
        public int MaxAmount;

        public int ContainerSlots;
        public RequiredSpellTargetType RequiredTargetType;
        public uint RequiredTargetId;
        [Persistent(10)] public StatModifier[] Mods;
        [Persistent(2)] public DamageInfo[] Damages;
        [Persistent(7)] public int[] Resistances;
        public int AttackTime;
        public ItemProjectileType ProjectileType;
        public float RangeModifier;
        public ItemBondType BondType;
        [Persistent(8)] public string[] Descriptions;
        public uint PageTextId;
        public ChatLanguage LanguageId;
        public PageMaterial PageMaterial;

        /// <summary>
        /// The Id of the Quest that will be started when this Item is used
        /// </summary>
        public uint QuestId;

        public uint LockId;
        public Material Material;
        public SheathType SheathType;
        public uint RandomPropertiesId;
        public uint RandomSuffixId;
        public uint BlockValue;
        public ItemSetId SetId;
        public int MaxDurability;
        public ZoneId ZoneId;
        public MapId MapId;
        public ItemBagFamilyMask BagFamily;
        public ToolCategory ToolCategory;
        [Persistent(3)] public SocketInfo[] Sockets;

        /// <summary>
        /// 
        /// </summary>
        public uint SocketBonusEnchantId;

        [NotPersistent] public ItemEnchantmentEntry SocketBonusEnchant;
        public uint GemPropertiesId;
        [NotPersistent] public GemProperties GemProperties;
        public int RequiredDisenchantingLevel;
        public float ArmorModifier;
        public int Duration;
        public PetFoodType m_PetFood;
        [Persistent(5)] public ItemSpell[] Spells;
        public uint StockRefillDelay;
        public int StockAmount;

        /// <summary>Amount of Items to be sold in one stack</summary>
        public int BuyStackSize;

        [NotPersistent] public uint RandomSuffixFactor;
        [NotPersistent] public SkillLine RequiredSkill;
        [NotPersistent] public ItemSubClassMask SubClassMask;
        [NotPersistent] public ItemSpell UseSpell;
        [NotPersistent] public ItemSpell TeachSpell;
        [NotPersistent] public Spell[] EquipSpells;
        [NotPersistent] public Spell[] HitSpells;
        [NotPersistent] public Spell SoulstoneSpell;
        [NotPersistent] public ItemSet Set;
        [NotPersistent] public LockEntry Lock;
        [NotPersistent] public Faction RequiredFaction;
        [NotPersistent] public Spell RequiredProfession;
        [NotPersistent] public EquipmentSlot[] EquipmentSlots;
        [NotPersistent] public bool IsAmmo;
        [NotPersistent] public bool IsBag;
        [NotPersistent] public bool IsContainer;
        [NotPersistent] public bool IsKey;
        [NotPersistent] public bool IsStackable;
        [NotPersistent] public bool IsWeapon;
        [NotPersistent] public bool IsRangedWeapon;
        [NotPersistent] public bool IsMeleeWeapon;
        [NotPersistent] public bool IsThrowable;
        [NotPersistent] public bool IsTwoHandWeapon;
        [NotPersistent] public bool IsHearthStone;
        [NotPersistent] public SkillId ItemProfession;
        [NotPersistent] public bool IsInventory;
        [NotPersistent] public bool IsCharter;
        [NotPersistent] public QuestTemplate[] CollectQuests;
        [NotPersistent] public bool HasSockets;
        [NotPersistent] public bool ConsumesAmount;
        [NotPersistent] public Func<Item> Creator;

        /// <summary>
        /// Called when an ItemRecord of this ItemTemplate has been created (if newly created or loaded from DB).
        /// That is before the actual Item object has been created.
        /// Called from the IO context if loaded from DB.
        /// </summary>
        public event Action<ItemRecord> Created;

        /// <summary>
        /// Called whenever an Item of this ItemTemplate is equipped
        /// </summary>
        public event Action<Item> Equipped;

        /// <summary>
        /// Called whenever an Item of this ItemTemplate is unequipped
        /// </summary>
        public event Action<Item> Unequipped;

        /// <summary>
        /// Called whenever an item of this ItemTemplate has been used
        /// </summary>
        public event Action<Item> Used;

        internal void NotifyCreated(ItemRecord record)
        {
            this.OnRecordCreated(record);
            Action<ItemRecord> created = this.Created;
            if (created == null)
                return;
            created(record);
        }

        internal void NotifyEquip(Item item)
        {
            Action<Item> equipped = this.Equipped;
            if (equipped == null)
                return;
            equipped(item);
        }

        internal void NotifyUnequip(Item item)
        {
            Action<Item> unequipped = this.Unequipped;
            if (unequipped == null)
                return;
            unequipped(item);
        }

        internal void NotifyUsed(Item item)
        {
            Action<Item> used = this.Used;
            if (used == null)
                return;
            used(item);
        }

        [NotPersistent]
        public string DefaultName
        {
            get { return this.Names.LocalizeWithDefaultLocale(); }
            set
            {
                if (this.Names == null)
                    this.Names = new string[8];
                this.Names[(int) RealmServerConfiguration.DefaultLocale] = value;
            }
        }

        public uint Id { get; set; }

        [NotPersistent]
        public string DefaultDescription
        {
            get { return this.Descriptions.LocalizeWithDefaultLocale(); }
            set
            {
                if (this.Names == null)
                    this.Names = new string[8];
                this.Descriptions[(int) RealmServerConfiguration.DefaultLocale] = value;
            }
        }

        public List<ItemRandomEnchantEntry> RandomPrefixes
        {
            get
            {
                if (this.RandomPropertiesId != 0U)
                    return EnchantMgr.RandomEnchantEntries.Get<List<ItemRandomEnchantEntry>>(this.RandomPropertiesId);
                return (List<ItemRandomEnchantEntry>) null;
            }
        }

        public List<ItemRandomEnchantEntry> RandomSuffixes
        {
            get
            {
                if (this.RandomPropertiesId != 0U)
                    return EnchantMgr.RandomEnchantEntries.Get<List<ItemRandomEnchantEntry>>(this.RandomSuffixId);
                return (List<ItemRandomEnchantEntry>) null;
            }
        }

        public ItemSpell GetSpell(ItemSpellTrigger trigger)
        {
            return ((IEnumerable<ItemSpell>) this.Spells).Where<ItemSpell>((Func<ItemSpell, bool>) (itemSpell =>
            {
                if (itemSpell != null && itemSpell.Trigger == trigger)
                    return itemSpell.Id != SpellId.None;
                return false;
            })).FirstOrDefault<ItemSpell>();
        }

        public int GetResistance(DamageSchool school)
        {
            return this.Resistances[(int) school];
        }

        [NotPersistent] public InventorySlotTypeMask InventorySlotMask { get; set; }

        public bool HasQuestRequirements
        {
            get
            {
                if (this.QuestHolderInfo == null)
                    return this.CollectQuests != null;
                return true;
            }
        }

        /// <summary>
        /// For templates of Containers only, checks whether the given
        /// Template may be added
        /// </summary>
        /// <param name="templ"></param>
        /// <returns></returns>
        public bool MayAddToContainer(ItemTemplate templ)
        {
            if (this.BagFamily != ItemBagFamilyMask.None)
                return templ.BagFamily.HasAnyFlag(this.BagFamily);
            return true;
        }

        public object GetId()
        {
            return (object) this.Id;
        }

        /// <summary>Set custom fields etc</summary>
        public void FinalizeDataHolder()
        {
            this.CheckId();
            ArrayUtil.Set<ItemTemplate>(ref ItemMgr.Templates, this.Id, this);
        }

        internal void InitializeTemplate()
        {
            if (this.Names == null)
                this.Names = new string[8];
            if (this.Descriptions == null)
                this.Descriptions = new string[8];
            if (this.DefaultDescription == null)
                this.DefaultDescription = "";
            if (string.IsNullOrEmpty(this.DefaultName) || this.Id == 0U)
                return;
            this.ItemId = (Asda2ItemId) this.Id;
            this.RequiredSkill = SkillHandler.Get(this.RequiredSkillId);
            this.Set = ItemMgr.GetSet(this.SetId);
            this.Lock = LockEntry.Entries.Get<LockEntry>(this.LockId);
            this.RequiredFaction = FactionMgr.Get(this.RequiredFactionId);
            this.RequiredProfession = SpellHandler.Get(this.RequiredProfessionId);
            this.SubClassMask =
                (ItemSubClassMask) (1 << (int) (this.SubClass & (ItemSubClass.WeaponDagger | ItemSubClass.WeaponThrown))
                );
            this.EquipmentSlots = ItemMgr.EquipmentSlotsByInvSlot.Get<EquipmentSlot[]>((uint) this.InventorySlotType);
            this.InventorySlotMask =
                (InventorySlotTypeMask) (1 << (int) (this.InventorySlotType &
                                                     (InventorySlotType.WeaponRanged | InventorySlotType.Cloak)));
            this.IsAmmo = this.InventorySlotType == InventorySlotType.Ammo;
            this.IsKey = this.Class == ItemClass.Key;
            this.IsBag = this.InventorySlotType == InventorySlotType.Bag;
            this.IsContainer = this.Class == ItemClass.Container || this.Class == ItemClass.Quiver;
            this.IsStackable = this.MaxAmount > 1 && this.RandomSuffixId == 0U && this.RandomPropertiesId == 0U;
            this.IsTwoHandWeapon = this.InventorySlotType == InventorySlotType.TwoHandWeapon;
            this.SetIsWeapon();
            if (this.ToolCategory != ToolCategory.None)
                ItemMgr.FirstTotemsPerCat[(uint) this.ToolCategory] = this;
            if (this.GemPropertiesId != 0U)
            {
                this.GemProperties = EnchantMgr.GetGemproperties(this.GemPropertiesId);
                if (this.GemProperties != null)
                    this.GemProperties.Enchantment.GemTemplate = this;
            }

            if (this.Sockets == null)
                this.Sockets = new SocketInfo[3];
            else if (((IEnumerable<SocketInfo>) this.Sockets).Contains<SocketInfo>(
                (Func<SocketInfo, bool>) (sock => sock.Color != SocketColor.None)))
                this.HasSockets = true;
            if (this.Damages == null)
                this.Damages = DamageInfo.EmptyArray;
            if (this.Resistances == null)
                this.Resistances = new int[7];
            if (this.SocketBonusEnchantId != 0U)
                this.SocketBonusEnchant = EnchantMgr.GetEnchantmentEntry(this.SocketBonusEnchantId);
            switch (this.Class)
            {
                case ItemClass.Weapon:
                    this.ItemProfession = ItemProfessions.WeaponSubClassProfessions.Get<SkillId>((uint) this.SubClass);
                    break;
                case ItemClass.Armor:
                    this.ItemProfession = ItemProfessions.ArmorSubClassProfessions.Get<SkillId>((uint) this.SubClass);
                    break;
            }

            int sheathType = (int) this.SheathType;
            if (this.Spells != null)
            {
                ArrayUtil.Prune<ItemSpell>(ref this.Spells);
                for (int index = 0; index < 5; ++index)
                {
                    this.Spells[index].Index = (uint) index;
                    this.Spells[index].FinalizeAfterLoad();
                }
            }
            else
                this.Spells = ItemSpell.EmptyArray;

            this.UseSpell = ((IEnumerable<ItemSpell>) this.Spells).Where<ItemSpell>(
                (Func<ItemSpell, bool>) (itemSpell =>
                {
                    if (itemSpell.Trigger == ItemSpellTrigger.Use)
                        return itemSpell.Spell != null;
                    return false;
                })).FirstOrDefault<ItemSpell>();
            if (this.UseSpell != null)
            {
                this.UseSpell.Spell.RequiredTargetType = this.RequiredTargetType;
                this.UseSpell.Spell.RequiredTargetId = this.RequiredTargetId;
            }

            this.EquipSpells = ((IEnumerable<ItemSpell>) this.Spells).Where<ItemSpell>((Func<ItemSpell, bool>) (spell =>
            {
                if (spell.Trigger == ItemSpellTrigger.Equip)
                    return spell.Spell != null;
                return false;
            })).Select<ItemSpell, Spell>((Func<ItemSpell, Spell>) (itemSpell => itemSpell.Spell)).ToArray<Spell>();
            this.SoulstoneSpell = ((IEnumerable<ItemSpell>) this.Spells).Where<ItemSpell>(
                    (Func<ItemSpell, bool>) (spell =>
                    {
                        if (spell.Trigger == ItemSpellTrigger.Soulstone)
                            return spell.Spell != null;
                        return false;
                    })).Select<ItemSpell, Spell>((Func<ItemSpell, Spell>) (itemSpell => itemSpell.Spell))
                .FirstOrDefault<Spell>();
            this.HitSpells = ((IEnumerable<ItemSpell>) this.Spells).Where<ItemSpell>((Func<ItemSpell, bool>) (spell =>
            {
                if (spell.Trigger == ItemSpellTrigger.ChanceOnHit)
                    return spell.Spell != null;
                return false;
            })).Select<ItemSpell, Spell>((Func<ItemSpell, Spell>) (itemSpell => itemSpell.Spell)).ToArray<Spell>();
            this.ConsumesAmount =
                (this.Class == ItemClass.Consumable ||
                 ((IEnumerable<ItemSpell>) this.Spells).Contains<ItemSpell>(
                     (Func<ItemSpell, bool>) (spell => spell.Trigger == ItemSpellTrigger.Consume))) &&
                (this.UseSpell == null || !this.UseSpell.HasCharges);
            this.IsHearthStone = this.UseSpell != null && this.UseSpell.Spell.IsHearthStoneSpell;
            this.IsInventory = this.InventorySlotType != InventorySlotType.None &&
                               this.InventorySlotType != InventorySlotType.Bag &&
                               this.InventorySlotType != InventorySlotType.Quiver &&
                               this.InventorySlotType != InventorySlotType.Relic;
            if (this.SetId != ItemSetId.None)
            {
                ItemSet itemSet = ItemMgr.Sets.Get<ItemSet>((uint) this.SetId);
                if (itemSet != null)
                {
                    int num = (int) ArrayUtil.Add<ItemTemplate>(ref itemSet.Templates, this);
                }
            }

            if (this.Mods != null)
                ArrayUtil.TruncVals<StatModifier>(ref this.Mods);
            else
                this.Mods = StatModifier.EmptyArray;
            this.IsCharter = this.Flags.HasFlag((Enum) ItemFlags.Charter);
            this.RandomSuffixFactor = EnchantMgr.GetRandomSuffixFactor(this);
            if (this.IsCharter)
                this.Creator = (Func<Item>) (() => (Item) new PetitionCharter());
            else if (this.IsContainer)
                this.Creator = (Func<Item>) (() => (Item) new Container());
            else
                this.Creator = (Func<Item>) (() => new Item());
        }

        /// <summary>Adds a new modifier to this Template</summary>
        public void AddMod(ItemModType modType, int value)
        {
            int num = (int) ArrayUtil.AddOnlyOne<StatModifier>(ref this.Mods, new StatModifier()
            {
                Type = modType,
                Value = value
            });
        }

        /// <summary>
        /// Returns false if the looter may not take one of these items.
        /// E.g. due to quest requirements, if this is a quest item and the looter does not need it (yet, or anymore).
        /// </summary>
        /// <param name="looter">Can be null</param>
        public bool CheckLootConstraints(Character looter)
        {
            return this.CheckQuestConstraints(looter);
        }

        public bool CheckQuestConstraints(Character looter)
        {
            if (!this.HasQuestRequirements)
                return true;
            if (looter == null ||
                this.QuestHolderInfo != null &&
                this.QuestHolderInfo.QuestStarts.Any<QuestTemplate>(
                    (Func<QuestTemplate, bool>) (quest => looter.QuestLog.HasActiveQuest(quest))) ||
                this.CollectQuests == null)
                return false;
            for (int index1 = 0; index1 < this.CollectQuests.Length; ++index1)
            {
                QuestTemplate collectQuest = this.CollectQuests[index1];
                if (collectQuest != null && looter.QuestLog.HasActiveQuest(collectQuest.Id))
                {
                    for (int index2 = 0; index2 < collectQuest.CollectableItems.Length; ++index2)
                    {
                        if (collectQuest.CollectableItems[index2].ItemId == this.ItemId &&
                            collectQuest.CollectableItems[index2].Amount >
                            looter.QuestLog.GetActiveQuest(collectQuest.Id).CollectedItems[index2])
                            return true;
                    }

                    for (int index2 = 0; index2 < collectQuest.CollectableSourceItems.Length; ++index2)
                    {
                        if (collectQuest.CollectableSourceItems[index2].ItemId == this.ItemId &&
                            collectQuest.CollectableSourceItems[index2].Amount > looter.QuestLog
                                .GetActiveQuest(collectQuest.Id).CollectedSourceItems[index2])
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns what went wrong (if anything) when the given unit tries to equip or use Items of this Template.
        /// </summary>
        public InventoryError CheckEquip(Character chr)
        {
            if (chr.GodMode)
                return InventoryError.OK;
            if ((long) chr.Level < (long) this.RequiredLevel)
                return InventoryError.YOU_MUST_REACH_LEVEL_N;
            if (this.RequiredClassMask != ClassMask.None && !this.RequiredClassMask.HasAnyFlag(chr.ClassMask))
                return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM;
            if (this.RequiredRaceMask != ~RaceMask.AllRaces1 && !this.RequiredRaceMask.HasAnyFlag(chr.RaceMask))
                return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM2;
            if (this.RequiredFaction != null)
            {
                if (chr.Faction != this.RequiredFaction)
                    return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM2;
                if (this.RequiredFactionStanding != StandingLevel.Hated &&
                    chr.Reputations.GetStandingLevel(this.RequiredFaction.ReputationIndex) >=
                    this.RequiredFactionStanding)
                    return InventoryError.ITEM_REPUTATION_NOT_ENOUGH;
            }

            if (this.RequiredSkill != null &&
                !chr.Skills.CheckSkill(this.RequiredSkill.Id, (int) this.RequiredSkillValue))
                return InventoryError.SKILL_ISNT_HIGH_ENOUGH;
            if (this.RequiredProfession != null && !chr.Spells.Contains(this.RequiredProfessionId))
                return InventoryError.NO_REQUIRED_PROFICIENCY;
            if (this.Set != null && this.Set.RequiredSkill != null &&
                !chr.Skills.CheckSkill(this.Set.RequiredSkill.Id, (int) this.Set.RequiredSkillValue))
                return InventoryError.SKILL_ISNT_HIGH_ENOUGH;
            if (this.ItemProfession != SkillId.None && !chr.Skills.Contains(this.ItemProfession))
                return InventoryError.NO_REQUIRED_PROFICIENCY;
            return this.IsWeapon && !chr.MayCarry(this.InventorySlotMask)
                ? InventoryError.CANT_DO_WHILE_DISARMED
                : InventoryError.OK;
        }

        internal void SetIsWeapon()
        {
            this.IsThrowable = this.InventorySlotType == InventorySlotType.Thrown;
            this.IsRangedWeapon = this.IsThrowable || this.InventorySlotType == InventorySlotType.WeaponRanged ||
                                  this.InventorySlotType == InventorySlotType.RangedRight;
            this.IsMeleeWeapon = this.InventorySlotType == InventorySlotType.TwoHandWeapon ||
                                 this.InventorySlotType == InventorySlotType.Weapon ||
                                 this.InventorySlotType == InventorySlotType.WeaponMainHand ||
                                 this.InventorySlotType == InventorySlotType.WeaponOffHand;
            this.IsWeapon = this.IsRangedWeapon || this.IsMeleeWeapon;
        }

        private void CheckId()
        {
            if (this.Id > 100000U)
                throw new Exception("Found item-template (" + (object) this.Id + ") with Id > " + (object) 100000U +
                                    ". Items with such a high ID would blow the item storage array.");
        }

        public ItemTemplate Template
        {
            get { return this; }
        }

        public ItemEnchantment[] Enchantments
        {
            get { return (ItemEnchantment[]) null; }
        }

        public bool IsEquipped
        {
            get { return false; }
        }

        public static IEnumerable<ItemTemplate> GetAllDataHolders()
        {
            return (IEnumerable<ItemTemplate>) ItemMgr.Templates;
        }

        /// <summary>
        /// Contains the quests that this item can start (items usually can only start one)
        /// </summary>
        public QuestHolderInfo QuestHolderInfo { get; internal set; }

        public IWorldLocation[] GetInWorldTemplates()
        {
            return (IWorldLocation[]) null;
        }

        public Item Create()
        {
            return this.Creator();
        }

        private void OnRecordCreated(ItemRecord record)
        {
            if (!this.IsCharter || record.IsNew)
                return;
            PetitionRecord.LoadRecord(record.OwnerId);
        }

        public void Dump(TextWriter writer)
        {
            this.Dump(writer, "");
        }

        public void Dump(TextWriter writer, string indent)
        {
            writer.WriteLine(indent + this.DefaultName + " (ID: " + (object) this.Id + " [" + (object) this.ItemId +
                             "])");
            indent += "\t";
            string str = indent;
            writer.WriteLine(str + "Infos:");
            indent += "\t";
            if (this.Class != ItemClass.None)
                writer.WriteLine(indent + "Class: " + (object) this.Class);
            if (this.SubClass != ItemSubClass.WeaponAxe)
                writer.WriteLine(indent + "SubClass " + (object) this.SubClass);
            if (this.DisplayId != 0U)
                writer.WriteLine(indent + "DisplayId: " + (object) this.DisplayId);
            writer.WriteLine(indent + "Quality: " + (object) this.Quality);
            if (this.Flags != ItemFlags.None)
                writer.WriteLine(indent + "Flags: " + (object) this.Flags);
            if (this.Flags2 != (ItemFlags2) 0)
                writer.WriteLine(indent + "Flags2: " + (object) this.Flags2);
            if (this.BuyPrice != 0U)
                writer.WriteLine(indent + "BuyPrice: " + Utility.FormatMoney(this.BuyPrice));
            if (this.SellPrice != 0U)
                writer.WriteLine(indent + "SellPrice: " + Utility.FormatMoney(this.SellPrice));
            if (this.Level != 0U)
                writer.WriteLine(indent + "Level: " + (object) this.Level);
            if (this.RequiredLevel != 0U)
                writer.WriteLine(indent + "RequiredLevel: " + (object) this.RequiredLevel);
            if (this.InventorySlotType != InventorySlotType.None)
                writer.WriteLine(indent + "InventorySlotType: " + (object) this.InventorySlotType);
            if (this.UniqueCount != 0)
                writer.WriteLine(indent + "UniqueCount: " + (object) this.UniqueCount);
            if (this.MaxAmount != 1)
                writer.WriteLine(indent + "MaxAmount: " + (object) this.MaxAmount);
            if (this.ContainerSlots != 0)
                writer.WriteLine(indent + "ContainerSlots: " + (object) this.ContainerSlots);
            if (this.BlockValue != 0U)
                writer.WriteLine(indent + "BlockValue: " + (object) this.BlockValue);
            List<string> collection1 = new List<string>(11);
            for (int index = 0; index < this.Mods.Length; ++index)
            {
                StatModifier mod = this.Mods[index];
                if (mod.Value != 0)
                    collection1.Add((mod.Value > 0 ? (object) "+" : (object) "").ToString() + (object) mod.Value + " " +
                                    (object) mod.Type);
            }

            if (collection1.Count > 0)
                writer.WriteLine(indent + "Modifiers: " + collection1.ToString<string>("; "));
            List<string> collection2 = new List<string>(5);
            for (int index = 0; index < this.Damages.Length; ++index)
            {
                DamageInfo damage = this.Damages[index];
                if ((double) damage.Maximum != 0.0)
                    collection2.Add(((double) damage.Minimum).ToString() + "-" + (object) damage.Maximum + " " +
                                    (object) damage.School);
            }

            if (collection2.Count > 0)
                writer.WriteLine(indent + "Damages: " + collection2.ToString<string>("; "));
            if (this.AttackTime != 0)
                writer.WriteLine(indent + "AttackTime: " + (object) this.AttackTime);
            List<string> collection3 = new List<string>(5);
            for (DamageSchool damageSchool = DamageSchool.Physical; damageSchool < DamageSchool.Count; ++damageSchool)
            {
                int resistance = this.Resistances[(int) damageSchool];
                if (resistance > 0)
                    collection3.Add((resistance > 0 ? (object) "+" : (object) "").ToString() + (object) resistance +
                                    " " + (object) damageSchool);
            }

            if (collection3.Count > 0)
                writer.WriteLine(indent + "Resistances: " + collection3.ToString<string>("; "));
            List<ItemSpell> collection4 = new List<ItemSpell>();
            foreach (ItemSpell spell in this.Spells)
            {
                if (spell.Id != SpellId.None)
                    collection4.Add(spell);
            }

            if (collection4.Count > 0)
                writer.WriteLine(indent + "Spells: " + collection4.ToString<ItemSpell>("; "));
            if (this.BondType != ItemBondType.None)
                writer.WriteLine(indent + "Binds: " + (object) this.BondType);
            if (this.PageTextId != 0U)
                writer.WriteLine(indent + "PageId: " + (object) this.PageTextId);
            if (this.PageMaterial != PageMaterial.None)
                writer.WriteLine(indent + "PageMaterial: " + (object) this.PageMaterial);
            if (this.LanguageId != ChatLanguage.Universal)
                writer.WriteLine(indent + "LanguageId: " + (object) this.LanguageId);
            if (this.LockId != 0U)
                writer.WriteLine(indent + "Lock: " + (object) this.LockId);
            if (this.Material != Material.None2)
                writer.WriteLine(indent + "Material: " + (object) this.Material);
            if (this.Duration != 0)
                writer.WriteLine(indent + "Duration: " + (object) this.Duration);
            if (this.SheathType != SheathType.None)
                writer.WriteLine(indent + "SheathType: " + (object) this.SheathType);
            if (this.RandomPropertiesId != 0U)
                writer.WriteLine(indent + "RandomPropertyId: " + (object) this.RandomPropertiesId);
            if (this.RandomSuffixId != 0U)
                writer.WriteLine(indent + "RandomSuffixId: " + (object) this.RandomSuffixId);
            if (this.SetId != ItemSetId.None)
                writer.WriteLine(indent + "Set: " + (object) this.SetId);
            if (this.MaxDurability != 0)
                writer.WriteLine(indent + "MaxDurability: " + (object) this.MaxDurability);
            if (this.MapId != MapId.Silaris)
                writer.WriteLine(indent + "Map: " + (object) this.MapId);
            if (this.ZoneId != ZoneId.None)
                writer.WriteLine(indent + "Zone: " + (object) this.ZoneId);
            if (this.BagFamily != ItemBagFamilyMask.None)
                writer.WriteLine(indent + "BagFamily: " + (object) this.BagFamily);
            if (this.ToolCategory != ToolCategory.None)
                writer.WriteLine(indent + "TotemCategory: " + (object) this.ToolCategory);
            List<string> collection5 = new List<string>(3);
            foreach (SocketInfo socket in this.Sockets)
            {
                if (socket.Color != SocketColor.None || socket.Content != 0)
                    collection5.Add(((int) socket.Color).ToString() + " (" + (object) socket.Content + ")");
            }

            if (collection5.Count > 0)
                writer.WriteLine(indent + "Sockets: " + collection5.ToString<string>("; "));
            if (this.GemProperties != null)
                writer.WriteLine(indent + "GemProperties: " + (object) this.GemProperties);
            if ((double) this.ArmorModifier != 0.0)
                writer.WriteLine(indent + "ArmorModifier: " + (object) this.ArmorModifier);
            if (this.RequiredDisenchantingLevel != -1 && this.RequiredDisenchantingLevel != 0)
                writer.WriteLine(indent + "RequiredDisenchantingLevel: " + (object) this.RequiredDisenchantingLevel);
            if (this.DefaultDescription.Length > 0)
                writer.WriteLine(indent + "Desc: " + this.DefaultDescription);
            writer.WriteLine(str + "Requirements:");
            if (this.RequiredClassMask != (ClassMask) 262143 && this.RequiredClassMask != ClassMask.AllClasses2)
                writer.WriteLine(indent + "Classes: " + (object) this.RequiredClassMask);
            if (this.RequiredRaceMask != RaceMask.AllRaces1)
                writer.WriteLine(indent + "Races: " + (object) this.RequiredRaceMask);
            if (this.RequiredSkillId != SkillId.None)
                writer.WriteLine(indent + "Skill: " + (object) this.RequiredSkillValue + " " +
                                 (object) this.RequiredSkillId);
            if (this.RequiredProfessionId != SpellId.None)
                writer.WriteLine(indent + "Profession: " + (object) this.RequiredProfessionId);
            if (this.RequiredPvPRank != 0U)
                writer.WriteLine(indent + "PvPRank: " + (object) this.RequiredPvPRank);
            if (this.UnknownRank != 0U)
                writer.WriteLine(indent + "UnknownRank: " + (object) this.UnknownRank);
            if (this.RequiredFactionId != FactionId.None)
                writer.WriteLine(indent + "Faction: " + (object) this.RequiredFactionId + " (" +
                                 (object) this.RequiredFactionStanding + ")");
            if (this.QuestId != 0U)
                writer.WriteLine(indent + "Quest: " + (object) this.QuestId);
            if (this.QuestHolderInfo == null)
                return;
            if (this.QuestHolderInfo.QuestStarts.Count > 0)
                writer.WriteLine(indent + "QuestStarts: " +
                                 this.QuestHolderInfo.QuestStarts.ToString<QuestTemplate>(", "));
            if (this.QuestHolderInfo.QuestEnds.Count <= 0)
                return;
            writer.WriteLine(indent + "QuestEnds: " + this.QuestHolderInfo.QuestEnds.ToString<QuestTemplate>(", "));
        }

        public override string ToString()
        {
            return string.Format("{0} (Id: {1}{2})", (object) this.DefaultName, (object) this.Id,
                this.InventorySlotType != InventorySlotType.None
                    ? (object) (" (" + (object) this.InventorySlotType + ")")
                    : (object) "");
        }
    }
}