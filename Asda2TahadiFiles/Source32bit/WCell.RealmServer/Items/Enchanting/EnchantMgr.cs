using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Spells;
using WCell.Util.Variables;

namespace WCell.RealmServer.Items.Enchanting
{
  public static class EnchantMgr
  {
    [NotVariable]public static readonly EnchantHandler[]
      ApplyEnchantToItemHandlers = new EnchantHandler[9];

    [NotVariable]public static readonly EnchantHandler[] RemoveEnchantFromItemHandlers =
      new EnchantHandler[9];

    [NotVariable]public static readonly EnchantHandler[] ApplyEquippedEnchantHandlers =
      new EnchantHandler[9];

    [NotVariable]public static readonly EnchantHandler[] RemoveEquippedEnchantHandlers =
      new EnchantHandler[9];

    [NotVariable]
    public static List<ItemRandomEnchantEntry>[] RandomEnchantEntries = new List<ItemRandomEnchantEntry>[9000];

    public static MappedDBCReader<ItemEnchantmentEntry, ItemEnchantmentConverter> EnchantmentEntryReader;

    public static MappedDBCReader<ItemEnchantmentCondition, ItemEnchantmentConditionConverter>
      EnchantmentConditionReader;

    public static MappedDBCReader<GemProperties, GemPropertiesConverter> GemPropertiesReader;

    internal static void Init()
    {
      ApplyEnchantToItemHandlers[2] = ApplyDamageToItem;
      RemoveEnchantFromItemHandlers[2] =
        RemoveDamageFromItem;
      ApplyEquippedEnchantHandlers[2] = DoNothing;
      ApplyEquippedEnchantHandlers[4] = DoNothing;
      ApplyEquippedEnchantHandlers[1] = ApplyCombatSpell;
      ApplyEquippedEnchantHandlers[3] = ApplyEquipSpell;
      ApplyEquippedEnchantHandlers[5] = ApplyStat;
      ApplyEquippedEnchantHandlers[6] = ApplyTotem;
      RemoveEquippedEnchantHandlers[2] = DoNothing;
      RemoveEquippedEnchantHandlers[4] = DoNothing;
      RemoveEquippedEnchantHandlers[1] = RemoveCombatSpell;
      RemoveEquippedEnchantHandlers[3] = RemoveEquipSpell;
      RemoveEquippedEnchantHandlers[5] = RemoveStat;
      RemoveEquippedEnchantHandlers[6] = RemoveTotem;
      EnchantmentConditionReader =
        new MappedDBCReader<ItemEnchantmentCondition, ItemEnchantmentConditionConverter>(
          RealmServerConfiguration.GetDBCFile("SpellItemEnchantmentCondition.dbc"));
      EnchantmentEntryReader =
        new MappedDBCReader<ItemEnchantmentEntry, ItemEnchantmentConverter>(
          RealmServerConfiguration.GetDBCFile("SpellItemEnchantment.dbc"));
      GemPropertiesReader =
        new MappedDBCReader<GemProperties, GemPropertiesConverter>(
          RealmServerConfiguration.GetDBCFile("GemProperties.dbc"));
    }

    private static void DoNothing(Item item, ItemEnchantmentEffect effect)
    {
    }

    public static ItemEnchantmentEntry GetEnchantmentEntry(uint id)
    {
      ItemEnchantmentEntry enchantmentEntry;
      EnchantmentEntryReader.Entries.TryGetValue((int) id, out enchantmentEntry);
      return enchantmentEntry;
    }

    public static GemProperties GetGemproperties(uint id)
    {
      GemProperties gemProperties;
      GemPropertiesReader.Entries.TryGetValue((int) id, out gemProperties);
      return gemProperties;
    }

    public static ItemEnchantmentCondition GetEnchantmentCondition(uint id)
    {
      ItemEnchantmentCondition enchantmentCondition;
      EnchantmentConditionReader.Entries.TryGetValue((int) id, out enchantmentCondition);
      return enchantmentCondition;
    }

    /// <summary>
    /// Applies the given EnchantEffect to the given Item and the wearer of the Item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="effect"></param>
    internal static void ApplyEquippedEffect(Item item, ItemEnchantmentEffect effect)
    {
      ApplyEquippedEnchantHandlers[(uint) effect.Type](item, effect);
    }

    internal static void ApplyEnchantToItem(Item item, ItemEnchantment enchant)
    {
      foreach(ItemEnchantmentEffect effect in enchant.Entry.Effects)
      {
        EnchantHandler enchantToItemHandler =
          ApplyEnchantToItemHandlers[(uint) effect.Type];
        if(enchantToItemHandler != null)
          enchantToItemHandler(item, effect);
      }
    }

    internal static void RemoveEnchantFromItem(Item item, ItemEnchantment enchant)
    {
      foreach(ItemEnchantmentEffect effect in enchant.Entry.Effects)
      {
        EnchantHandler enchantFromItemHandler =
          RemoveEnchantFromItemHandlers[(uint) effect.Type];
        if(enchantFromItemHandler != null)
          enchantFromItemHandler(item, effect);
      }
    }

    /// <summary>
    /// Removes the given EnchantEffect from the given Item and the wearer of the Item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="effect"></param>
    internal static void RemoveEffect(Item item, ItemEnchantmentEffect effect)
    {
      RemoveEquippedEnchantHandlers[(uint) effect.Type](item, effect);
    }

    private static void ApplyCombatSpell(Item item, ItemEnchantmentEffect effect)
    {
      Spell spell = SpellHandler.Get(effect.Misc);
      if(spell == null)
        ContentMgr.OnInvalidClientData("Enchantment Effect {0} had invalid SpellId: {1}", (object) effect,
          (object) (SpellId) effect.Misc);
      else
        item.OwningCharacter.AddProcHandler(new ItemHitProcHandler(item, spell));
    }

    private static void ApplyEquipSpell(Item item, ItemEnchantmentEffect effect)
    {
      Character owningCharacter = item.OwningCharacter;
      Spell spell = SpellHandler.Get((SpellId) effect.Misc);
      if(spell == null)
        LogManager.GetCurrentClassLogger().Warn("{0} had invalid SpellId: {1}", effect,
          (SpellId) effect.Misc);
      else
        SpellCast.ValidateAndTriggerNew(spell, owningCharacter, owningCharacter,
          null, item, null, null);
    }

    private static void ApplyStat(Item item, ItemEnchantmentEffect effect)
    {
      item.OwningCharacter.ApplyStatMod((ItemModType) effect.Misc, effect.MaxAmount);
    }

    private static void ApplyTotem(Item item, ItemEnchantmentEffect effect)
    {
    }

    private static void RemoveCombatSpell(Item item, ItemEnchantmentEffect effect)
    {
      item.OwningCharacter.RemoveProcHandler(handler =>
      {
        if(handler.ProcSpell != null)
          return (int) handler.ProcSpell.Id == (int) effect.Misc;
        return false;
      });
    }

    private static void RemoveEquipSpell(Item item, ItemEnchantmentEffect effect)
    {
      item.OwningCharacter.Auras.Remove((SpellId) effect.Misc);
    }

    private static void RemoveStat(Item item, ItemEnchantmentEffect effect)
    {
      item.OwningCharacter.RemoveStatMod((ItemModType) effect.Misc, effect.MaxAmount);
    }

    private static void RemoveTotem(Item item, ItemEnchantmentEffect effect)
    {
    }

    private static void ApplyDamageToItem(Item item, ItemEnchantmentEffect effect)
    {
      item.BonusDamage += effect.MaxAmount;
      if(!item.IsEquippedItem)
        return;
      item.Container.Owner.UpdateDamage((InventorySlot) item.Slot);
    }

    private static void RemoveDamageFromItem(Item item, ItemEnchantmentEffect effect)
    {
      item.BonusDamage -= effect.MaxAmount;
      if(!item.IsEquippedItem)
        return;
      item.Container.Owner.UpdateDamage((InventorySlot) item.Slot);
    }

    public static ItemSuffixCategory GetSuffixCategory(ItemTemplate template)
    {
      if(template.IsRangedWeapon)
        return ItemSuffixCategory.Ranged;
      if(template.IsWeapon)
        return ItemSuffixCategory.Weapon;
      switch(template.InventorySlotType)
      {
        case InventorySlotType.Head:
        case InventorySlotType.Body:
        case InventorySlotType.Chest:
        case InventorySlotType.Legs:
        case InventorySlotType.Robe:
          return ItemSuffixCategory.MainArmor;
        case InventorySlotType.Neck:
        case InventorySlotType.Wrist:
        case InventorySlotType.Finger:
        case InventorySlotType.Shield:
        case InventorySlotType.Cloak:
        case InventorySlotType.Holdable:
          return ItemSuffixCategory.Other;
        case InventorySlotType.Shoulder:
        case InventorySlotType.Waist:
        case InventorySlotType.Feet:
        case InventorySlotType.Hand:
        case InventorySlotType.Trinket:
          return ItemSuffixCategory.SecondaryArmor;
        default:
          return ItemSuffixCategory.None;
      }
    }

    public static uint GetRandomSuffixFactor(ItemTemplate template)
    {
      ItemSuffixCategory suffixCategory = GetSuffixCategory(template);
      if(suffixCategory >= ItemSuffixCategory.None)
        return 0;
      ItemLevelInfo levelInfo = ItemMgr.GetLevelInfo(template.Level);
      if(levelInfo != null)
      {
        switch(template.Quality)
        {
          case ItemQuality.Uncommon:
            return levelInfo.UncommonPoints[(uint) suffixCategory];
          case ItemQuality.Rare:
            return levelInfo.RarePoints[(uint) suffixCategory];
          case ItemQuality.Epic:
          case ItemQuality.Legendary:
          case ItemQuality.Artifact:
            return levelInfo.EpicPoints[(uint) suffixCategory];
        }
      }

      return 0;
    }

    public delegate void EnchantHandler(Item item, ItemEnchantmentEffect effect);
  }
}