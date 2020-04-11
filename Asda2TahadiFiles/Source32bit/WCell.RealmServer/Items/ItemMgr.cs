using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.Quests;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Items
{
  [GlobalMgr]
  public static class ItemMgr
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// All defined <see cref="T:WCell.RealmServer.Items.ItemTemplate">ItemTemplates</see>.
    /// </summary>
    [NotVariable]public static ItemTemplate[] Templates = new ItemTemplate[100000];

    /// <summary>All ItemSet definitions</summary>
    [NotVariable]public static ItemSet[] Sets = new ItemSet[1000];

    /// <summary>All partial inventory types by InventorySlot</summary>
    public static readonly PartialInventoryType[] PartialInventoryTypes = new PartialInventoryType[118];

    public static readonly EquipmentSlot[] AllBagSlots = new EquipmentSlot[4]
    {
      EquipmentSlot.Bag1,
      EquipmentSlot.Bag2,
      EquipmentSlot.Bag3,
      EquipmentSlot.Bag4
    };

    /// <summary>
    /// Maps a set of available InventorySlots by their corresponding InventorySlotType
    /// </summary>
    public static readonly EquipmentSlot[][] EquipmentSlotsByInvSlot = GetEqByInv();

    public static readonly InventorySlot[][] EquippableInvSlotsByClass = GetEqByCl();

    public static readonly InventorySlot[] EquipmentSlots = new InventorySlot[19]
    {
      InventorySlot.Head,
      InventorySlot.Neck,
      InventorySlot.Shoulders,
      InventorySlot.Shirt,
      InventorySlot.Chest,
      InventorySlot.Belt,
      InventorySlot.Pants,
      InventorySlot.Boots,
      InventorySlot.Wrist,
      InventorySlot.Gloves,
      InventorySlot.AvHead,
      InventorySlot.AvShirt,
      InventorySlot.AvPans,
      InventorySlot.AvaBoots,
      InventorySlot.AvGloves,
      InventorySlot.AvLeftHead,
      InventorySlot.AvRightHead,
      InventorySlot.AvCloak,
      InventorySlot.AvWings
    };

    /// <summary>
    /// Contains all InventorySlots that are used as storage on the Character, without bank slots
    /// </summary>
    public static readonly InventorySlot[] StorageSlotsWithoutBank = new InventorySlot[20]
    {
      InventorySlot.BackPack1,
      InventorySlot.BackPack2,
      InventorySlot.BackPack3,
      InventorySlot.BackPack4,
      InventorySlot.BackPack5,
      InventorySlot.BackPack6,
      InventorySlot.BackPack7,
      InventorySlot.BackPack8,
      InventorySlot.BackPack9,
      InventorySlot.BackPack10,
      InventorySlot.BackPack11,
      InventorySlot.BackPack12,
      InventorySlot.BackPack13,
      InventorySlot.BackPack14,
      InventorySlot.BackPack15,
      InventorySlot.BackPackLast,
      InventorySlot.Bag1,
      InventorySlot.Bag2,
      InventorySlot.Bag3,
      InventorySlot.BagLast
    };

    /// <summary>
    /// Contains all Equipment and on-character inventory slots without keys
    /// </summary>
    public static readonly InventorySlot[] InvSlots = new InventorySlot[43]
    {
      InventorySlot.Head,
      InventorySlot.Neck,
      InventorySlot.Shoulders,
      InventorySlot.Shirt,
      InventorySlot.Chest,
      InventorySlot.Belt,
      InventorySlot.Pants,
      InventorySlot.Boots,
      InventorySlot.Wrist,
      InventorySlot.Gloves,
      InventorySlot.AvHead,
      InventorySlot.AvShirt,
      InventorySlot.AvPans,
      InventorySlot.AvaBoots,
      InventorySlot.AvGloves,
      InventorySlot.AvLeftHead,
      InventorySlot.AvRightHead,
      InventorySlot.AvCloak,
      InventorySlot.AvWings,
      InventorySlot.Bag1,
      InventorySlot.Bag2,
      InventorySlot.Bag3,
      InventorySlot.BagLast,
      InventorySlot.BackPack1,
      InventorySlot.BackPack2,
      InventorySlot.BackPack3,
      InventorySlot.BackPack4,
      InventorySlot.BackPack5,
      InventorySlot.BackPack6,
      InventorySlot.BackPack7,
      InventorySlot.BackPack8,
      InventorySlot.BackPack9,
      InventorySlot.BackPack10,
      InventorySlot.BackPack11,
      InventorySlot.BackPack12,
      InventorySlot.BackPack13,
      InventorySlot.BackPack14,
      InventorySlot.BackPack15,
      InventorySlot.BackPackLast,
      InventorySlot.Bag1,
      InventorySlot.Bag2,
      InventorySlot.Bag3,
      InventorySlot.BagLast
    };

    /// <summary>
    /// Contains all InventorySlots that are used as storage on the Character, including bank slots
    /// </summary>
    public static readonly InventorySlot[] InvSlotsWithBank = new InventorySlot[78]
    {
      InventorySlot.Head,
      InventorySlot.Neck,
      InventorySlot.Shoulders,
      InventorySlot.Shirt,
      InventorySlot.Chest,
      InventorySlot.Belt,
      InventorySlot.Pants,
      InventorySlot.Boots,
      InventorySlot.Wrist,
      InventorySlot.Gloves,
      InventorySlot.AvHead,
      InventorySlot.AvShirt,
      InventorySlot.AvPans,
      InventorySlot.AvaBoots,
      InventorySlot.AvGloves,
      InventorySlot.AvLeftHead,
      InventorySlot.AvRightHead,
      InventorySlot.AvCloak,
      InventorySlot.AvWings,
      InventorySlot.Bag1,
      InventorySlot.Bag2,
      InventorySlot.Bag3,
      InventorySlot.BagLast,
      InventorySlot.BackPack1,
      InventorySlot.BackPack2,
      InventorySlot.BackPack3,
      InventorySlot.BackPack4,
      InventorySlot.BackPack5,
      InventorySlot.BackPack6,
      InventorySlot.BackPack7,
      InventorySlot.BackPack8,
      InventorySlot.BackPack9,
      InventorySlot.BackPack10,
      InventorySlot.BackPack11,
      InventorySlot.BackPack12,
      InventorySlot.BackPack13,
      InventorySlot.BackPack14,
      InventorySlot.BackPack15,
      InventorySlot.BackPackLast,
      InventorySlot.Bag1,
      InventorySlot.Bag2,
      InventorySlot.Bag3,
      InventorySlot.BagLast,
      InventorySlot.Bank1,
      InventorySlot.Bank2,
      InventorySlot.Bank3,
      InventorySlot.Bank4,
      InventorySlot.Bank5,
      InventorySlot.Bank6,
      InventorySlot.Bank7,
      InventorySlot.Bank8,
      InventorySlot.Bank9,
      InventorySlot.Bank10,
      InventorySlot.Bank11,
      InventorySlot.Bank12,
      InventorySlot.Bank13,
      InventorySlot.Bank14,
      InventorySlot.Bank15,
      InventorySlot.Bank16,
      InventorySlot.Bank17,
      InventorySlot.Bank18,
      InventorySlot.Bank19,
      InventorySlot.Bank20,
      InventorySlot.Bank21,
      InventorySlot.Bank22,
      InventorySlot.Bank23,
      InventorySlot.Bank24,
      InventorySlot.Bank25,
      InventorySlot.Bank26,
      InventorySlot.Bank27,
      InventorySlot.BankLast,
      InventorySlot.BankBag1,
      InventorySlot.BankBag2,
      InventorySlot.BankBag3,
      InventorySlot.BankBag4,
      InventorySlot.BankBag5,
      InventorySlot.BankBag6,
      InventorySlot.BankBagLast
    };

    /// <summary>Contains all BankSlots</summary>
    public static readonly InventorySlot[] BankSlots = new InventorySlot[28]
    {
      InventorySlot.Bank1,
      InventorySlot.Bank2,
      InventorySlot.Bank3,
      InventorySlot.Bank4,
      InventorySlot.Bank5,
      InventorySlot.Bank6,
      InventorySlot.Bank7,
      InventorySlot.Bank8,
      InventorySlot.Bank9,
      InventorySlot.Bank10,
      InventorySlot.Bank11,
      InventorySlot.Bank12,
      InventorySlot.Bank13,
      InventorySlot.Bank14,
      InventorySlot.Bank15,
      InventorySlot.Bank16,
      InventorySlot.Bank17,
      InventorySlot.Bank18,
      InventorySlot.Bank19,
      InventorySlot.Bank20,
      InventorySlot.Bank21,
      InventorySlot.Bank22,
      InventorySlot.Bank23,
      InventorySlot.Bank24,
      InventorySlot.Bank25,
      InventorySlot.Bank26,
      InventorySlot.Bank27,
      InventorySlot.BankLast
    };

    /// <summary>Contains all InventorySlots for BankBags</summary>
    public static readonly InventorySlot[] BankBagSlots = new InventorySlot[7]
    {
      InventorySlot.BankBag1,
      InventorySlot.BankBag2,
      InventorySlot.BankBag3,
      InventorySlot.BankBag4,
      InventorySlot.BankBag5,
      InventorySlot.BankBag6,
      InventorySlot.BankBagLast
    };

    /// <summary>
    /// All slots that can contain Items that actually belong to the Character (all InventorySlots, but BuyBack)
    /// </summary>
    public static readonly InventorySlot[] OwnedSlots = new InventorySlot[106]
    {
      InventorySlot.Head,
      InventorySlot.Neck,
      InventorySlot.Shoulders,
      InventorySlot.Shirt,
      InventorySlot.Chest,
      InventorySlot.Belt,
      InventorySlot.Pants,
      InventorySlot.Boots,
      InventorySlot.Wrist,
      InventorySlot.Gloves,
      InventorySlot.AvHead,
      InventorySlot.AvShirt,
      InventorySlot.AvPans,
      InventorySlot.AvaBoots,
      InventorySlot.AvGloves,
      InventorySlot.AvLeftHead,
      InventorySlot.AvRightHead,
      InventorySlot.AvCloak,
      InventorySlot.AvWings,
      InventorySlot.Bag1,
      InventorySlot.Bag2,
      InventorySlot.Bag3,
      InventorySlot.BagLast,
      InventorySlot.BackPack1,
      InventorySlot.BackPack2,
      InventorySlot.BackPack3,
      InventorySlot.BackPack4,
      InventorySlot.BackPack5,
      InventorySlot.BackPack6,
      InventorySlot.BackPack7,
      InventorySlot.BackPack8,
      InventorySlot.BackPack9,
      InventorySlot.BackPack10,
      InventorySlot.BackPack11,
      InventorySlot.BackPack12,
      InventorySlot.BackPack13,
      InventorySlot.BackPack14,
      InventorySlot.BackPack15,
      InventorySlot.BackPackLast,
      InventorySlot.Bank1,
      InventorySlot.Bank2,
      InventorySlot.Bank3,
      InventorySlot.Bank4,
      InventorySlot.Bank5,
      InventorySlot.Bank6,
      InventorySlot.Bank7,
      InventorySlot.Bank8,
      InventorySlot.Bank9,
      InventorySlot.Bank10,
      InventorySlot.Bank11,
      InventorySlot.Bank12,
      InventorySlot.Bank13,
      InventorySlot.Bank14,
      InventorySlot.Bank15,
      InventorySlot.Bank16,
      InventorySlot.Bank17,
      InventorySlot.Bank18,
      InventorySlot.Bank19,
      InventorySlot.Bank20,
      InventorySlot.Bank21,
      InventorySlot.Bank22,
      InventorySlot.Bank23,
      InventorySlot.Bank24,
      InventorySlot.Bank25,
      InventorySlot.Bank26,
      InventorySlot.Bank27,
      InventorySlot.BankLast,
      InventorySlot.BankBag1,
      InventorySlot.BankBag2,
      InventorySlot.BankBag3,
      InventorySlot.BankBag4,
      InventorySlot.BankBag5,
      InventorySlot.BankBag6,
      InventorySlot.BankBagLast,
      InventorySlot.Key1,
      InventorySlot.Key2,
      InventorySlot.Key3,
      InventorySlot.Key4,
      InventorySlot.Key5,
      InventorySlot.Key6,
      InventorySlot.Key7,
      InventorySlot.Key8,
      InventorySlot.Key9,
      InventorySlot.Key10,
      InventorySlot.Key11,
      InventorySlot.Key12,
      InventorySlot.Key13,
      InventorySlot.Key14,
      InventorySlot.Key15,
      InventorySlot.Key16,
      InventorySlot.Key17,
      InventorySlot.Key18,
      InventorySlot.Key19,
      InventorySlot.Key20,
      InventorySlot.Key21,
      InventorySlot.Key22,
      InventorySlot.Key23,
      InventorySlot.Key24,
      InventorySlot.Key25,
      InventorySlot.Key26,
      InventorySlot.Key27,
      InventorySlot.Key28,
      InventorySlot.Key29,
      InventorySlot.Key30,
      InventorySlot.Key31,
      InventorySlot.KeyLast
    };

    [NotVariable]public static readonly ItemTemplate[] FirstTotemsPerCat = new ItemTemplate[291];

    private static readonly List<Tuple<Asda2ItemId, Action<ItemTemplate>>> loadHooks =
      new List<Tuple<Asda2ItemId, Action<ItemTemplate>>>();

    private static readonly List<Tuple<ItemClass, Action<ItemTemplate>>> itemClassLoadHooks =
      new List<Tuple<ItemClass, Action<ItemTemplate>>>();

    public const uint MaxId = 100000;
    [NotVariable]public static MappedDBCReader<ItemLevelInfo, ItemRandPropPointConverter> RandomPropPointReader;

    [NotVariable]
    public static MappedDBCReader<ItemRandomPropertyEntry, ItemRandomPropertiesConverter> RandomPropertiesReader;

    [NotVariable]public static MappedDBCReader<ItemRandomSuffixEntry, ItemRandomSuffixConverter> RandomSuffixReader;

    [NotVariable]public static MappedDBCReader<ScalingStatDistributionEntry, ScalingStatDistributionConverter>
      ScalingStatDistributionReader;

    [NotVariable]public static MappedDBCReader<ScalingStatValues, ScalingStatValuesConverter> ScalingStatValuesReader;

    [NotVariable]public static InventorySlot[] AllSlots;
    [NotVariable]public static bool[] ContainerSlotsWithBank;
    [NotVariable]public static bool[] ContainerSlotsWithoutBank;
    [NotVariable]public static bool[] ContainerBankSlots;

    /// <summary>Returns the ItemTemplate with the given id</summary>
    public static ItemTemplate GetTemplate(Asda2ItemId id)
    {
      if((long) id >= Templates.Length)
        return null;
      return Templates[(uint) id];
    }

    /// <summary>Returns the ItemTemplate with the given id</summary>
    public static ItemTemplate GetTemplateForced(Asda2ItemId id)
    {
      ItemTemplate itemTemplate = (long) id < (long) Templates.Length
        ? Templates[(uint) id]
        : null;
      if(itemTemplate == null)
        throw new ContentException("Requested ItemTemplate does not exist: {0}", (object) id);
      return itemTemplate;
    }

    /// <summary>Returns the ItemTemplate with the given id</summary>
    public static ItemTemplate GetTemplate(uint id)
    {
      if(id >= Templates.Length)
        return null;
      return Templates[id];
    }

    /// <summary>Returns a List of templates with the given ItemClass</summary>
    public static IEnumerable<ItemTemplate> GetTemplates(ItemClass type)
    {
      return Templates
        .Where(template => template != null)
        .Where(template => template.Class == type);
    }

    /// <summary>Returns the ItemSet with the given id</summary>
    public static ItemSet GetSet(ItemSetId id)
    {
      if((long) id >= Sets.Length)
        return null;
      return Sets[(uint) id];
    }

    /// <summary>Returns the ItemSet with the given id</summary>
    public static ItemSet GetSet(uint id)
    {
      if(id >= Sets.Length)
        return null;
      return Sets[id];
    }

    public static EquipmentSlot[] GetEquipmentSlots(InventorySlotType invSlot)
    {
      return EquipmentSlotsByInvSlot[(int) invSlot];
    }

    private static EquipmentSlot[][] GetEqByInv()
    {
      EquipmentSlot[][] equipmentSlotArray =
        new EquipmentSlot[(int) (1 + Utility.GetMaxEnum<InventorySlotType>())][];
      equipmentSlotArray[18] = AllBagSlots;
      equipmentSlotArray[4] = new EquipmentSlot[1]
      {
        EquipmentSlot.Shirt
      };
      equipmentSlotArray[5] = new EquipmentSlot[1]
      {
        EquipmentSlot.Chest
      };
      equipmentSlotArray[16] = new EquipmentSlot[1]
      {
        EquipmentSlot.Back
      };
      equipmentSlotArray[8] = new EquipmentSlot[1]
      {
        EquipmentSlot.Boots
      };
      equipmentSlotArray[11] = new EquipmentSlot[2]
      {
        EquipmentSlot.Finger1,
        EquipmentSlot.Finger2
      };
      equipmentSlotArray[10] = new EquipmentSlot[1]
      {
        EquipmentSlot.Gloves
      };
      equipmentSlotArray[1] = new EquipmentSlot[1];
      equipmentSlotArray[23] = new EquipmentSlot[1]
      {
        EquipmentSlot.OffHand
      };
      equipmentSlotArray[7] = new EquipmentSlot[1]
      {
        EquipmentSlot.Pants
      };
      equipmentSlotArray[2] = new EquipmentSlot[1]
      {
        EquipmentSlot.Neck
      };
      equipmentSlotArray[27] = AllBagSlots;
      equipmentSlotArray[15] = new EquipmentSlot[1]
      {
        EquipmentSlot.ExtraWeapon
      };
      equipmentSlotArray[26] = new EquipmentSlot[1]
      {
        EquipmentSlot.ExtraWeapon
      };
      equipmentSlotArray[28] = new EquipmentSlot[1]
      {
        EquipmentSlot.ExtraWeapon
      };
      equipmentSlotArray[20] = new EquipmentSlot[1]
      {
        EquipmentSlot.Chest
      };
      equipmentSlotArray[14] = new EquipmentSlot[1]
      {
        EquipmentSlot.OffHand
      };
      equipmentSlotArray[3] = new EquipmentSlot[1]
      {
        EquipmentSlot.Shoulders
      };
      equipmentSlotArray[19] = new EquipmentSlot[1]
      {
        EquipmentSlot.Tabard
      };
      equipmentSlotArray[25] = new EquipmentSlot[1]
      {
        EquipmentSlot.ExtraWeapon
      };
      equipmentSlotArray[12] = new EquipmentSlot[2]
      {
        EquipmentSlot.Trinket1,
        EquipmentSlot.Trinket2
      };
      equipmentSlotArray[17] = new EquipmentSlot[1]
      {
        EquipmentSlot.MainHand
      };
      equipmentSlotArray[6] = new EquipmentSlot[1]
      {
        EquipmentSlot.Belt
      };
      equipmentSlotArray[13] = new EquipmentSlot[2]
      {
        EquipmentSlot.MainHand,
        EquipmentSlot.OffHand
      };
      equipmentSlotArray[21] = new EquipmentSlot[1]
      {
        EquipmentSlot.MainHand
      };
      equipmentSlotArray[22] = new EquipmentSlot[1]
      {
        EquipmentSlot.OffHand
      };
      equipmentSlotArray[9] = new EquipmentSlot[1]
      {
        EquipmentSlot.Wrist
      };
      equipmentSlotArray[24] = null;
      return equipmentSlotArray;
    }

    private static InventorySlot[][] GetEqByCl()
    {
      InventorySlot[][] inventorySlotArray = new InventorySlot[17][];
      inventorySlotArray[2] = new InventorySlot[3]
      {
        InventorySlot.AvLeftHead,
        InventorySlot.AvRightHead,
        InventorySlot.AvCloak
      };
      inventorySlotArray[4] = new InventorySlot[9]
      {
        InventorySlot.Chest,
        InventorySlot.Boots,
        InventorySlot.Gloves,
        InventorySlot.Head,
        InventorySlot.Pants,
        InventorySlot.Chest,
        InventorySlot.Shoulders,
        InventorySlot.Wrist,
        InventorySlot.Belt
      };
      return inventorySlotArray;
    }

    public static bool IsContainerEquipmentSlot(int slot)
    {
      if(slot >= 19 && slot <= 22)
        return true;
      if(slot >= 67)
        return slot <= 73;
      return false;
    }

    static ItemMgr()
    {
      List<InventorySlot> list = ((IEnumerable<InventorySlot>) Enum.GetValues(typeof(InventorySlot)))
        .ToList();
      list.Remove(InventorySlot.Count);
      list.Remove(InventorySlot.Invalid);
      AllSlots = list.ToArray();
    }

    public static void Initialize()
    {
      InitMisc();
      LoadAll();
    }

    public static void ForceInitialize()
    {
      InitMisc();
      if(Loaded)
        return;
      LoadAll();
    }

    public static void InitMisc()
    {
      if(ContainerSlotsWithBank != null)
        return;
      LockEntry.Initialize();
      ContainerSlotsWithBank = new bool[118];
      ContainerSlotsWithBank.Fill(true, 19, 22);
      ContainerSlotsWithBank.Fill(true, 67, 73);
      ContainerSlotsWithoutBank = new bool[118];
      ContainerSlotsWithoutBank.Fill(true, 19, 22);
      ContainerBankSlots = new bool[118];
      ContainerBankSlots.Fill(true, 67, 73);
      InitItemSlotHandlers();
    }

    private static void LoadDBCs()
    {
    }

    public static bool Loaded { get; private set; }

    public static void LoadAll()
    {
      if(Loaded)
        return;
      ContentMgr.Load<ItemTemplate>();
      OnLoaded();
      foreach(ItemTemplate template in Templates)
      {
        if(template != null)
          template.InitializeTemplate();
      }

      TruncSets();
      if(ArchetypeMgr.Loaded)
        ArchetypeMgr.LoadItems();
      SpellHandler.InitTools();
      LoadItemCharRelations();
      Singleton<AuctionMgr>.Instance.LoadItems();
      if(QuestMgr.Loaded)
        EnsureItemQuestRelations();
      ServerApp<RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(ItemMgr));
      Loaded = true;
    }

    private static void LoadItemCharRelations()
    {
      foreach(Character allCharacter in World.GetAllCharacters())
      {
        IContextHandler contextHandler = allCharacter.ContextHandler;
        if(contextHandler != null)
        {
          Character character = allCharacter;
          contextHandler.AddMessage(() =>
          {
            if(!character.IsInWorld)
              return;
            character.InitItems();
          });
        }
      }
    }

    internal static void EnsureItemQuestRelations()
    {
      foreach(QuestTemplate template1 in QuestMgr.Templates)
      {
        if(template1 != null && template1.CollectableItems != null)
        {
          foreach(Asda2ItemStackDescription collectableItem in template1.CollectableItems)
          {
            ItemTemplate template2 = GetTemplate(collectableItem.ItemId);
            if(template2 == null)
              ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
                (object) template1, (object) collectableItem);
            else if(template2.CollectQuests == null)
            {
              template2.CollectQuests = new QuestTemplate[1]
              {
                template1
              };
            }
            else
            {
              int num = (int) ArrayUtil.AddOnlyOne(ref template2.CollectQuests, template1);
            }
          }

          foreach(Asda2ItemStackDescription collectableSourceItem in template1.CollectableSourceItems)
          {
            ItemTemplate template2 = GetTemplate(collectableSourceItem.ItemId);
            if(template2 == null)
              ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
                (object) template1, (object) collectableSourceItem);
            else if(template2.CollectQuests == null)
            {
              template2.CollectQuests = new QuestTemplate[1]
              {
                template1
              };
            }
            else
            {
              int num = (int) ArrayUtil.AddOnlyOne(ref template2.CollectQuests, template1);
            }
          }
        }
      }

      foreach(ItemTemplate template1 in Templates)
      {
        if(template1 != null && template1.QuestId != 0U)
        {
          QuestTemplate template2 = QuestMgr.GetTemplate(template1.QuestId);
          if(template2 == null)
            ContentMgr.OnInvalidDBData("Item {0} had invalid QuestId: {1}", (object) template1,
              (object) template1.QuestId);
          else
            template2.Starters.Add(template1);
        }
      }
    }

    /// <summary>
    /// Load item-set info from the DBCs (automatically called on startup)
    /// </summary>
    public static void LoadSets()
    {
      foreach(ItemSet itemSet in new MappedDBCReader<ItemSet, ItemSet.ItemSetDBCConverter>(
        RealmServerConfiguration.GetDBCFile("ItemSet.dbc")).Entries.Values)
      {
        if(itemSet.Id >= Sets.Length)
          Array.Resize(ref Sets, (int) itemSet.Id + 10);
        Sets[(int) itemSet.Id] = itemSet;
      }
    }

    /// <summary>
    /// Resize all Template-Arrays of sets to their actual size.
    /// </summary>
    internal static void TruncSets()
    {
      foreach(ItemSet set in Sets)
      {
        if(set != null)
        {
          for(uint index = 0; (long) index < (long) set.Templates.Length; ++index)
          {
            if(set.Templates[index] == null)
            {
              Array.Resize(ref set.Templates, (int) index);
              break;
            }
          }
        }
      }
    }

    public static void InitItemSlotHandlers()
    {
      PartialInventoryTypes.Fill(PartialInventoryType.Equipment, 0, 18);
      PartialInventoryTypes.Fill(PartialInventoryType.BackPack, 23, 38);
      PartialInventoryTypes.Fill(PartialInventoryType.EquippedContainers, 19, 22);
      PartialInventoryTypes.Fill(PartialInventoryType.Bank, 39, 66);
      PartialInventoryTypes.Fill(PartialInventoryType.BankBags, 67, 73);
      PartialInventoryTypes.Fill(PartialInventoryType.BuyBack, 74, 85);
      PartialInventoryTypes.Fill(PartialInventoryType.KeyRing, 86, 117);
    }

    public static ItemTemplate GetFirstItemOfToolCategory(ToolCategory toolCat)
    {
      return FirstTotemsPerCat[(int) toolCat];
    }

    public static EquipmentSlot[] GetToolCategorySlots(ToolCategory toolCat)
    {
      return FirstTotemsPerCat[(int) toolCat]?.EquipmentSlots;
    }

    public static Dictionary<int, TotemCategoryInfo> ReadTotemCategories()
    {
      return new MappedDBCReader<TotemCategoryInfo, TotemCatConverter>(
        RealmServerConfiguration.GetDBCFile("TotemCategory.dbc")).Entries;
    }

    public static ItemLevelInfo GetLevelInfo(uint itemLevel)
    {
      return new ItemLevelInfo();
    }

    public static ItemRandomPropertyEntry GetRandomPropertyEntry(uint id)
    {
      if(RandomPropertiesReader == null)
        LoadDBCs();
      ItemRandomPropertyEntry randomPropertyEntry;
      RandomPropertiesReader.Entries.TryGetValue((int) id, out randomPropertyEntry);
      return randomPropertyEntry;
    }

    public static ItemRandomSuffixEntry GetRandomSuffixEntry(uint id)
    {
      if(RandomSuffixReader == null)
        LoadDBCs();
      ItemRandomSuffixEntry randomSuffixEntry;
      RandomSuffixReader.Entries.TryGetValue((int) id, out randomSuffixEntry);
      return randomSuffixEntry;
    }

    public static ScalingStatDistributionEntry GetScalingStatDistributionEntry(uint id)
    {
      if(ScalingStatDistributionReader == null)
        LoadDBCs();
      ScalingStatDistributionEntry distributionEntry;
      ScalingStatDistributionReader.Entries.TryGetValue((int) id, out distributionEntry);
      return distributionEntry;
    }

    public static ScalingStatValues GetScalingStatValue(uint id)
    {
      if(ScalingStatValuesReader == null)
        LoadDBCs();
      ScalingStatValues scalingStatValues;
      ScalingStatValuesReader.Entries.TryGetValue((int) id, out scalingStatValues);
      return scalingStatValues;
    }

    /// <summary>
    /// Adds a callback to be called on the given set of ItemTemplates after load and before Item initialization
    /// </summary>
    public static void Apply(Action<ItemTemplate> cb, params Asda2ItemId[] ids)
    {
      foreach(Asda2ItemId id in ids)
        loadHooks.Add(Tuple.Create(id, cb));
    }

    public static void Apply(Action<ItemTemplate> cb, params ItemClass[] classes)
    {
      foreach(ItemClass itemClass in classes)
        itemClassLoadHooks.Add(Tuple.Create(itemClass, cb));
    }

    private static void OnLoaded()
    {
      foreach(Tuple<Asda2ItemId, Action<ItemTemplate>> loadHook in loadHooks)
        loadHook.Item2(GetTemplateForced(loadHook.Item1));
      foreach(Tuple<ItemClass, Action<ItemTemplate>> itemClassLoadHook in itemClassLoadHooks)
      {
        foreach(ItemTemplate template in GetTemplates(itemClassLoadHook.Item1))
          itemClassLoadHook.Item2(template);
      }
    }

    public struct TotemCategoryInfo
    {
      public int Id;
      public string Name;
    }

    public class TotemCatConverter : AdvancedDBCRecordConverter<TotemCategoryInfo>
    {
      public override TotemCategoryInfo ConvertTo(byte[] rawData, ref int id)
      {
        return new TotemCategoryInfo
        {
          Id = id = GetInt32(rawData, 0),
          Name = GetString(rawData, 1)
        };
      }
    }
  }
}