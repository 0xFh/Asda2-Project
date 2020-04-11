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
    [Persistent(8)]public string[] Names;
    [NotPersistent]public Asda2ItemId ItemId;
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
    [Persistent(10)]public StatModifier[] Mods;
    [Persistent(2)]public DamageInfo[] Damages;
    [Persistent(7)]public int[] Resistances;
    public int AttackTime;
    public ItemProjectileType ProjectileType;
    public float RangeModifier;
    public ItemBondType BondType;
    [Persistent(8)]public string[] Descriptions;
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
    [Persistent(3)]public SocketInfo[] Sockets;

    /// <summary>
    /// 
    /// </summary>
    public uint SocketBonusEnchantId;

    [NotPersistent]public ItemEnchantmentEntry SocketBonusEnchant;
    public uint GemPropertiesId;
    [NotPersistent]public GemProperties GemProperties;
    public int RequiredDisenchantingLevel;
    public float ArmorModifier;
    public int Duration;
    public PetFoodType m_PetFood;
    [Persistent(5)]public ItemSpell[] Spells;
    public uint StockRefillDelay;
    public int StockAmount;

    /// <summary>Amount of Items to be sold in one stack</summary>
    public int BuyStackSize;

    [NotPersistent]public uint RandomSuffixFactor;
    [NotPersistent]public SkillLine RequiredSkill;
    [NotPersistent]public ItemSubClassMask SubClassMask;
    [NotPersistent]public ItemSpell UseSpell;
    [NotPersistent]public ItemSpell TeachSpell;
    [NotPersistent]public Spell[] EquipSpells;
    [NotPersistent]public Spell[] HitSpells;
    [NotPersistent]public Spell SoulstoneSpell;
    [NotPersistent]public ItemSet Set;
    [NotPersistent]public LockEntry Lock;
    [NotPersistent]public Faction RequiredFaction;
    [NotPersistent]public Spell RequiredProfession;
    [NotPersistent]public EquipmentSlot[] EquipmentSlots;
    [NotPersistent]public bool IsAmmo;
    [NotPersistent]public bool IsBag;
    [NotPersistent]public bool IsContainer;
    [NotPersistent]public bool IsKey;
    [NotPersistent]public bool IsStackable;
    [NotPersistent]public bool IsWeapon;
    [NotPersistent]public bool IsRangedWeapon;
    [NotPersistent]public bool IsMeleeWeapon;
    [NotPersistent]public bool IsThrowable;
    [NotPersistent]public bool IsTwoHandWeapon;
    [NotPersistent]public bool IsHearthStone;
    [NotPersistent]public SkillId ItemProfession;
    [NotPersistent]public bool IsInventory;
    [NotPersistent]public bool IsCharter;
    [NotPersistent]public QuestTemplate[] CollectQuests;
    [NotPersistent]public bool HasSockets;
    [NotPersistent]public bool ConsumesAmount;
    [NotPersistent]public Func<Item> Creator;

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
      OnRecordCreated(record);
      Action<ItemRecord> created = Created;
      if(created == null)
        return;
      created(record);
    }

    internal void NotifyEquip(Item item)
    {
      Action<Item> equipped = Equipped;
      if(equipped == null)
        return;
      equipped(item);
    }

    internal void NotifyUnequip(Item item)
    {
      Action<Item> unequipped = Unequipped;
      if(unequipped == null)
        return;
      unequipped(item);
    }

    internal void NotifyUsed(Item item)
    {
      Action<Item> used = Used;
      if(used == null)
        return;
      used(item);
    }

    [NotPersistent]
    public string DefaultName
    {
      get { return Names.LocalizeWithDefaultLocale(); }
      set
      {
        if(Names == null)
          Names = new string[8];
        Names[(int) RealmServerConfiguration.DefaultLocale] = value;
      }
    }

    public uint Id { get; set; }

    [NotPersistent]
    public string DefaultDescription
    {
      get { return Descriptions.LocalizeWithDefaultLocale(); }
      set
      {
        if(Names == null)
          Names = new string[8];
        Descriptions[(int) RealmServerConfiguration.DefaultLocale] = value;
      }
    }

    public List<ItemRandomEnchantEntry> RandomPrefixes
    {
      get
      {
        if(RandomPropertiesId != 0U)
          return EnchantMgr.RandomEnchantEntries.Get(RandomPropertiesId);
        return null;
      }
    }

    public List<ItemRandomEnchantEntry> RandomSuffixes
    {
      get
      {
        if(RandomPropertiesId != 0U)
          return EnchantMgr.RandomEnchantEntries.Get(RandomSuffixId);
        return null;
      }
    }

    public ItemSpell GetSpell(ItemSpellTrigger trigger)
    {
      return Spells.Where(itemSpell =>
      {
        if(itemSpell != null && itemSpell.Trigger == trigger)
          return itemSpell.Id != SpellId.None;
        return false;
      }).FirstOrDefault();
    }

    public int GetResistance(DamageSchool school)
    {
      return Resistances[(int) school];
    }

    [NotPersistent]
    public InventorySlotTypeMask InventorySlotMask { get; set; }

    public bool HasQuestRequirements
    {
      get
      {
        if(QuestHolderInfo == null)
          return CollectQuests != null;
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
      if(BagFamily != ItemBagFamilyMask.None)
        return templ.BagFamily.HasAnyFlag(BagFamily);
      return true;
    }

    public object GetId()
    {
      return Id;
    }

    /// <summary>Set custom fields etc</summary>
    public void FinalizeDataHolder()
    {
      CheckId();
      ArrayUtil.Set(ref ItemMgr.Templates, Id, this);
    }

    internal void InitializeTemplate()
    {
      if(Names == null)
        Names = new string[8];
      if(Descriptions == null)
        Descriptions = new string[8];
      if(DefaultDescription == null)
        DefaultDescription = "";
      if(string.IsNullOrEmpty(DefaultName) || Id == 0U)
        return;
      ItemId = (Asda2ItemId) Id;
      RequiredSkill = SkillHandler.Get(RequiredSkillId);
      Set = ItemMgr.GetSet(SetId);
      Lock = LockEntry.Entries.Get(LockId);
      RequiredFaction = FactionMgr.Get(RequiredFactionId);
      RequiredProfession = SpellHandler.Get(RequiredProfessionId);
      SubClassMask =
        (ItemSubClassMask) (1 << (int) (SubClass & (ItemSubClass.WeaponDagger | ItemSubClass.WeaponThrown))
        );
      EquipmentSlots = ItemMgr.EquipmentSlotsByInvSlot.Get((uint) InventorySlotType);
      InventorySlotMask =
        (InventorySlotTypeMask) (1 << (int) (InventorySlotType &
                                             (InventorySlotType.WeaponRanged | InventorySlotType.Cloak)));
      IsAmmo = InventorySlotType == InventorySlotType.Ammo;
      IsKey = Class == ItemClass.Key;
      IsBag = InventorySlotType == InventorySlotType.Bag;
      IsContainer = Class == ItemClass.Container || Class == ItemClass.Quiver;
      IsStackable = MaxAmount > 1 && RandomSuffixId == 0U && RandomPropertiesId == 0U;
      IsTwoHandWeapon = InventorySlotType == InventorySlotType.TwoHandWeapon;
      SetIsWeapon();
      if(ToolCategory != ToolCategory.None)
        ItemMgr.FirstTotemsPerCat[(uint) ToolCategory] = this;
      if(GemPropertiesId != 0U)
      {
        GemProperties = EnchantMgr.GetGemproperties(GemPropertiesId);
        if(GemProperties != null)
          GemProperties.Enchantment.GemTemplate = this;
      }

      if(Sockets == null)
        Sockets = new SocketInfo[3];
      else if(Sockets.Contains(
        sock => sock.Color != SocketColor.None))
        HasSockets = true;
      if(Damages == null)
        Damages = DamageInfo.EmptyArray;
      if(Resistances == null)
        Resistances = new int[7];
      if(SocketBonusEnchantId != 0U)
        SocketBonusEnchant = EnchantMgr.GetEnchantmentEntry(SocketBonusEnchantId);
      switch(Class)
      {
        case ItemClass.Weapon:
          ItemProfession = ItemProfessions.WeaponSubClassProfessions.Get((uint) SubClass);
          break;
        case ItemClass.Armor:
          ItemProfession = ItemProfessions.ArmorSubClassProfessions.Get((uint) SubClass);
          break;
      }

      int sheathType = (int) SheathType;
      if(Spells != null)
      {
        ArrayUtil.Prune(ref Spells);
        for(int index = 0; index < 5; ++index)
        {
          Spells[index].Index = (uint) index;
          Spells[index].FinalizeAfterLoad();
        }
      }
      else
        Spells = ItemSpell.EmptyArray;

      UseSpell = Spells.Where(
        itemSpell =>
        {
          if(itemSpell.Trigger == ItemSpellTrigger.Use)
            return itemSpell.Spell != null;
          return false;
        }).FirstOrDefault();
      if(UseSpell != null)
      {
        UseSpell.Spell.RequiredTargetType = RequiredTargetType;
        UseSpell.Spell.RequiredTargetId = RequiredTargetId;
      }

      EquipSpells = Spells.Where(spell =>
      {
        if(spell.Trigger == ItemSpellTrigger.Equip)
          return spell.Spell != null;
        return false;
      }).Select(itemSpell => itemSpell.Spell).ToArray();
      SoulstoneSpell = Spells.Where(
          spell =>
          {
            if(spell.Trigger == ItemSpellTrigger.Soulstone)
              return spell.Spell != null;
            return false;
          }).Select(itemSpell => itemSpell.Spell)
        .FirstOrDefault();
      HitSpells = Spells.Where(spell =>
      {
        if(spell.Trigger == ItemSpellTrigger.ChanceOnHit)
          return spell.Spell != null;
        return false;
      }).Select(itemSpell => itemSpell.Spell).ToArray();
      ConsumesAmount =
        (Class == ItemClass.Consumable ||
         Spells.Contains(
           spell => spell.Trigger == ItemSpellTrigger.Consume)) &&
        (UseSpell == null || !UseSpell.HasCharges);
      IsHearthStone = UseSpell != null && UseSpell.Spell.IsHearthStoneSpell;
      IsInventory = InventorySlotType != InventorySlotType.None &&
                    InventorySlotType != InventorySlotType.Bag &&
                    InventorySlotType != InventorySlotType.Quiver &&
                    InventorySlotType != InventorySlotType.Relic;
      if(SetId != ItemSetId.None)
      {
        ItemSet itemSet = ItemMgr.Sets.Get((uint) SetId);
        if(itemSet != null)
        {
          int num = (int) ArrayUtil.Add(ref itemSet.Templates, this);
        }
      }

      if(Mods != null)
        ArrayUtil.TruncVals(ref Mods);
      else
        Mods = StatModifier.EmptyArray;
      IsCharter = Flags.HasFlag(ItemFlags.Charter);
      RandomSuffixFactor = EnchantMgr.GetRandomSuffixFactor(this);
      if(IsCharter)
        Creator = () => (Item) new PetitionCharter();
      else if(IsContainer)
        Creator = () => (Item) new Container();
      else
        Creator = () => new Item();
    }

    /// <summary>Adds a new modifier to this Template</summary>
    public void AddMod(ItemModType modType, int value)
    {
      int num = (int) ArrayUtil.AddOnlyOne(ref Mods, new StatModifier
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
      return CheckQuestConstraints(looter);
    }

    public bool CheckQuestConstraints(Character looter)
    {
      if(!HasQuestRequirements)
        return true;
      if(looter == null ||
         QuestHolderInfo != null &&
         QuestHolderInfo.QuestStarts.Any(
           quest => looter.QuestLog.HasActiveQuest(quest)) ||
         CollectQuests == null)
        return false;
      for(int index1 = 0; index1 < CollectQuests.Length; ++index1)
      {
        QuestTemplate collectQuest = CollectQuests[index1];
        if(collectQuest != null && looter.QuestLog.HasActiveQuest(collectQuest.Id))
        {
          for(int index2 = 0; index2 < collectQuest.CollectableItems.Length; ++index2)
          {
            if(collectQuest.CollectableItems[index2].ItemId == ItemId &&
               collectQuest.CollectableItems[index2].Amount >
               looter.QuestLog.GetActiveQuest(collectQuest.Id).CollectedItems[index2])
              return true;
          }

          for(int index2 = 0; index2 < collectQuest.CollectableSourceItems.Length; ++index2)
          {
            if(collectQuest.CollectableSourceItems[index2].ItemId == ItemId &&
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
      if(chr.GodMode)
        return InventoryError.OK;
      if(chr.Level < RequiredLevel)
        return InventoryError.YOU_MUST_REACH_LEVEL_N;
      if(RequiredClassMask != ClassMask.None && !RequiredClassMask.HasAnyFlag(chr.ClassMask))
        return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM;
      if(RequiredRaceMask != ~RaceMask.AllRaces1 && !RequiredRaceMask.HasAnyFlag(chr.RaceMask))
        return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM2;
      if(RequiredFaction != null)
      {
        if(chr.Faction != RequiredFaction)
          return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM2;
        if(RequiredFactionStanding != StandingLevel.Hated &&
           chr.Reputations.GetStandingLevel(RequiredFaction.ReputationIndex) >=
           RequiredFactionStanding)
          return InventoryError.ITEM_REPUTATION_NOT_ENOUGH;
      }

      if(RequiredSkill != null &&
         !chr.Skills.CheckSkill(RequiredSkill.Id, (int) RequiredSkillValue))
        return InventoryError.SKILL_ISNT_HIGH_ENOUGH;
      if(RequiredProfession != null && !chr.Spells.Contains(RequiredProfessionId))
        return InventoryError.NO_REQUIRED_PROFICIENCY;
      if(Set != null && Set.RequiredSkill != null &&
         !chr.Skills.CheckSkill(Set.RequiredSkill.Id, (int) Set.RequiredSkillValue))
        return InventoryError.SKILL_ISNT_HIGH_ENOUGH;
      if(ItemProfession != SkillId.None && !chr.Skills.Contains(ItemProfession))
        return InventoryError.NO_REQUIRED_PROFICIENCY;
      return IsWeapon && !chr.MayCarry(InventorySlotMask)
        ? InventoryError.CANT_DO_WHILE_DISARMED
        : InventoryError.OK;
    }

    internal void SetIsWeapon()
    {
      IsThrowable = InventorySlotType == InventorySlotType.Thrown;
      IsRangedWeapon = IsThrowable || InventorySlotType == InventorySlotType.WeaponRanged ||
                       InventorySlotType == InventorySlotType.RangedRight;
      IsMeleeWeapon = InventorySlotType == InventorySlotType.TwoHandWeapon ||
                      InventorySlotType == InventorySlotType.Weapon ||
                      InventorySlotType == InventorySlotType.WeaponMainHand ||
                      InventorySlotType == InventorySlotType.WeaponOffHand;
      IsWeapon = IsRangedWeapon || IsMeleeWeapon;
    }

    private void CheckId()
    {
      if(Id > 100000U)
        throw new Exception("Found item-template (" + Id + ") with Id > " + 100000U +
                            ". Items with such a high ID would blow the item storage array.");
    }

    public ItemTemplate Template
    {
      get { return this; }
    }

    public ItemEnchantment[] Enchantments
    {
      get { return null; }
    }

    public bool IsEquipped
    {
      get { return false; }
    }

    public static IEnumerable<ItemTemplate> GetAllDataHolders()
    {
      return ItemMgr.Templates;
    }

    /// <summary>
    /// Contains the quests that this item can start (items usually can only start one)
    /// </summary>
    public QuestHolderInfo QuestHolderInfo { get; internal set; }

    public IWorldLocation[] GetInWorldTemplates()
    {
      return null;
    }

    public Item Create()
    {
      return Creator();
    }

    private void OnRecordCreated(ItemRecord record)
    {
      if(!IsCharter || record.IsNew)
        return;
      PetitionRecord.LoadRecord(record.OwnerId);
    }

    public void Dump(TextWriter writer)
    {
      Dump(writer, "");
    }

    public void Dump(TextWriter writer, string indent)
    {
      writer.WriteLine(indent + DefaultName + " (ID: " + Id + " [" + ItemId +
                       "])");
      indent += "\t";
      string str = indent;
      writer.WriteLine(str + "Infos:");
      indent += "\t";
      if(Class != ItemClass.None)
        writer.WriteLine(indent + "Class: " + Class);
      if(SubClass != ItemSubClass.WeaponAxe)
        writer.WriteLine(indent + "SubClass " + SubClass);
      if(DisplayId != 0U)
        writer.WriteLine(indent + "DisplayId: " + DisplayId);
      writer.WriteLine(indent + "Quality: " + Quality);
      if(Flags != ItemFlags.None)
        writer.WriteLine(indent + "Flags: " + Flags);
      if(Flags2 != 0)
        writer.WriteLine(indent + "Flags2: " + Flags2);
      if(BuyPrice != 0U)
        writer.WriteLine(indent + "BuyPrice: " + Utility.FormatMoney(BuyPrice));
      if(SellPrice != 0U)
        writer.WriteLine(indent + "SellPrice: " + Utility.FormatMoney(SellPrice));
      if(Level != 0U)
        writer.WriteLine(indent + "Level: " + Level);
      if(RequiredLevel != 0U)
        writer.WriteLine(indent + "RequiredLevel: " + RequiredLevel);
      if(InventorySlotType != InventorySlotType.None)
        writer.WriteLine(indent + "InventorySlotType: " + InventorySlotType);
      if(UniqueCount != 0)
        writer.WriteLine(indent + "UniqueCount: " + UniqueCount);
      if(MaxAmount != 1)
        writer.WriteLine(indent + "MaxAmount: " + MaxAmount);
      if(ContainerSlots != 0)
        writer.WriteLine(indent + "ContainerSlots: " + ContainerSlots);
      if(BlockValue != 0U)
        writer.WriteLine(indent + "BlockValue: " + BlockValue);
      List<string> collection1 = new List<string>(11);
      for(int index = 0; index < Mods.Length; ++index)
      {
        StatModifier mod = Mods[index];
        if(mod.Value != 0)
          collection1.Add((mod.Value > 0 ? "+" : "") + mod.Value + " " +
                          mod.Type);
      }

      if(collection1.Count > 0)
        writer.WriteLine(indent + "Modifiers: " + collection1.ToString("; "));
      List<string> collection2 = new List<string>(5);
      for(int index = 0; index < Damages.Length; ++index)
      {
        DamageInfo damage = Damages[index];
        if(damage.Maximum != 0.0)
          collection2.Add(((double) damage.Minimum) + "-" + damage.Maximum + " " +
                          damage.School);
      }

      if(collection2.Count > 0)
        writer.WriteLine(indent + "Damages: " + collection2.ToString("; "));
      if(AttackTime != 0)
        writer.WriteLine(indent + "AttackTime: " + AttackTime);
      List<string> collection3 = new List<string>(5);
      for(DamageSchool damageSchool = DamageSchool.Physical; damageSchool < DamageSchool.Count; ++damageSchool)
      {
        int resistance = Resistances[(int) damageSchool];
        if(resistance > 0)
          collection3.Add((resistance > 0 ? "+" : "") + resistance +
                          " " + damageSchool);
      }

      if(collection3.Count > 0)
        writer.WriteLine(indent + "Resistances: " + collection3.ToString("; "));
      List<ItemSpell> collection4 = new List<ItemSpell>();
      foreach(ItemSpell spell in Spells)
      {
        if(spell.Id != SpellId.None)
          collection4.Add(spell);
      }

      if(collection4.Count > 0)
        writer.WriteLine(indent + "Spells: " + collection4.ToString("; "));
      if(BondType != ItemBondType.None)
        writer.WriteLine(indent + "Binds: " + BondType);
      if(PageTextId != 0U)
        writer.WriteLine(indent + "PageId: " + PageTextId);
      if(PageMaterial != PageMaterial.None)
        writer.WriteLine(indent + "PageMaterial: " + PageMaterial);
      if(LanguageId != ChatLanguage.Universal)
        writer.WriteLine(indent + "LanguageId: " + LanguageId);
      if(LockId != 0U)
        writer.WriteLine(indent + "Lock: " + LockId);
      if(Material != Material.None2)
        writer.WriteLine(indent + "Material: " + Material);
      if(Duration != 0)
        writer.WriteLine(indent + "Duration: " + Duration);
      if(SheathType != SheathType.None)
        writer.WriteLine(indent + "SheathType: " + SheathType);
      if(RandomPropertiesId != 0U)
        writer.WriteLine(indent + "RandomPropertyId: " + RandomPropertiesId);
      if(RandomSuffixId != 0U)
        writer.WriteLine(indent + "RandomSuffixId: " + RandomSuffixId);
      if(SetId != ItemSetId.None)
        writer.WriteLine(indent + "Set: " + SetId);
      if(MaxDurability != 0)
        writer.WriteLine(indent + "MaxDurability: " + MaxDurability);
      if(MapId != MapId.Silaris)
        writer.WriteLine(indent + "Map: " + MapId);
      if(ZoneId != ZoneId.None)
        writer.WriteLine(indent + "Zone: " + ZoneId);
      if(BagFamily != ItemBagFamilyMask.None)
        writer.WriteLine(indent + "BagFamily: " + BagFamily);
      if(ToolCategory != ToolCategory.None)
        writer.WriteLine(indent + "TotemCategory: " + ToolCategory);
      List<string> collection5 = new List<string>(3);
      foreach(SocketInfo socket in Sockets)
      {
        if(socket.Color != SocketColor.None || socket.Content != 0)
          collection5.Add(((int) socket.Color) + " (" + socket.Content + ")");
      }

      if(collection5.Count > 0)
        writer.WriteLine(indent + "Sockets: " + collection5.ToString("; "));
      if(GemProperties != null)
        writer.WriteLine(indent + "GemProperties: " + GemProperties);
      if(ArmorModifier != 0.0)
        writer.WriteLine(indent + "ArmorModifier: " + ArmorModifier);
      if(RequiredDisenchantingLevel != -1 && RequiredDisenchantingLevel != 0)
        writer.WriteLine(indent + "RequiredDisenchantingLevel: " + RequiredDisenchantingLevel);
      if(DefaultDescription.Length > 0)
        writer.WriteLine(indent + "Desc: " + DefaultDescription);
      writer.WriteLine(str + "Requirements:");
      if(RequiredClassMask != (ClassMask) 262143 && RequiredClassMask != ClassMask.AllClasses2)
        writer.WriteLine(indent + "Classes: " + RequiredClassMask);
      if(RequiredRaceMask != RaceMask.AllRaces1)
        writer.WriteLine(indent + "Races: " + RequiredRaceMask);
      if(RequiredSkillId != SkillId.None)
        writer.WriteLine(indent + "Skill: " + RequiredSkillValue + " " +
                         RequiredSkillId);
      if(RequiredProfessionId != SpellId.None)
        writer.WriteLine(indent + "Profession: " + RequiredProfessionId);
      if(RequiredPvPRank != 0U)
        writer.WriteLine(indent + "PvPRank: " + RequiredPvPRank);
      if(UnknownRank != 0U)
        writer.WriteLine(indent + "UnknownRank: " + UnknownRank);
      if(RequiredFactionId != FactionId.None)
        writer.WriteLine(indent + "Faction: " + RequiredFactionId + " (" +
                         RequiredFactionStanding + ")");
      if(QuestId != 0U)
        writer.WriteLine(indent + "Quest: " + QuestId);
      if(QuestHolderInfo == null)
        return;
      if(QuestHolderInfo.QuestStarts.Count > 0)
        writer.WriteLine(indent + "QuestStarts: " +
                         QuestHolderInfo.QuestStarts.ToString(", "));
      if(QuestHolderInfo.QuestEnds.Count <= 0)
        return;
      writer.WriteLine(indent + "QuestEnds: " + QuestHolderInfo.QuestEnds.ToString(", "));
    }

    public override string ToString()
    {
      return string.Format("{0} (Id: {1}{2})", DefaultName, Id,
        InventorySlotType != InventorySlotType.None
          ? " (" + InventorySlotType + ")"
          : "");
    }
  }
}