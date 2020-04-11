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
      get { return UpdateFieldInfos; }
    }

    public override ObjectTypeId ObjectTypeId
    {
      get { return ObjectTypeId.Item; }
    }

    public static Asda2Item CreateItem(int templateId, Character owner, int amount)
    {
      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(templateId);
      if(template != null)
        return CreateItem(template, owner, amount);
      return null;
    }

    public static Asda2Item CreateItem(Asda2ItemId templateId, Character owner, int amount)
    {
      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(templateId);
      if(template != null)
        return CreateItem(template, owner, amount);
      return null;
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
      if(template == null)
      {
        log.Warn("{0} had an ItemRecord with invalid ItemId: {1}", owner, record);
        return null;
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
      m_record = Asda2ItemRecord.CreateRecord(template);
      Type |= ObjectTypes.Item;
      m_template = template;
      Durability = m_template.MaxDurability;
      MaxDurability = m_template.MaxDurability;
      Amount = amount;
      OwningCharacter = owner;
      ItemId = (int) template.ItemId;
      EntityId = new EntityId((uint) m_record.Guid, HighId.Item);
      GenerateNewOptions();
      RecalculateItemParametrs();
      template.NotifyCreated(m_record);
      OnInit();
    }

    /// <summary>Loads an already created item</summary>
    internal void LoadItem(Asda2ItemRecord record, Character owner, Asda2ItemTemplate template)
    {
      m_record = record;
      OwningCharacter = owner;
      LoadItem(record, template);
    }

    /// <summary>Loads an already created item without owner</summary>
    /// <param name="record"></param>
    /// <param name="template"></param>
    internal void LoadItem(Asda2ItemRecord record, Asda2ItemTemplate template)
    {
      m_record = record;
      m_template = template;
      EntryId = m_template.Id;
      ItemId = (int) template.ItemId;
      Type |= ObjectTypes.Item;
      _slot = record.Slot;
      _inventoryType = record.InventoryType;
      SetInt32(ItemFields.DURABILITY, record.Durability);
      SetInt32(ItemFields.DURATION, record.Duration);
      SetInt32(ItemFields.STACK_COUNT, record.Amount);
      MaxDurability = m_template.MaxDurability;
      RecalculateItemParametrs();
      OnLoad();
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
      get { return m_template; }
    }

    public LockEntry Lock
    {
      get { return m_template.Lock; }
    }

    public override bool IsInWorld
    {
      get { return m_isInWorld; }
    }

    /// <summary>Whether this object has already been deleted.</summary>
    public bool IsDeleted { get; internal set; }

    /// <summary>Checks whether this Item can currently be used</summary>
    public bool CanBeUsed
    {
      get
      {
        if(MaxDurability == 0 || Durability > 0)
          return m_loot == null;
        return false;
      }
    }

    /// <summary>The name of this item</summary>
    public string Name
    {
      get
      {
        if(m_template != null)
          return m_template.Name;
        return "";
      }
    }

    public bool CanBeTraded
    {
      get
      {
        if(m_template.MaxDurability != 0)
          return Durability > 0;
        return true;
      }
    }

    /// <summary>See IUsable.Owner</summary>
    public Unit Owner
    {
      get { return m_owner; }
    }

    /// <summary>Whether this Item is currently equipped.</summary>
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
      get { return InventoryType == Asda2InventoryType.Equipment; }
    }

    /// <summary>Wheter this item's bonuses are applied</summary>
    public bool IsApplied { get; private set; }

    /// <summary>
    /// Called when this Item was added to someone's inventory
    /// </summary>
    protected internal void OnAdd()
    {
      if(m_template.BondType != ItemBondType.OnPickup && m_template.BondType != ItemBondType.Quest)
        return;
      IsSoulbound = true;
    }

    /// <summary>
    /// Saves all recent changes that were made to this Item to the DB
    /// </summary>
    public void Save()
    {
      if(IsDeleted)
      {
        LogUtil.ErrorException(
          new InvalidOperationException("Trying to save deleted Item: " + this));
      }
      else
      {
        try
        {
          m_record.SaveAndFlush();
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex,
            string.Format("failed to save item, item {0} acc {1}[{2}]", Name,
              OwningCharacter == null ? "null" : OwningCharacter.Name,
              (uint) (OwningCharacter == null ? 999 : (int) OwningCharacter.AccId)));
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
      Amount -= amount;
      return CreateItem(m_template, OwningCharacter, amount);
    }

    /// <summary>TODO: Random properties</summary>
    public bool CanStackWith(Asda2Item otherItem)
    {
      if(m_template.IsStackable)
        return m_template == otherItem.m_template;
      return false;
    }

    /// <summary>A chest was looted empty</summary>
    public override void OnFinishedLooting()
    {
      Destroy();
    }

    public override uint GetLootId(Asda2LootEntryType type)
    {
      return m_template.Id;
    }

    /// <summary>
    /// Called when this Item gets equipped.
    /// Requires map context.
    /// </summary>
    public void OnEquip()
    {
      if(IsApplied)
        return;
      IsApplied = true;
      RecalculateItemParametrs();
      int slot = Slot;
      Character owningCharacter = OwningCharacter;
      if(Soul1Id != 0)
        ProcessAddSoul(Soul1Id);
      if(Soul2Id != 0)
        ProcessAddSoul(Soul2Id);
      if(Soul3Id != 0)
        ProcessAddSoul(Soul3Id);
      if(Soul4Id != 0)
        ProcessAddSoul(Soul4Id);
      if(Parametr1Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr1Type, Parametr1Value);
      if(Parametr2Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr2Type, Parametr2Value);
      if(Parametr3Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr3Type, Parametr3Value);
      if(Parametr4Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr4Type, Parametr4Value);
      if(Parametr5Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr5Type, Parametr5Value);
      if(Category == Asda2ItemCategory.RodFishingSkill)
        OwningCharacter.ApplyStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RodGauge)
        OwningCharacter.ApplyStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RodFishingSkillAndGauge)
      {
        OwningCharacter.ApplyStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
        OwningCharacter.ApplyStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
      }
      else if(Category == Asda2ItemCategory.NacklessMDef || Category == Asda2ItemCategory.RingMDef)
        OwningCharacter.ApplyStatMod(ItemModType.Asda2MagicDefence,
          (int) (Template.ValueOnUse *
                 (double) CharacterFormulas.ItemsMagicDeffenceMultiplier));
      else if(Category == Asda2ItemCategory.NacklessCriticalChance)
      {
        OwningCharacter.ApplyStatMod(ItemModType.CriticalStrikeRating, Template.ValueOnUse);
        OwningCharacter.ApplyStatMod(ItemModType.SpellCriticalStrikeRating, Template.ValueOnUse);
      }
      else if(Category == Asda2ItemCategory.NacklessHealth)
        OwningCharacter.ApplyStatMod(ItemModType.Health, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.NacklessMana)
        OwningCharacter.ApplyStatMod(ItemModType.Power, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RingMaxDef)
        OwningCharacter.ApplyStatMod(ItemModType.Asda2Defence, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RingMaxMAtack)
        OwningCharacter.ApplyStatMod(ItemModType.MagicDamage, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RingMaxAtack)
        OwningCharacter.ApplyStatMod(ItemModType.Damage, Template.ValueOnUse);

      IsSoulbound = true;
      if(m_template.EquipmentSlot == Asda2EquipmentSlots.Shild)
        owningCharacter.UpdateBlockChance();
      SetItemDataRecord setItemRecord = SetItemManager.GetSetItemRecord(ItemId);
      if(setItemRecord != null)
      {
        if(!OwningCharacter.AppliedSets.ContainsKey(setItemRecord.Id))
        {
          OwningCharacter.AppliedSets.Add(setItemRecord.Id, 1);
        }
        else
        {
          Dictionary<int, byte> appliedSets;
          int id;
          (appliedSets = OwningCharacter.AppliedSets)[id = setItemRecord.Id] =
            (byte) (appliedSets[id] + 1U);
        }

        AddSetBonus(setItemRecord.GetBonus(OwningCharacter.AppliedSets[setItemRecord.Id]));
      }

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
      if(!IsApplied)
        return;
      IsApplied = false;
      if(Soul1Id != 0)
        ProcessRemoveSoul(Soul1Id);
      if(Soul2Id != 0)
        ProcessRemoveSoul(Soul2Id);
      if(Soul3Id != 0)
        ProcessRemoveSoul(Soul3Id);
      if(Soul4Id != 0)
        ProcessRemoveSoul(Soul4Id);
      if(Parametr1Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr1Type, -Parametr1Value);
      if(Parametr2Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr2Type, -Parametr2Value);
      if(Parametr3Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr3Type, -Parametr3Value);
      if(Parametr4Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr4Type, -Parametr4Value);
      if(Parametr5Type != Asda2ItemBonusType.None)
        ModifyStat(Parametr5Type, -Parametr5Value);
      if(Category == Asda2ItemCategory.RodFishingSkill)
        OwningCharacter.RemoveStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RodGauge)
        OwningCharacter.RemoveStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RodFishingSkillAndGauge)
      {
        OwningCharacter.RemoveStatMod(ItemModType.FishingSkill, Template.ValueOnUse);
        OwningCharacter.RemoveStatMod(ItemModType.FishingGauge, Template.ValueOnUse);
      }
      else if(Category == Asda2ItemCategory.NacklessMDef || Category == Asda2ItemCategory.RingMDef)
        OwningCharacter.RemoveStatMod(ItemModType.Asda2MagicDefence,
          (int) (Template.ValueOnUse *
                 (double) CharacterFormulas.ItemsMagicDeffenceMultiplier));
      else if(Category == Asda2ItemCategory.NacklessCriticalChance)
      {
        OwningCharacter.RemoveStatMod(ItemModType.CriticalStrikeRating, Template.ValueOnUse);
        OwningCharacter.RemoveStatMod(ItemModType.SpellCriticalStrikeRating, Template.ValueOnUse);
      }
      else if(Category == Asda2ItemCategory.NacklessHealth)
        OwningCharacter.RemoveStatMod(ItemModType.Health, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.NacklessMana)
        OwningCharacter.RemoveStatMod(ItemModType.Power, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RingMaxDef)
        OwningCharacter.RemoveStatMod(ItemModType.Asda2Defence, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RingMaxMAtack)
        OwningCharacter.RemoveStatMod(ItemModType.MagicDamage, Template.ValueOnUse);
      else if(Category == Asda2ItemCategory.RingMaxAtack)
        OwningCharacter.RemoveStatMod(ItemModType.Damage, Template.ValueOnUse);

      if(m_template.EquipmentSlot == Asda2EquipmentSlots.Shild)
        m_owner.UpdateBlockChance();
      SetItemDataRecord setItemRecord = SetItemManager.GetSetItemRecord(ItemId);
      if(setItemRecord != null && OwningCharacter.AppliedSets.ContainsKey(setItemRecord.Id))
      {
        RemoveSetBonus(setItemRecord.GetBonus(OwningCharacter.AppliedSets[setItemRecord.Id]));
        Dictionary<int, byte> appliedSets;
        int id;
        (appliedSets = OwningCharacter.AppliedSets)[id = setItemRecord.Id] =
          (byte) (appliedSets[id] - 1U);
      }

      if(m_hitProc != null)
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
      if(bonus == null)
        return;
      ModifyStat((Asda2ItemBonusType) bonus.Type, bonus.Value);
    }

    private void RemoveSetBonus(Asda2SetBonus bonus)
    {
      if(bonus == null)
        return;
      ModifyStat((Asda2ItemBonusType) bonus.Type, -bonus.Value);
    }

    private void ProcessAddSoul(int sowelId)
    {
      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(sowelId);
      if(template.SowelBonusType == ItemBonusType.WeaponAtack ||
         template.SowelBonusType == ItemBonusType.WaponMAtack)
        return;
      ModifyStatBySowel(template.SowelBonusType, template.SowelBonusValue);
    }

    private void ModifyStat(Asda2ItemBonusType type, int value)
    {
      value = (int) (value * (double) CharacterFormulas.CalculateEnchantMultiplier(Enchant));
      switch(type)
      {
        case Asda2ItemBonusType.MaxAtack:
          OwningCharacter.ChangeModifier(StatModifierInt.Damage,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
          break;
        case Asda2ItemBonusType.MaxMAtak:
          OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
          break;
        case Asda2ItemBonusType.MaxDef:
          OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
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
          OwningCharacter.ChangeModifier(StatModifierFloat.FireAttribute, value / 100f);
          break;
        case Asda2ItemBonusType.WaterAttribue:
          OwningCharacter.ChangeModifier(StatModifierFloat.WaterAttribute, value / 100f);
          break;
        case Asda2ItemBonusType.EarthAttribute:
          OwningCharacter.ChangeModifier(StatModifierFloat.EarthAttribute, value / 100f);
          break;
        case Asda2ItemBonusType.ClimateAtribute:
          OwningCharacter.ChangeModifier(StatModifierFloat.ClimateAttribute, value / 100f);
          break;
        case Asda2ItemBonusType.LightAttribute:
          OwningCharacter.ChangeModifier(StatModifierFloat.LightAttribute, value / 100f);
          break;
        case Asda2ItemBonusType.DarkAttribute:
          OwningCharacter.ChangeModifier(StatModifierFloat.DarkAttribute, value / 100f);
          break;
        case Asda2ItemBonusType.FireResistance:
          OwningCharacter.ChangeModifier(StatModifierFloat.FireResist, value / 100f);
          break;
        case Asda2ItemBonusType.WaterResistance:
          OwningCharacter.ChangeModifier(StatModifierFloat.WaterResist, value / 100f);
          break;
        case Asda2ItemBonusType.EarthResistance:
          OwningCharacter.ChangeModifier(StatModifierFloat.EarthResit, value / 100f);
          break;
        case Asda2ItemBonusType.ClimateResistance:
          OwningCharacter.ChangeModifier(StatModifierFloat.ClimateResist, value / 100f);
          break;
        case Asda2ItemBonusType.LightResistance:
          OwningCharacter.ChangeModifier(StatModifierFloat.LightResist, value / 100f);
          break;
        case Asda2ItemBonusType.DarkResistance:
          OwningCharacter.ChangeModifier(StatModifierFloat.DarkResit, value / 100f);
          break;
        case Asda2ItemBonusType.CraftingChance:
          OwningCharacter.ChangeModifier(StatModifierFloat.CraftingChance, value / 100f);
          break;
        case Asda2ItemBonusType.OhsSkillDamage:
          if(OwningCharacter.Archetype.ClassId != ClassId.OHS)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
          break;
        case Asda2ItemBonusType.SpearSkillDamage:
          if(OwningCharacter.Archetype.ClassId != ClassId.Spear)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
          break;
        case Asda2ItemBonusType.ThsSkillDamage:
          if(OwningCharacter.Archetype.ClassId != ClassId.THS)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
          break;
        case Asda2ItemBonusType.CrossbowSkillDamage:
          if(OwningCharacter.Archetype.ClassId != ClassId.Crossbow)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
          break;
        case Asda2ItemBonusType.BowSkillDamage:
          if(OwningCharacter.Archetype.ClassId != ClassId.Bow)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
          break;
        case Asda2ItemBonusType.BalistaSkillDamage:
          if(OwningCharacter.Archetype.ClassId != ClassId.Balista)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
          break;
        case Asda2ItemBonusType.StaffSkillDamage:
          if(OwningCharacter.Archetype.ClassId != ClassId.AtackMage &&
             OwningCharacter.Archetype.ClassId != ClassId.HealMage &&
             OwningCharacter.Archetype.ClassId != ClassId.SupportMage)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellDamage, value);
          break;
        case Asda2ItemBonusType.OhsSkillCrit:
          if(OwningCharacter.Archetype.ClassId != ClassId.OHS)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
          break;
        case Asda2ItemBonusType.SpearSkillCrit:
          if(OwningCharacter.Archetype.ClassId != ClassId.Spear)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
          break;
        case Asda2ItemBonusType.ThsSkillCrit:
          if(OwningCharacter.Archetype.ClassId != ClassId.THS)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
          break;
        case Asda2ItemBonusType.CrossbowSkillCrit:
          if(OwningCharacter.Archetype.ClassId != ClassId.Crossbow)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
          break;
        case Asda2ItemBonusType.BowSkillCrit:
          if(OwningCharacter.Archetype.ClassId != ClassId.Bow)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
          break;
        case Asda2ItemBonusType.BalistaSkillCrit:
          if(OwningCharacter.Archetype.ClassId != ClassId.Balista)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
          break;
        case Asda2ItemBonusType.StaffSkillCrit:
          if(OwningCharacter.Archetype.ClassId != ClassId.AtackMage &&
             OwningCharacter.Archetype.ClassId != ClassId.HealMage &&
             OwningCharacter.Archetype.ClassId != ClassId.SupportMage)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellCrit, value);
          break;
        case Asda2ItemBonusType.HuntingExp:
          OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, value / 100f);
          break;
        case Asda2ItemBonusType.HuntingExpMinus:
          OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount,
            (float) (-(double) value / 100.0));
          break;
        case Asda2ItemBonusType.QuestExp:
          OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, value / 100f);
          break;
        case Asda2ItemBonusType.QuestExpMinus:
          OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount,
            (float) (-(double) value / 100.0));
          break;
        case Asda2ItemBonusType.SkillRange:
          OwningCharacter.ChangeModifier(StatModifierInt.SpellRange, value);
          break;
        case Asda2ItemBonusType.RecoveryAmount:
          OwningCharacter.ChangeModifier(StatModifierInt.HealthRegen, value);
          break;
        case Asda2ItemBonusType.DropRate:
          OwningCharacter.ChangeModifier(StatModifierFloat.Asda2DropChance, value / 100f);
          break;
        case Asda2ItemBonusType.ExpItem:
          OwningCharacter.ChangeModifier(StatModifierFloat.Asda2ExpAmount, value / 100f);
          break;
        case Asda2ItemBonusType.MinAtack:
          OwningCharacter.ChangeModifier(StatModifierInt.Damage,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
          break;
        case Asda2ItemBonusType.MinMAtack:
          OwningCharacter.ChangeModifier(StatModifierInt.MagicDamage,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
          break;
        case Asda2ItemBonusType.MinDef:
          OwningCharacter.ChangeModifier(StatModifierInt.Asda2Defence,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
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
          OwningCharacter.ChangeModifier(StatModifierInt.BlockChance,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
          break;
        case Asda2ItemBonusType.MaxBlockRatePrc:
          OwningCharacter.ChangeModifier(StatModifierInt.BlockChance,
            (int) (value * (double) CharacterFormulas.MaxToTotalMultiplier));
          break;
        case Asda2ItemBonusType.BlockRatePrc:
          OwningCharacter.ChangeModifier(StatModifierInt.BlockChance, value);
          break;
        case Asda2ItemBonusType.BlockedDamadgeReduction:
          OwningCharacter.ChangeModifier(StatModifierInt.BlockValue, value);
          break;
        case Asda2ItemBonusType.OhsSubEffectChange:
          if(OwningCharacter.Archetype.ClassId != ClassId.OHS)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
          break;
        case Asda2ItemBonusType.SpearSubEffectChange:
          if(OwningCharacter.Archetype.ClassId != ClassId.Spear)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
          break;
        case Asda2ItemBonusType.ThsSubEffectChange:
          if(OwningCharacter.Archetype.ClassId != ClassId.THS)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
          break;
        case Asda2ItemBonusType.CrossbowSubEffectChange:
          if(OwningCharacter.Archetype.ClassId != ClassId.Crossbow)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
          break;
        case Asda2ItemBonusType.BowSubEffectChange:
          if(OwningCharacter.Archetype.ClassId != ClassId.Bow)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
          break;
        case Asda2ItemBonusType.BalistaSubEffectChange:
          if(OwningCharacter.Archetype.ClassId != ClassId.Balista)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
          break;
        case Asda2ItemBonusType.StaffSubEffectChange:
          if(OwningCharacter.Archetype.ClassId != ClassId.AtackMage &&
             OwningCharacter.Archetype.ClassId != ClassId.HealMage &&
             OwningCharacter.Archetype.ClassId != ClassId.SupportMage)
            break;
          OwningCharacter.ChangeModifier(StatModifierInt.SpellSubEffectChance, value);
          break;
        case Asda2ItemBonusType.HealSkill:
          OwningCharacter.HealingDoneMod += value;
          break;
        case Asda2ItemBonusType.RecoverySkill:
          OwningCharacter.HealingDoneMod += value;
          break;
        case Asda2ItemBonusType.HealRecoverySkill:
          OwningCharacter.HealingDoneMod += value;
          break;
        case Asda2ItemBonusType.RecoveryAmountByHealRecoveryPrc:
          OwningCharacter.HealingDoneModPct += value;
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
          OwningCharacter.ChangeModifier(StatModifierFloat.MeleeAttackTime, value / 100f);
          break;
        case Asda2ItemBonusType.MovementSpeedPrc:
          OwningCharacter.ChangeModifier(StatModifierFloat.Speed, value / 100f);
          break;
        case Asda2ItemBonusType.MagicDeffence:
          OwningCharacter.ChangeModifier(StatModifierInt.Asda2MagicDefence, value);
          break;
      }
    }

    private void ModifyStatBySowel(ItemBonusType type, int value)
    {
      value = (int) (value * (double) CharacterFormulas.CalculateEnchantMultiplier(Enchant));
      switch(type)
      {
        case ItemBonusType.Defence:
          OwningCharacter.ApplyStatMod(ItemModType.Asda2Defence,
            CharacterFormulas.GetSowelDeffence(value, Template.RequiredProfession));
          break;
        case ItemBonusType.Strength:
          OwningCharacter.ApplyStatMod(ItemModType.Strength, value);
          break;
        case ItemBonusType.Agility:
          OwningCharacter.ApplyStatMod(ItemModType.Agility, (int) (value * (77.0 / 64.0)));
          break;
        case ItemBonusType.Stamina:
          OwningCharacter.ApplyStatMod(ItemModType.Stamina, (int) (value * 1.5));
          break;
        case ItemBonusType.Energy:
          OwningCharacter.ApplyStatMod(ItemModType.Spirit, (int) (value * 1.5));
          break;
        case ItemBonusType.Intelect:
          OwningCharacter.ApplyStatMod(ItemModType.Intellect, value);
          break;
        case ItemBonusType.Luck:
          OwningCharacter.ApplyStatMod(ItemModType.Luck, (int) (value * 2.625));
          break;
        case ItemBonusType.AtackSpeedByPrc:
          OwningCharacter.ApplyStatMod(ItemModType.AtackTimePrc, value);
          break;
        case ItemBonusType.PhysicalDamageReduceByPrc:
          OwningCharacter.ApplyStatMod(ItemModType.Luck, value);
          break;
        case ItemBonusType.DropGoldByPrc:
          OwningCharacter.ApplyStatMod(ItemModType.DropGoldByPrc, value);
          break;
        case ItemBonusType.Expirience:
          OwningCharacter.ApplyStatMod(ItemModType.Asda2Expirience, value);
          break;
        case ItemBonusType.DropByPrc:
          OwningCharacter.ApplyStatMod(ItemModType.DropChance, value);
          break;
      }
    }

    private void ProcessRemoveSoul(int sowelId)
    {
      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(sowelId);
      if(template.SowelBonusType == ItemBonusType.WeaponAtack ||
         template.SowelBonusType == ItemBonusType.WaponMAtack)
        return;
      ModifyStatBySowel(template.SowelBonusType, -template.SowelBonusValue);
    }

    /// <summary>
    /// Called whenever an item is used.
    /// Make sure to only call on Items whose Template has a UseSpell.
    /// </summary>
    internal void OnUse()
    {
      if(m_template.BondType == ItemBondType.OnUse)
        IsSoulbound = true;
      m_template.NotifyUsed(this);
    }

    public void Destroy()
    {
      DoDestroy();
    }

    /// <summary>Called by the container to</summary>
    protected internal virtual void DoDestroy()
    {
      Asda2ItemRecord record = m_record;
      if(m_owner != null)
        m_owner.Asda2Inventory.RemoveItemFromInventory(this);
      if(record == null)
        return;
      record.OwnerId = 0U;
      record.DeleteLater();
      m_record = null;
      Dispose();
    }

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
    }

    public override void Dispose(bool disposing)
    {
      m_owner = null;
      m_isInWorld = false;
      IsDeleted = true;
    }

    public override string ToString()
    {
      return string.Format("[{0}]{1} Amount {2} Category {3}", (object) ItemId,
        (object) (Asda2ItemId) ItemId, (object) Amount, (object) Category);
    }

    public bool IsInContext
    {
      get
      {
        Unit owner = Owner;
        if(owner != null)
        {
          IContextHandler contextHandler = owner.ContextHandler;
          if(contextHandler != null)
            return contextHandler.IsInContext;
        }

        return false;
      }
    }

    public bool IsWeapon
    {
      get { return Template.IsWeapon; }
      set { }
    }

    public bool IsAccessory
    {
      get { return Template.IsAccessory; }
      set { }
    }

    public int BoosterId
    {
      get { return Template.BoosterId; }
    }

    public int PackageId
    {
      get { return Template.PackageId; }
    }

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
      Unit owner = Owner;
      if(owner == null)
        return;
      owner.AddMessage(message);
    }

    public void AddMessage(Action action)
    {
      Unit owner = Owner;
      if(owner == null)
        return;
      owner.AddMessage(action);
    }

    public bool ExecuteInContext(Action action)
    {
      Unit owner = Owner;
      if(owner != null)
        return owner.ExecuteInContext(action);
      return false;
    }

    public void EnsureContext()
    {
      Unit owner = Owner;
      if(owner == null)
        return;
      owner.EnsureContext();
    }

    public List<Asda2ItemTemplate> InsertedSowels
    {
      get
      {
        List<Asda2ItemTemplate> asda2ItemTemplateList = new List<Asda2ItemTemplate>();
        if(Soul1Id != 0)
        {
          Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(Soul1Id);
          if(template != null)
            asda2ItemTemplateList.Add(template);
        }

        if(Soul2Id != 0)
        {
          Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(Soul2Id);
          if(template != null)
            asda2ItemTemplateList.Add(template);
        }

        if(Soul3Id != 0)
        {
          Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(Soul3Id);
          if(template != null)
            asda2ItemTemplateList.Add(template);
        }

        if(Soul4Id != 0)
        {
          Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(Soul4Id);
          if(template != null)
            asda2ItemTemplateList.Add(template);
        }

        return asda2ItemTemplateList;
      }
    }

    public byte RequiredLevel
    {
      get { return (byte) Template.RequiredLevel; }
    }

    public bool IsRod
    {
      get { return Template.IsRod; }
    }

    public bool SetParametr(Asda2ItemBonusType type, short value, byte slot)
    {
      if(slot > 4)
        return false;
      switch(slot)
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

    private void RecalculateItemParametrs()
    {
      if(!IsWeapon)
        return;
      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(Soul1Id);
      if(template == null)
      {
        Damages = new DamageInfo[1]
        {
          new DamageInfo(DamageSchoolMask.Physical, 1f, 3f)
        };
      }
      else
      {
        float enchantMultiplier = CharacterFormulas.CalculateEnchantMultiplier(Enchant);
        float num = CharacterFormulas.CalcWeaponTypeMultiplier(Category,
          OwningCharacter == null ? ClassId.NoClass : OwningCharacter.Archetype.ClassId);
        Damages = new DamageInfo[1]
        {
          new DamageInfo(
            Category == Asda2ItemCategory.Staff ? DamageSchoolMask.Magical : DamageSchoolMask.Physical,
            template.SowelBonusValue * enchantMultiplier * num,
            (float) (template.SowelBonusValue * (double) enchantMultiplier * 1.10000002384186) *
            num)
        };
      }
    }

    private float OprionValueMultiplier
    {
      get
      {
        float num = 1f;
        if(Record.IsCrafted)
          num += 0.5f;
        switch(Template.Quality)
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
      float oprionValueMultiplier = OprionValueMultiplier;
      ItemStatBonus bonus1 = Template.StatGeneratorCommon.GetBonus();
      Parametr1Type = bonus1.Type;
      Parametr1Value = (short) (bonus1.GetValue() * (double) oprionValueMultiplier);
      ItemStatBonus bonus2 = Template.StatGeneratorCommon.GetBonus();
      Parametr2Type = bonus2.Type;
      Parametr2Value = (short) (bonus2.GetValue() * (double) oprionValueMultiplier);
      if(Record.IsCrafted)
        GenerateOptionsByCraft();
      if(Enchant < CharacterFormulas.OptionStatStartsWithEnchantValue)
        return;
      GenerateOptionsByUpgrade();
    }

    public void GenerateOptionsByCraft()
    {
      ItemStatBonus bonus = Template.StatGeneratorCraft.GetBonus();
      Parametr3Type = bonus.Type;
      Parametr3Value = (short) (bonus.GetValue() * (double) OprionValueMultiplier);
    }

    public void GenerateOptionsByUpgrade()
    {
      ItemStatBonus bonus = Template.StatGeneratorEnchant.GetBonus();
      if(Parametr4Type == Asda2ItemBonusType.None)
      {
        Parametr4Type = bonus.Type;
      }

      Parametr4Value = (short) (bonus.GetValue() * (double) OprionValueMultiplier *
                                CharacterFormulas.CalculateEnchantMultiplierNotDamageItemStats(
                                  Enchant));
    }

    public void SetRandomAdvancedEnchant()
    {
      ItemStatBonus bonus = Template.StatGeneratorAdvanced.GetBonus();
      Parametr5Type = bonus.Type;
      Parametr5Value = (short) (bonus.GetValue() * (double) OprionValueMultiplier);
    }

    public int ItemId
    {
      get
      {
        if(!IsDeleted)
          return m_record.ItemId;
        return _itemId;
      }
      set
      {
        if(m_record != null)
          m_record.ItemId = value;
        _itemId = value;
      }
    }

    public Character OwningCharacter
    {
      get { return m_owner; }
      internal set
      {
        if(m_owner == value)
          return;
        m_owner = value;
        if(m_owner != null)
        {
          m_isInWorld = m_unknown = true;
          m_record.OwnerId = value.EntityId.Low;
          m_record.OwnerName = value.Name;
        }
        else
        {
          m_record.OwnerId = 0U;
          m_record.OwnerName = "No owner.";
        }
      }
    }

    public int CountForNextSell { get; set; }

    /// <summary>The life-time of this Item in seconds</summary>
    public EntityId Creator
    {
      get { return new EntityId((ulong) m_record.CreatorEntityId); }
      set { m_record.CreatorEntityId = (long) value.Full; }
    }

    /// <summary>
    /// The Slot of this Item within its <see cref="T:WCell.RealmServer.Entities.Container">Container</see>.
    /// </summary>
    public short Slot
    {
      get
      {
        if(!IsDeleted)
          return m_record.Slot;
        return _slot;
      }
      internal set
      {
        m_record.Slot = value;
        _slot = value;
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
      if(value == 0)
        return 0;
      Amount += value;
      return value;
    }

    /// <summary>
    /// Current amount of items in this stack.
    /// Setting the Amount to 0 will destroy the Item.
    /// Keep in mind that this is uint and thus can never become smaller than 0!
    /// </summary>
    public int Amount
    {
      get { return IsDeleted ? -1 : m_record.Amount; }
      set
      {
        if(value <= 0)
        {
          m_record.Amount = 0;
          Destroy();
        }
        else
        {
          if(value - m_record.Amount == 0)
            return;
          m_record.Amount = value;
        }
      }
    }

    public uint Duration
    {
      get
      {
        if(!IsDeleted)
          return (uint) m_record.Duration;
        return 0;
      }
      set { m_record.Duration = (int) value; }
    }

    /// <summary>
    /// Charges of the <c>UseSpell</c> of this Item.
    /// </summary>
    public bool IsAuctioned
    {
      get
      {
        if(!IsDeleted)
          return m_record.IsAuctioned;
        return false;
      }
      set { m_record.IsAuctioned = true; }
    }

    public int AuctionPrice
    {
      get { return Record.AuctionPrice; }
      set { Record.AuctionPrice = value; }
    }

    public bool IsSoulbound
    {
      get
      {
        if(!IsDeleted)
          return m_record.IsSoulBound;
        return false;
      }
      set { m_record.IsSoulBound = value; }
    }

    public byte Durability
    {
      get { return IsDeleted ? (byte) 0 : m_record.Durability; }
      set { m_record.Durability = value; }
    }

    public byte MaxDurability
    {
      get { return IsDeleted ? (byte) 0 : Template.MaxDurability; }
      protected set { Template.MaxDurability = value; }
    }

    public void RepairDurability()
    {
      Durability = MaxDurability;
    }

    public DamageInfo[] Damages
    {
      get { return _damages; }
      private set { _damages = value; }
    }

    public int BonusDamage { get; set; }

    public bool IsRanged
    {
      get
      {
        if(!IsDeleted)
          return m_template.IsRangedWeapon;
        return false;
      }
    }

    public bool IsMelee
    {
      get
      {
        if(!IsDeleted)
          return m_template.IsMeleeWeapon;
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
      get { return IsDeleted ? 0.0f : m_template.AtackRange; }
    }

    /// <summary>The time in milliseconds between 2 attacks</summary>
    public int AttackTime
    {
      get
      {
        if(!IsDeleted)
          return m_template.AttackTime;
        return 0;
      }
    }

    public Asda2ItemRecord Record
    {
      get { return m_record; }
    }

    public override ObjectTypeCustom CustomType
    {
      get { return ObjectTypeCustom.Object | ObjectTypeCustom.Item; }
    }

    public Asda2InventoryType InventoryType
    {
      get
      {
        if(!IsDeleted)
          return (Asda2InventoryType) m_record.InventoryType;
        return (Asda2InventoryType) _inventoryType;
      }
      set
      {
        m_record.InventoryType = (byte) value;
        _inventoryType = m_record.InventoryType;
      }
    }

    public int Soul1Id
    {
      get
      {
        if(!IsDeleted)
          return m_record.Soul1Id;
        return 0;
      }
      set { m_record.Soul1Id = value; }
    }

    public int Soul2Id
    {
      get
      {
        if(!IsDeleted)
          return m_record.Soul2Id;
        return 0;
      }
      set { m_record.Soul2Id = value; }
    }

    public int Soul3Id
    {
      get
      {
        if(!IsDeleted)
          return m_record.Soul3Id;
        return 0;
      }
      set { m_record.Soul3Id = value; }
    }

    public int Soul4Id
    {
      get
      {
        if(!IsDeleted)
          return m_record.Soul4Id;
        return 0;
      }
      set { m_record.Soul4Id = value; }
    }

    private bool IsValidSowel(int id)
    {
      return IsValidSowel(Asda2ItemMgr.GetTemplate(id));
    }

    private bool IsValidSowel(Asda2ItemTemplate sowel)
    {
      return sowel != null && sowel.Category == Asda2ItemCategory.Sowel &&
             (IsValidSowelEquipSlot(sowel) && sowel.RequiredLevel <= Owner.Level);
    }

    public bool InsertSowel(Asda2Item sowel, byte slot)
    {
      if(!IsValidSowel(sowel.Template) || slot > SocketsCount - 1)
        return false;
      switch(slot)
      {
        case 0:
          Soul1Id = sowel.ItemId;
          break;
        case 1:
          Soul2Id = sowel.ItemId;
          break;
        case 2:
          Soul3Id = sowel.ItemId;
          break;
        case 3:
          Soul4Id = sowel.ItemId;
          break;
      }

      RecalculateItemParametrs();
      return true;
    }

    private bool IsValidSowelEquipSlot(Asda2ItemTemplate sowel)
    {
      int sowelEquipmentType = (int) sowel.SowelEquipmentType;
      return Template.EquipmentSlot == (Asda2EquipmentSlots) sowel.SowelEquipmentType;
    }

    public byte Enchant
    {
      get { return IsDeleted ? (byte) 0 : m_record.Enchant; }
      set
      {
        if(value == Enchant)
          return;
        m_record.Enchant = value;
        if(Enchant >= CharacterFormulas.OptionStatStartsWithEnchantValue)
          GenerateOptionsByUpgrade();
        RecalculateItemParametrs();
      }
    }

    public Asda2ItemBonusType Parametr1Type
    {
      get
      {
        if(!IsDeleted)
          return (Asda2ItemBonusType) m_record.Parametr1Type;
        return Asda2ItemBonusType.None;
      }
      set { m_record.Parametr1Type = (short) value; }
    }

    public short Parametr1Value
    {
      get { return IsDeleted ? (short) 0 : m_record.Parametr1Value; }
      set { m_record.Parametr1Value = value; }
    }

    public Asda2ItemBonusType Parametr2Type
    {
      get
      {
        if(!IsDeleted)
          return (Asda2ItemBonusType) m_record.Parametr2Type;
        return Asda2ItemBonusType.None;
      }
      set { m_record.Parametr2Type = (short) value; }
    }

    public short Parametr2Value
    {
      get { return IsDeleted ? (short) 0 : m_record.Parametr2Value; }
      set { m_record.Parametr2Value = value; }
    }

    public Asda2ItemBonusType Parametr3Type
    {
      get
      {
        if(!IsDeleted)
          return (Asda2ItemBonusType) m_record.Parametr3Type;
        return Asda2ItemBonusType.None;
      }
      set { m_record.Parametr3Type = (short) value; }
    }

    public short Parametr3Value
    {
      get { return IsDeleted ? (short) 0 : m_record.Parametr1Value; }
      set { m_record.Parametr1Value = value; }
    }

    public Asda2ItemBonusType Parametr4Type
    {
      get
      {
        if(!IsDeleted)
          return (Asda2ItemBonusType) m_record.Parametr4Type;
        return Asda2ItemBonusType.None;
      }
      set { m_record.Parametr4Type = (short) value; }
    }

    public short Parametr4Value
    {
      get { return IsDeleted ? (short) 0 : m_record.Parametr4Value; }
      set { m_record.Parametr4Value = value; }
    }

    public Asda2ItemBonusType Parametr5Type
    {
      get
      {
        if(!IsDeleted)
          return (Asda2ItemBonusType) m_record.Parametr5Type;
        return Asda2ItemBonusType.None;
      }
      set { m_record.Parametr5Type = (short) value; }
    }

    public short Parametr5Value
    {
      get { return IsDeleted ? (short) 0 : m_record.Parametr5Value; }
      set { m_record.Parametr5Value = value; }
    }

    public ushort Weight
    {
      get { return IsDeleted ? (ushort) 0 : m_record.Weight; }
      set { m_record.Weight = value; }
    }

    public byte SealCount
    {
      get { return IsDeleted ? (byte) 0 : m_record.SealCount; }
      set { m_record.SealCount = value; }
    }

    public Asda2ItemCategory Category
    {
      get
      {
        if(!IsDeleted)
          return Template.Category;
        return 0;
      }
    }

    public byte SowelSlots
    {
      get { return IsDeleted ? (byte) 0 : Template.SowelSocketsCount; }
    }

    public int AuctionId
    {
      get { return (int) Record.Guid; }
    }

    public uint RepairCost()
    {
      return CharacterFormulas.CalculteItemRepairCost(MaxDurability, Durability,
        Template.SellPrice, Enchant, (byte) Template.AuctionLevelCriterion,
        (byte) Template.Quality);
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
      return chr == m_owner
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
      if(Durability < i)
      {
        Durability = 0;
        OnUnEquip();
      }
      else
        Durability -= i;

      if(silent)
        return;
      Asda2CharacterHandler.SendUpdateDurabilityResponse(OwningCharacter.Client, this);
    }
  }
}