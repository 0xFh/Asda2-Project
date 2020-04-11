using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Items
{
    [GlobalMgr]
    public static class Asda2ItemMgr
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        [NotVariable]
        public static
            Dictionary<Asda2Profession, Dictionary<ItemStatsSlots, Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>>
            > ItemStatsInfos =
                new Dictionary<Asda2Profession,
                    Dictionary<ItemStatsSlots, Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>>>();

        /// <summary>
        /// All defined <see cref="T:WCell.RealmServer.Items.ItemTemplate">ItemTemplates</see>.
        /// </summary>
        [NotVariable] public static Asda2ItemTemplate[] Templates = new Asda2ItemTemplate[100000];

        [NotVariable]
        public static Dictionary<int, List<PackageDrop>> PackageDrops = new Dictionary<int, List<PackageDrop>>();

        [NotVariable]
        public static Dictionary<int, List<BoosterDrop>> BoosterDrops = new Dictionary<int, List<BoosterDrop>>();

        [NotVariable]
        public static Dictionary<int, List<DecompositionDrop>> DecompositionDrops =
            new Dictionary<int, List<DecompositionDrop>>();

        [NotVariable] public static WarShopDataRecord[] WarShopDataRecords = new WarShopDataRecord[2000];

        [NotVariable]
        public static Dictionary<int, RegularShopRecord> AvalibleRegularShopItems =
            new Dictionary<int, RegularShopRecord>();

        /// <summary>All ItemSet definitions</summary>
        [NotVariable] public static ItemSet[] Sets = new ItemSet[1000];

        /// <summary>All partial inventory types by InventorySlot</summary>
        public static readonly PartialInventoryType[] PartialInventoryTypes = new PartialInventoryType[118];

        [NotVariable] public static AvatarDisasembleRecord[] RegularAvatarRecords = new AvatarDisasembleRecord[100];
        [NotVariable] public static AvatarDisasembleRecord[] PremiumAvatarRecords = new AvatarDisasembleRecord[100];

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
        public static readonly EquipmentSlot[][] EquipmentSlotsByInvSlot = Asda2ItemMgr.GetEqByInv();

        public static readonly InventorySlot[][] EquippableInvSlotsByClass = Asda2ItemMgr.GetEqByCl();

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

        [NotVariable] public static Asda2BossSummonRecord[] SummonRecords = new Asda2BossSummonRecord[100000];

        [NotVariable]
        public static Asda2GuildWaveItemRecord[] GuildWaveRewardRecords = new Asda2GuildWaveItemRecord[100000];

        [NotVariable] public static ItemCombineDataRecord[] ItemCombineRecords = new ItemCombineDataRecord[1000];
        [NotVariable] public static readonly Asda2ItemTemplate[] FirstTotemsPerCat = new Asda2ItemTemplate[291];

        private static readonly List<Tuple<Asda2ItemId, Action<Asda2ItemTemplate>>> loadHooks =
            new List<Tuple<Asda2ItemId, Action<Asda2ItemTemplate>>>();

        private static readonly List<Tuple<ItemClass, Action<Asda2ItemTemplate>>> itemClassLoadHooks =
            new List<Tuple<ItemClass, Action<Asda2ItemTemplate>>>();

        public const uint MaxId = 2500000;
        [NotVariable] public static MappedDBCReader<ItemLevelInfo, ItemRandPropPointConverter> RandomPropPointReader;

        [NotVariable]
        public static MappedDBCReader<ItemRandomPropertyEntry, ItemRandomPropertiesConverter> RandomPropertiesReader;

        [NotVariable]
        public static MappedDBCReader<ItemRandomSuffixEntry, ItemRandomSuffixConverter> RandomSuffixReader;

        [NotVariable]
        public static MappedDBCReader<ScalingStatDistributionEntry, ScalingStatDistributionConverter>
            ScalingStatDistributionReader;

        [NotVariable]
        public static MappedDBCReader<ScalingStatValues, ScalingStatValuesConverter> ScalingStatValuesReader;

        [NotVariable] public static InventorySlot[] AllSlots;
        [NotVariable] public static bool[] ContainerSlotsWithBank;
        [NotVariable] public static bool[] ContainerSlotsWithoutBank;
        [NotVariable] public static bool[] ContainerBankSlots;

        /// <summary>Returns the ItemTemplate with the given id</summary>
        public static Asda2ItemTemplate GetTemplate(Asda2ItemId id)
        {
            if ((long) id < (long) Asda2ItemMgr.Templates.Length)
                return Asda2ItemMgr.Templates[(uint) id];
            return (Asda2ItemTemplate) null;
        }

        public static WarShopDataRecord GetWarshopDataRecord(int id)
        {
            if ((long) (uint) id < (long) Asda2ItemMgr.WarShopDataRecords.Length)
                return Asda2ItemMgr.WarShopDataRecords[(uint) id];
            return (WarShopDataRecord) null;
        }

        /// <summary>Returns the ItemTemplate with the given id</summary>
        public static Asda2ItemTemplate GetTemplateForced(Asda2ItemId id)
        {
            Asda2ItemTemplate asda2ItemTemplate = (long) id < (long) Asda2ItemMgr.Templates.Length
                ? Asda2ItemMgr.Templates[(uint) id]
                : (Asda2ItemTemplate) null;
            if (asda2ItemTemplate == null)
                throw new ContentException("Requested ItemTemplate does not exist: {0}", new object[1]
                {
                    (object) id
                });
            return asda2ItemTemplate;
        }

        /// <summary>Returns the ItemTemplate with the given id</summary>
        public static Asda2ItemTemplate GetTemplate(int id)
        {
            if (id >= Asda2ItemMgr.Templates.Length || id < 0)
                return (Asda2ItemTemplate) null;
            return Asda2ItemMgr.Templates[id];
        }

        /// <summary>Returns the ItemSet with the given id</summary>
        public static ItemSet GetSet(ItemSetId id)
        {
            if ((long) id >= (long) Asda2ItemMgr.Sets.Length)
                return (ItemSet) null;
            return Asda2ItemMgr.Sets[(uint) id];
        }

        /// <summary>Returns the ItemSet with the given id</summary>
        public static ItemSet GetSet(uint id)
        {
            if ((long) id >= (long) Asda2ItemMgr.Sets.Length)
                return (ItemSet) null;
            return Asda2ItemMgr.Sets[id];
        }

        public static EquipmentSlot[] GetEquipmentSlots(InventorySlotType invSlot)
        {
            return Asda2ItemMgr.EquipmentSlotsByInvSlot[(int) invSlot];
        }

        private static EquipmentSlot[][] GetEqByInv()
        {
            EquipmentSlot[][] equipmentSlotArray =
                new EquipmentSlot[(int) (1 + Utility.GetMaxEnum<InventorySlotType>())][];
            equipmentSlotArray[18] = Asda2ItemMgr.AllBagSlots;
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
            equipmentSlotArray[27] = Asda2ItemMgr.AllBagSlots;
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
            equipmentSlotArray[24] = (EquipmentSlot[]) null;
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

        public static int GetEnchantPrice(byte enchLevel, int itemLevel, Asda2ItemQuality rarity)
        {
            int startPrice = Asda2ItemMgr.GetStartPrice(itemLevel);
            int step = Asda2ItemMgr.GetStep(rarity);
            int num = startPrice;
            for (int index = 0; index < (int) enchLevel; ++index)
                num += step * Asda2ItemMgr.GetMult(index + 1);
            return num;
        }

        private static int GetMult(int lvl)
        {
            if (lvl < 3)
                return 1;
            return lvl % 5 != 0 ? 2 : 4;
        }

        private static int GetStep(Asda2ItemQuality rarity)
        {
            switch (rarity)
            {
                case Asda2ItemQuality.White:
                    return 5;
                case Asda2ItemQuality.Yello:
                    return 25;
                case Asda2ItemQuality.Purple:
                    return 250;
                case Asda2ItemQuality.Green:
                    return 1250;
                case Asda2ItemQuality.Orange:
                    return 1000;
                default:
                    return 0;
            }
        }

        private static int GetStartPrice(int level)
        {
            if (level < 20)
                return 0;
            if (level < 40)
                return 500;
            if (level < 60)
                return 2000;
            if (level < 80)
                return 5000;
            return level < 100 ? 10000 : 0;
        }

        public static bool IsContainerEquipmentSlot(int slot)
        {
            if (slot >= 19 && slot <= 22)
                return true;
            if (slot >= 67)
                return slot <= 73;
            return false;
        }

        static Asda2ItemMgr()
        {
            List<InventorySlot> list = ((IEnumerable<InventorySlot>) Enum.GetValues(typeof(InventorySlot)))
                .ToList<InventorySlot>();
            list.Remove(InventorySlot.Count);
            list.Remove(InventorySlot.Invalid);
            Asda2ItemMgr.AllSlots = list.ToArray();
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fourth, "Initialize Items")]
        public static void Initialize()
        {
            Asda2ItemMgr.LoadAll();
            Asda2CryptHelper.InitTransparations();
        }

        public static void ForceInitialize()
        {
            if (Asda2ItemMgr.Loaded)
                return;
            Asda2ItemMgr.LoadAll();
        }

        public static bool Loaded { get; private set; }

        public static void LoadAll()
        {
            if (Asda2ItemMgr.Loaded)
                return;
            ContentMgr.Load<ItemStatsInfo>();
            ContentMgr.Load<RegularShopRecord>();
            ContentMgr.Load<Asda2ItemTemplate>();
            ContentMgr.Load<BoosterDrop>();
            ContentMgr.Load<PackageDrop>();
            ContentMgr.Load<DecompositionDrop>();
            ContentMgr.Load<WarShopDataRecord>();
            ContentMgr.Load<AvatarDisasembleRecord>();
            ContentMgr.Load<Asda2BossSummonRecord>();
            ContentMgr.Load<ItemCombineDataRecord>();
            ContentMgr.Load<Asda2GuildWaveItemRecord>();
            Asda2ItemMgr.OnLoaded();
            foreach (Asda2ItemTemplate template in Asda2ItemMgr.Templates)
            {
                if (template != null)
                    template.InitializeTemplate();
            }

            Asda2ItemMgr.Loaded = true;
        }

        private static void LoadItemCharRelations()
        {
            foreach (Character allCharacter in World.GetAllCharacters())
            {
                IContextHandler contextHandler = allCharacter.ContextHandler;
                if (contextHandler != null)
                {
                    Character character = allCharacter;
                    contextHandler.AddMessage((Action) (() =>
                    {
                        if (!character.IsInWorld)
                            return;
                        character.InitItems();
                    }));
                }
            }
        }

        internal static void EnsureItemQuestRelations()
        {
            foreach (QuestTemplate template1 in QuestMgr.Templates)
            {
                if (template1 != null && template1.CollectableItems != null)
                {
                    foreach (Asda2ItemStackDescription collectableItem in template1.CollectableItems)
                    {
                        Asda2ItemTemplate template2 = Asda2ItemMgr.GetTemplate(collectableItem.ItemId);
                        if (template2 == null)
                            ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
                                (object) template1, (object) collectableItem);
                        else if (template2.CollectQuests == null)
                        {
                            template2.CollectQuests = new QuestTemplate[1]
                            {
                                template1
                            };
                        }
                        else
                        {
                            int num = (int) ArrayUtil.AddOnlyOne<QuestTemplate>(ref template2.CollectQuests, template1);
                        }
                    }

                    foreach (Asda2ItemStackDescription collectableSourceItem in template1.CollectableSourceItems)
                    {
                        Asda2ItemTemplate template2 = Asda2ItemMgr.GetTemplate(collectableSourceItem.ItemId);
                        if (template2 == null)
                            ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
                                (object) template1, (object) collectableSourceItem);
                        else if (template2.CollectQuests == null)
                        {
                            template2.CollectQuests = new QuestTemplate[1]
                            {
                                template1
                            };
                        }
                        else
                        {
                            int num = (int) ArrayUtil.AddOnlyOne<QuestTemplate>(ref template2.CollectQuests, template1);
                        }
                    }
                }
            }

            foreach (Asda2ItemTemplate template1 in Asda2ItemMgr.Templates)
            {
                if (template1 != null && template1.QuestId != 0U)
                {
                    QuestTemplate template2 = QuestMgr.GetTemplate(template1.QuestId);
                    if (template2 == null)
                        ContentMgr.OnInvalidDBData("Item {0} had invalid QuestId: {1}", (object) template1,
                            (object) template1.QuestId);
                    else
                        template2.Starters.Add((IQuestHolderEntry) template1);
                }
            }
        }

        /// <summary>
        /// Load item-set info from the DBCs (automatically called on startup)
        /// </summary>
        public static void LoadSets()
        {
            foreach (ItemSet itemSet in new MappedDBCReader<ItemSet, ItemSet.ItemSetDBCConverter>(
                RealmServerConfiguration.GetDBCFile("ItemSet.dbc")).Entries.Values)
            {
                if ((long) itemSet.Id >= (long) Asda2ItemMgr.Sets.Length)
                    Array.Resize<ItemSet>(ref Asda2ItemMgr.Sets, (int) itemSet.Id + 10);
                Asda2ItemMgr.Sets[(int) itemSet.Id] = itemSet;
            }
        }

        /// <summary>
        /// Resize all Template-Arrays of sets to their actual size.
        /// </summary>
        internal static void TruncSets()
        {
            foreach (ItemSet set in Asda2ItemMgr.Sets)
            {
                if (set != null)
                {
                    for (uint index = 0; (long) index < (long) set.Templates.Length; ++index)
                    {
                        if (set.Templates[index] == null)
                        {
                            Array.Resize<ItemTemplate>(ref set.Templates, (int) index);
                            break;
                        }
                    }
                }
            }
        }

        public static void InitItemSlotHandlers()
        {
            Asda2ItemMgr.PartialInventoryTypes.Fill<PartialInventoryType>(PartialInventoryType.Equipment, 0, 18);
            Asda2ItemMgr.PartialInventoryTypes.Fill<PartialInventoryType>(PartialInventoryType.BackPack, 23, 38);
            Asda2ItemMgr.PartialInventoryTypes.Fill<PartialInventoryType>(PartialInventoryType.EquippedContainers, 19,
                22);
            Asda2ItemMgr.PartialInventoryTypes.Fill<PartialInventoryType>(PartialInventoryType.Bank, 39, 66);
            Asda2ItemMgr.PartialInventoryTypes.Fill<PartialInventoryType>(PartialInventoryType.BankBags, 67, 73);
            Asda2ItemMgr.PartialInventoryTypes.Fill<PartialInventoryType>(PartialInventoryType.BuyBack, 74, 85);
            Asda2ItemMgr.PartialInventoryTypes.Fill<PartialInventoryType>(PartialInventoryType.KeyRing, 86, 117);
        }

        public static Asda2ItemTemplate GetFirstItemOfToolCategory(ToolCategory toolCat)
        {
            return Asda2ItemMgr.FirstTotemsPerCat[(int) toolCat];
        }

        public static Dictionary<int, Asda2ItemMgr.TotemCategoryInfo> ReadTotemCategories()
        {
            return new MappedDBCReader<Asda2ItemMgr.TotemCategoryInfo, Asda2ItemMgr.TotemCatConverter>(
                RealmServerConfiguration.GetDBCFile("TotemCategory.dbc")).Entries;
        }

        /// <summary>
        /// Adds a callback to be called on the given set of ItemTemplates after load and before Item initialization
        /// </summary>
        public static void Apply(Action<Asda2ItemTemplate> cb, params Asda2ItemId[] ids)
        {
            foreach (Asda2ItemId id in ids)
                Asda2ItemMgr.loadHooks.Add(Tuple.Create<Asda2ItemId, Action<Asda2ItemTemplate>>(id, cb));
        }

        public static void Apply(Action<Asda2ItemTemplate> cb, params ItemClass[] classes)
        {
            foreach (ItemClass itemClass in classes)
                Asda2ItemMgr.itemClassLoadHooks.Add(Tuple.Create<ItemClass, Action<Asda2ItemTemplate>>(itemClass, cb));
        }

        private static void OnLoaded()
        {
        }

        public struct TotemCategoryInfo
        {
            public int Id;
            public string Name;
        }

        public class TotemCatConverter : AdvancedDBCRecordConverter<Asda2ItemMgr.TotemCategoryInfo>
        {
            public override Asda2ItemMgr.TotemCategoryInfo ConvertTo(byte[] rawData, ref int id)
            {
                return new Asda2ItemMgr.TotemCategoryInfo()
                {
                    Id = id = DBCRecordConverter.GetInt32(rawData, 0),
                    Name = this.GetString(rawData, 1)
                };
            }
        }
    }
}