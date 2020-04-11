using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.Quests;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.RealmServer.Items
{
    public enum ItemStatsSlots
    {
        Common = 0,
        Enchant = 1,
        Craft = 2,
        Advanced = 3,
        Any =4
    }

    [DataHolder]
    public class ItemStatsInfo : IDataHolder
    {
        public Asda2Profession ClassMask;
        public Asda2ItemBonusType StatType;
        public ItemStatsSlots StatSlot;
        public Asda2EquipmentSlots ItemType;
        public int BaseValue;
        public int SpreadingPrc;
        public float PerLevelInc;
        public int Id;

        public int Chance;

        public Asda2ItemQuality ReqiredQuality;

        public void FinalizeDataHolder()
        {
            switch (ClassMask)
            {
                case Asda2Profession.Any:
                    InitByStatSlot(Asda2Profession.Mage);
                    InitByStatSlot(Asda2Profession.Warrior);
                    InitByStatSlot(Asda2Profession.Archer);
                    InitByStatSlot(Asda2Profession.NoProfession);
                    break;
                case Asda2Profession.ArcherAndWarrior:
                    InitByStatSlot(Asda2Profession.Warrior);
                    InitByStatSlot(Asda2Profession.Archer);
                    break;
                default:
                    InitByStatSlot(ClassMask);
                    break;
            }
        }

        private void InitByStatSlot(Asda2Profession proff)
        {
            switch (StatSlot)
            {
                case ItemStatsSlots.Any:
                    InitByItemType(ItemStatsSlots.Common, proff);
                    InitByItemType(ItemStatsSlots.Craft, proff);
                    InitByItemType(ItemStatsSlots.Enchant, proff);
                    InitByItemType(ItemStatsSlots.Advanced, proff);
                    break;
                default:
                    InitByItemType(StatSlot, proff);
                    break;
            }
        }

        private void InitByItemType(ItemStatsSlots statSlot, Asda2Profession proffession)
        {
            switch (ItemType)
            {
                case Asda2EquipmentSlots.AnyArmor:
                    Init(Asda2EquipmentSlots.Head, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Shirt, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Boots, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Gloves, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Pans, statSlot, proffession);
                    break;
                case Asda2EquipmentSlots.AnyAvatar:
                    Init(Asda2EquipmentSlots.AvatarBoots, statSlot, proffession);
                    Init(Asda2EquipmentSlots.AvatarGloves, statSlot, proffession);
                    Init(Asda2EquipmentSlots.AvatarHead, statSlot, proffession);
                    Init(Asda2EquipmentSlots.AvatarPans, statSlot, proffession);
                    Init(Asda2EquipmentSlots.AvatarShirt, statSlot, proffession);
                    break;
                case Asda2EquipmentSlots.AnyAvatarAccecory:
                    Init(Asda2EquipmentSlots.AvaratRightHead, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Wings, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Cape, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Accessory, statSlot, proffession);
                    break;
                case Asda2EquipmentSlots.Jevelery:
                    Init(Asda2EquipmentSlots.RightRing, statSlot, proffession);
                    Init(Asda2EquipmentSlots.LeftRing, statSlot, proffession);
                    Init(Asda2EquipmentSlots.Nackles, statSlot, proffession);
                    break;
                default:
                    Init(ItemType, statSlot, proffession);
                    break;
            }
        }

        private void Init(Asda2EquipmentSlots itemType,ItemStatsSlots statSlot,Asda2Profession proffession)
        {
            if (!Asda2ItemMgr.ItemStatsInfos.ContainsKey(proffession))
                Asda2ItemMgr.ItemStatsInfos.Add(proffession,
                                                new Dictionary
                                                    <ItemStatsSlots, Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>>());
            if (!Asda2ItemMgr.ItemStatsInfos[proffession].ContainsKey(statSlot))
                Asda2ItemMgr.ItemStatsInfos[proffession].Add(statSlot, new Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>());
            if (!Asda2ItemMgr.ItemStatsInfos[proffession][statSlot].ContainsKey(itemType))
                Asda2ItemMgr.ItemStatsInfos[proffession][statSlot].Add(itemType, new List<ItemStatsInfo>());
            Asda2ItemMgr.ItemStatsInfos[proffession][statSlot][itemType].Add(this);
            
        }
    }

    [GlobalMgr]
	public static class Asda2ItemMgr
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
        
		public const uint MaxId = 2500000;
        [NotVariable]
        public static Dictionary<Asda2Profession,Dictionary<ItemStatsSlots,Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>>> ItemStatsInfos= new Dictionary<Asda2Profession, Dictionary<ItemStatsSlots, Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>>>();
            /// <summary>
		/// All defined <see cref="ItemTemplate">ItemTemplates</see>.
		/// </summary>
		[NotVariable]
		public static Asda2ItemTemplate[] Templates = new Asda2ItemTemplate[100000];
        [NotVariable]
	    public static Dictionary<int, List<PackageDrop>> PackageDrops = new Dictionary<int, List<PackageDrop>>();
        [NotVariable]
        public static Dictionary<int, List<BoosterDrop>> BoosterDrops = new Dictionary<int, List<BoosterDrop>>();
        [NotVariable]
        public static Dictionary<int, List<DecompositionDrop>> DecompositionDrops = new Dictionary<int, List<DecompositionDrop>>();
        [NotVariable]
        public static WarShopDataRecord[] WarShopDataRecords = new WarShopDataRecord[2000];
        [NotVariable]
        public static Dictionary<int, RegularShopRecord> AvalibleRegularShopItems = new Dictionary<int, RegularShopRecord>();
        
	    /// <summary>
		/// All ItemSet definitions
		/// </summary>
		[NotVariable]
		public static ItemSet[] Sets = new ItemSet[1000];

		//[NotVariable]
		//public static List<ItemRandomPropertyInfo>[] RandomProperties = new List<ItemRandomPropertyInfo>[20000];

		//[NotVariable]
		//public static List<ItemRandomSuffixInfo>[] RandomSuffixes = new List<ItemRandomSuffixInfo>[20000];

		[NotVariable]
		public static MappedDBCReader<ItemLevelInfo, ItemRandPropPointConverter> RandomPropPointReader;

		[NotVariable]
		public static MappedDBCReader<ItemRandomPropertyEntry, ItemRandomPropertiesConverter> RandomPropertiesReader;

		[NotVariable]
		public static MappedDBCReader<ItemRandomSuffixEntry, ItemRandomSuffixConverter> RandomSuffixReader;

        [NotVariable]
        public static MappedDBCReader<ScalingStatDistributionEntry, ScalingStatDistributionConverter> ScalingStatDistributionReader;

        [NotVariable]
        public static MappedDBCReader<ScalingStatValues, ScalingStatValuesConverter> ScalingStatValuesReader;

		/// <summary>
		/// All partial inventory types by InventorySlot
		/// </summary>
		public static readonly PartialInventoryType[] PartialInventoryTypes =
			new PartialInventoryType[(int)InventorySlot.Count];

		/// <summary>
		/// Returns the ItemTemplate with the given id
		/// </summary>
        public static Asda2ItemTemplate GetTemplate(Asda2ItemId id)
		{
		    return (uint)id >= Templates.Length ? null : Templates[(uint)id];
		}

	    public static WarShopDataRecord GetWarshopDataRecord(int id)
        {
            return (uint)id >= WarShopDataRecords.Length ? null : WarShopDataRecords[(uint)id];
        }

	    /// <summary>
		/// Returns the ItemTemplate with the given id
		/// </summary>
		public static Asda2ItemTemplate GetTemplateForced(Asda2ItemId id)
		{
			Asda2ItemTemplate templ;
			if ((uint)id >= Templates.Length)
			{
				templ = null;
			}
			else
			{
				templ = Templates[(uint)id];
			}

			if (templ == null)
			{
				throw new ContentException("Requested ItemTemplate does not exist: {0}", id);
			}
			return templ;
		}

		/// <summary>
		/// Returns the ItemTemplate with the given id
		/// </summary>
		public static Asda2ItemTemplate GetTemplate(int id)
		{
			if (id >= Templates.Length || id < 0)
			{
				return null;
			}
			return Templates[id];
		}

        [NotVariable]
        public static AvatarDisasembleRecord[] RegularAvatarRecords = new AvatarDisasembleRecord[100];
        [NotVariable]
        public static AvatarDisasembleRecord[] PremiumAvatarRecords = new AvatarDisasembleRecord[100];
		/// <summary>
		/// Returns the ItemSet with the given id
		/// </summary>
		public static ItemSet GetSet(ItemSetId id)
		{
			if ((uint)id >= Sets.Length)
			{
				return null;
			}
			return Sets[(uint)id];
		}

		/// <summary>
		/// Returns the ItemSet with the given id
		/// </summary>
		public static ItemSet GetSet(uint id)
		{
			if (id >= Sets.Length)
			{
				return null;
			}
			return Sets[id];
		}

		#region Slot Mapping

		public static readonly EquipmentSlot[] AllBagSlots = new[] { EquipmentSlot.Bag1, EquipmentSlot.Bag2, EquipmentSlot.Bag3, EquipmentSlot.Bag4 };

		public static EquipmentSlot[] GetEquipmentSlots(InventorySlotType invSlot)
		{
			return EquipmentSlotsByInvSlot[(int)invSlot];
		}

		/// <summary>
		/// Maps a set of available InventorySlots by their corresponding InventorySlotType
		/// </summary>
		public static readonly EquipmentSlot[][] EquipmentSlotsByInvSlot = GetEqByInv();

		public static readonly InventorySlot[][] EquippableInvSlotsByClass = GetEqByCl();

		static EquipmentSlot[][] GetEqByInv()
		{
			var slots = new EquipmentSlot[1 + (int)Utility.GetMaxEnum<InventorySlotType>()][];

			slots[(int)InventorySlotType.Bag] = AllBagSlots;
			slots[(int)InventorySlotType.Body] = new[] { EquipmentSlot.Shirt };
			slots[(int)InventorySlotType.Chest] = new[] { EquipmentSlot.Chest };
			slots[(int)InventorySlotType.Cloak] = new[] { EquipmentSlot.Back };
			slots[(int)InventorySlotType.Feet] = new[] { EquipmentSlot.Boots };
			slots[(int)InventorySlotType.Finger] = new[] { EquipmentSlot.Finger1, EquipmentSlot.Finger2 };
			slots[(int)InventorySlotType.Hand] = new[] { EquipmentSlot.Gloves };
			slots[(int)InventorySlotType.Head] = new[] { EquipmentSlot.Head };
			slots[(int)InventorySlotType.Holdable] = new[] { EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.Legs] = new[] { EquipmentSlot.Pants };
			slots[(int)InventorySlotType.Neck] = new[] { EquipmentSlot.Neck };
			slots[(int)InventorySlotType.Quiver] = AllBagSlots;
			slots[(int)InventorySlotType.WeaponRanged] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.RangedRight] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.Relic] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.Robe] = new[] { EquipmentSlot.Chest };
			slots[(int)InventorySlotType.Shield] = new[] { EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.Shoulder] = new[] { EquipmentSlot.Shoulders };
			slots[(int)InventorySlotType.Tabard] = new[] { EquipmentSlot.Tabard };
			slots[(int)InventorySlotType.Thrown] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.Trinket] = new[] { EquipmentSlot.Trinket1, EquipmentSlot.Trinket2 };
			slots[(int)InventorySlotType.TwoHandWeapon] = new[] { EquipmentSlot.MainHand };
			slots[(int)InventorySlotType.Waist] = new[] { EquipmentSlot.Belt };
			slots[(int)InventorySlotType.Weapon] = new[] { EquipmentSlot.MainHand, EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.WeaponMainHand] = new[] { EquipmentSlot.MainHand };
			slots[(int)InventorySlotType.WeaponOffHand] = new[] { EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.Wrist] = new[] { EquipmentSlot.Wrist };

			// special treatment
			slots[(int)InventorySlotType.Ammo] = null; // new[] { EquipmentSlot.Invalid };
			return slots;
		}

		private static InventorySlot[][] GetEqByCl()
		{
			var slots = new InventorySlot[(int)ItemClass.End][];
			slots[(int)ItemClass.Weapon] = new[] { InventorySlot.MainHand, InventorySlot.OffHand, InventorySlot.ExtraWeapon };
			slots[(int)ItemClass.Armor] = new[] { InventorySlot.Chest, InventorySlot.Boots, InventorySlot.Gloves, InventorySlot.Head,
				InventorySlot.Pants, InventorySlot.Chest, InventorySlot.Shoulders, InventorySlot.Wrist, InventorySlot.Belt };
			return slots;
		}
		#endregion
#region asda2 item enchanting
        public static int GetEnchantPrice(Byte enchLevel, int itemLevel, Asda2ItemQuality rarity)
        {
            var startPrice = GetStartPrice(itemLevel);
            var step = GetStep(rarity);
            var resultPrice = startPrice;
            for (int i = 0; i < enchLevel; i++)
            {
                resultPrice += step * GetMult(i + 1);
            }
            return resultPrice;
        }

        private static int GetMult(int lvl)
        {
            if (lvl < 3)
            {
                return 1;
            }

            return lvl % 5 == 0 ? 4 : 2;
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
            }
            return 0;
        }

        private static int GetStartPrice(int level)
        {
           if(level<20)
                    return 0;
            if(level<40)
                    return 500;
             if(level<60)
                    return 2000;
             if(level<80)
                    return 5000;
               if(level<100)
                    return 10000;
            return 0;
        }
#endregion
		#region Slots
		public static readonly InventorySlot[] EquipmentSlots = new[] {
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
			InventorySlot.Finger1,
			InventorySlot.Finger2,
			InventorySlot.Trinket1,
			InventorySlot.Trinket2,
			InventorySlot.Back,
			InventorySlot.MainHand,
			InventorySlot.OffHand,
			InventorySlot.ExtraWeapon,
			InventorySlot.Tabard
		};

		/// <summary>
		/// Contains all InventorySlots that are used as storage on the Character, without bank slots
		/// </summary>
		public static readonly InventorySlot[] StorageSlotsWithoutBank = new[] {
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
		public static readonly InventorySlot[] InvSlots = new[] {
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
			InventorySlot.Finger1,
			InventorySlot.Finger2,
			InventorySlot.Trinket1,
			InventorySlot.Trinket2,
			InventorySlot.Back,
			InventorySlot.MainHand,
			InventorySlot.OffHand,
			InventorySlot.ExtraWeapon,
			InventorySlot.Tabard,
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
		public static readonly InventorySlot[] InvSlotsWithBank = new[] {
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
			InventorySlot.Finger1,
			InventorySlot.Finger2,
			InventorySlot.Trinket1,
			InventorySlot.Trinket2,
			InventorySlot.Back,
			InventorySlot.MainHand,
			InventorySlot.OffHand,
			InventorySlot.ExtraWeapon,
			InventorySlot.Tabard,
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


		/// <summary>
		/// Contains all BankSlots
		/// </summary>
		public readonly static InventorySlot[] BankSlots = new[] {
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

		/// <summary>
		/// Contains all InventorySlots for BankBags
		/// </summary>
		public static readonly InventorySlot[] BankBagSlots = new[]
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
		public static readonly InventorySlot[] OwnedSlots = new[]
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
		                                           		InventorySlot.Finger1,
		                                           		InventorySlot.Finger2,
		                                           		InventorySlot.Trinket1,
		                                           		InventorySlot.Trinket2,
		                                           		InventorySlot.Back,
		                                           		InventorySlot.MainHand,
		                                           		InventorySlot.OffHand,
		                                           		InventorySlot.ExtraWeapon,
		                                           		InventorySlot.Tabard,
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

		[NotVariable]
		/// <summary>
		/// All slots that can contain Items
		/// </summary>
		public static InventorySlot[] AllSlots;

		[NotVariable]
		/// <summary>
		/// Contains true for all InventorySlots that can contain equippable bags (4 bag slots and up to 7 bank bag slots)
		/// </summary>
		public static bool[] ContainerSlotsWithBank;

		[NotVariable]
		/// <summary>
		/// Contains true for all bag-InventorySlots
		/// </summary>
		public static bool[] ContainerSlotsWithoutBank;

		[NotVariable]
		/// <summary>
		/// Contains true for all InventorySlots that represent BankBags
		/// </summary>
		public static bool[] ContainerBankSlots;

		public static bool IsContainerEquipmentSlot(int slot)
		{
			return (slot >= (int)InventorySlot.Bag1 && slot <= (int)InventorySlot.BagLast) ||
				(slot >= (int)InventorySlot.BankBag1 && slot <= (int)InventorySlot.BankBagLast);
		}
		#endregion

		static Asda2ItemMgr()
		{
			var list = ((InventorySlot[])Enum.GetValues(typeof(InventorySlot))).ToList();
			list.Remove(InventorySlot.Count);
			list.Remove(InventorySlot.Invalid);
			AllSlots = list.ToArray();
		}

		#region Load & Initialize
		[Initialization(InitializationPass.Fourth, "Initialize Items")]
		public static void Initialize()
		{
			LoadAll();
            Asda2CryptHelper.InitTransparations();
		}

		public static void ForceInitialize()
		{
			if (!Loaded)
			{
				LoadAll();
			}
		}



		public static bool Loaded { get; private set; }
        [NotVariable]
	    public static Asda2BossSummonRecord[] SummonRecords = new Asda2BossSummonRecord[100000];
        [NotVariable]
        public static Asda2GuildWaveItemRecord[] GuildWaveRewardRecords = new Asda2GuildWaveItemRecord[100000];
        [NotVariable]
	    public static ItemCombineDataRecord[] ItemCombineRecords = new ItemCombineDataRecord[1000];

	    public static void LoadAll()
		{
            
			
            if (!Loaded)
            {
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
                OnLoaded();
                foreach (var templ in Templates)
                {
                    if (templ != null)
                    {
                        templ.InitializeTemplate();
                    }
                }
                Loaded = true;
            }
		}

		private static void LoadItemCharRelations()
		{
			foreach (var chr in World.GetAllCharacters())
			{
				var context = chr.ContextHandler;
				if (context != null)
				{
					var character = chr;
					context.AddMessage(() =>
					{
						if (character.IsInWorld)
						{
							character.InitItems();
						}
					});
				}
			}
		}

		internal static void EnsureItemQuestRelations()
		{
			// Collect quests
			foreach (var quest in QuestMgr.Templates)
			{
				if (quest == null)
				{
					continue;
				}
				if (quest.CollectableItems == null)
				{
					continue;
				}

				foreach (var itemInfo in quest.CollectableItems)
				{
					var item = GetTemplate(itemInfo.ItemId);
					if (item == null)
					{
						ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
													   quest, itemInfo);
					}
					else
					{
						if (item.CollectQuests == null)
						{
							item.CollectQuests = new[] { quest };
						}
						else
						{
							ArrayUtil.AddOnlyOne(ref item.CollectQuests, quest);
						}
					}
				}

                foreach (var itemInfo in quest.CollectableSourceItems)
                {
                    var item = GetTemplate(itemInfo.ItemId);
                    if (item == null)
                    {
                        ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
                                                       quest, itemInfo);
                    }
                    else
                    {
                        if (item.CollectQuests == null)
                        {
                            item.CollectQuests = new[] { quest };
                        }
                        else
                        {
                            ArrayUtil.AddOnlyOne(ref item.CollectQuests, quest);
                        }
                    }
                }
			}

			// Item QuestGivers
			foreach (var item in Templates)
			{
				if (item != null && item.QuestId != 0)
				{
					var quest = QuestMgr.GetTemplate(item.QuestId);
					if (quest == null)
					{
						ContentMgr.OnInvalidDBData("Item {0} had invalid QuestId: {1}", item, item.QuestId);
						continue;
					}
					quest.Starters.Add(item);
				}
			}
		}

		/// <summary>
		/// Load item-set info from the DBCs (automatically called on startup)
		/// </summary>
		public static void LoadSets()
		{
			var reader = new MappedDBCReader<ItemSet, ItemSet.ItemSetDBCConverter>(
                RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_ITEMSET));

			foreach (var set in reader.Entries.Values)
			{
				if (set.Id >= Sets.Length)
				{
					Array.Resize(ref Sets, (int)set.Id + 10);
				}
				Sets[(int)set.Id] = set;
			}
		}

		/// <summary>
		/// Resize all Template-Arrays of sets to their actual size.
		/// </summary>
		internal static void TruncSets()
		{
			foreach (var set in Sets)
			{
				if (set != null)
				{
					for (uint i = 0; i < set.Templates.Length; i++)
					{
						if (set.Templates[i] == null)
						{
							// truncate
							Array.Resize(ref set.Templates, (int)i);
							break;
						}
					}
				}
			}
		}
		#endregion

		#region PartialInventoryTypes

		public static void InitItemSlotHandlers()
		{
			PartialInventoryTypes.Fill(PartialInventoryType.Equipment, (int)InventorySlot.Head,
						 (int)InventorySlot.Tabard);
			PartialInventoryTypes.Fill(PartialInventoryType.BackPack, (int)InventorySlot.BackPack1,
						 (int)InventorySlot.BackPackLast);
			PartialInventoryTypes.Fill(PartialInventoryType.EquippedContainers, (int)InventorySlot.Bag1,
						 (int)InventorySlot.BagLast);
			PartialInventoryTypes.Fill(PartialInventoryType.Bank, (int)InventorySlot.Bank1,
						 (int)InventorySlot.BankLast);
			PartialInventoryTypes.Fill(PartialInventoryType.BankBags, (int)InventorySlot.BankBag1,
						 (int)InventorySlot.BankBagLast);
			PartialInventoryTypes.Fill(PartialInventoryType.BuyBack, (int)InventorySlot.BuyBack1,
						 (int)InventorySlot.BuyBackLast);
			PartialInventoryTypes.Fill(PartialInventoryType.KeyRing, (int)InventorySlot.Key1,
						 (int)InventorySlot.KeyLast);
		}

		#endregion

		#region TotemCategories
		[NotVariable]
		public static readonly Asda2ItemTemplate[] FirstTotemsPerCat = new Asda2ItemTemplate[(uint)ToolCategory.End + 100];

		public static Asda2ItemTemplate GetFirstItemOfToolCategory(ToolCategory toolCat)
		{
			return FirstTotemsPerCat[(int)toolCat];
		}

		/*public static EquipmentSlot[] GetToolCategorySlots(ToolCategory toolCat)
		{
			var templ = FirstTotemsPerCat[(int) toolCat];
			if (templ == null) return null;

			return templ.EquipmentSlots;
		}*/

		public static Dictionary<int, TotemCategoryInfo> ReadTotemCategories()
		{
			var reader = new MappedDBCReader<TotemCategoryInfo, TotemCatConverter>(RealmServerConfiguration.GetDBCFile(
                                                                                    WCellConstants.DBC_TOTEMCATEGORY));
			return reader.Entries;
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
				var cat = new TotemCategoryInfo
				{
					Id = (id = GetInt32(rawData, 0)),
					Name = GetString(rawData, 1)
				};

				return cat;
			}
		}
		#endregion
        
		#region Apply changes when loading
		private static readonly List<Tuple<Asda2ItemId, Action<Asda2ItemTemplate>>> loadHooks = new List<Tuple<Asda2ItemId, Action<Asda2ItemTemplate>>>();
		private static readonly List<Tuple<ItemClass, Action<Asda2ItemTemplate>>> itemClassLoadHooks = new List<Tuple<ItemClass, Action<Asda2ItemTemplate>>>();
		/// <summary>
		/// Adds a callback to be called on the given set of ItemTemplates after load and before Item initialization
		/// </summary>
		public static void Apply(Action<Asda2ItemTemplate> cb, params Asda2ItemId[] ids)
		{
			foreach (var id in ids)
			{
				loadHooks.Add(Tuple.Create(id, cb));
			}
		}

		public static void Apply(Action<Asda2ItemTemplate> cb, params ItemClass[] classes)
		{
			foreach (var cls in classes)
			{
				itemClassLoadHooks.Add(Tuple.Create(cls, cb));
			}
		}

		static void OnLoaded()
		{
            // todo asda2 OnLoaded
			// Perform the action on a template
			/*foreach (var hook in loadHooks)
			{
				hook.Item2(GetTemplateForced(hook.Item1));
			}

			// Perform an action an each member of the itemclasses
			foreach(var hook in itemClassLoadHooks)
			{
				foreach(var template in GetTemplates(hook.Item1))
				{
					hook.Item2(template);
				}
			}*/
		}
		#endregion
	}

    [DataHolder]
    public class Asda2GuildWaveItemRecord : IDataHolder
    {
        public int Id
        {
            get;
            set;
        }

        public int Wave
        {
            get;
            set;
        }

        public int Lvl
        {
            get;
            set;
        }

        public int Difficulty
        {
            get;
            set;
        }

        public int Item1
        {
            get;
            set;
        }

        public int Item2
        {
            get;
            set;
        }

        public int Item3
        {
            get;
            set;
        }

        public int Item4
        {
            get;
            set;
        }

        public int Item5
        {
            get;
            set;
        }

        public int Item6
        {
            get;
            set;
        }

        public int Item7
        {
            get;
            set;
        }

        public int Item8
        {
            get;
            set;
        }

        public int Chance1
        {
            get;
            set;
        }

        public int Chance2
        {
            get;
            set;
        }

        public int Chance3
        {
            get;
            set;
        }

        public int Chance4
        {
            get;
            set;
        }

        public int Chance5
        {
            get;
            set;
        }

        public int Chance6
        {
            get;
            set;
        }

        public int Chance7
        {
            get;
            set;
        }

        public int Chance8
        {
            get;
            set;
        }

        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.GuildWaveRewardRecords.SetValue(this, Id);
        }
    }

    [DataHolder]
    public class Asda2BossSummonRecord : IDataHolder
    {
        public int Id { get; set; }
        public byte Amount { get; set; }
        public NPCId MobId { get; set; }
        public MapId MapId { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.SummonRecords.SetValue(this,Id);
        }
    }

    [DataHolder]
    public class ItemCombineDataRecord : IDataHolder
    {
        public int Id { get; set; }
        [Persistent(5)]
        public int[] RequiredItems { get; set; }
        [Persistent(5)]
        public int[] Amounts { get; set; }
        public int ResultItem { get; set; }
        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.ItemCombineRecords.SetValue(this, Id);
        }
    }
    [DataHolder]
    public class AvatarDisasembleRecord : IDataHolder
    {
        public int Id { get; set; }
        public int IsRegular { get; set; }
        public int Level { get; set; }
        [Persistent (Length = 10)]
        public int[] ItemIds { get; set; }
        [Persistent(Length = 10)]
        public int[] Chances { get; set; }
        public string ChancesAsString
        {
            get
            {
                var r = Chances.Aggregate("", (current, i) => current + (i.ToString(CultureInfo.InvariantCulture) + ","));
                return r;
            }
        }

        public void FinalizeDataHolder()
        {
            if (IsRegular == 0)
                Asda2ItemMgr.RegularAvatarRecords.SetValue(this, Id);
            else
                Asda2ItemMgr.PremiumAvatarRecords.SetValue(this, Id);
        }

        public Asda2ItemId GetRandomItemId()
        {
            var rnd = Utility.Random(0, 100000);
            var i = 0;
            for (int j = 0; j < ItemIds.Length; j++)
            {
                i += Chances[j];
                if (i >= rnd)
                    return (Asda2ItemId) ItemIds[j];
            }
            return (Asda2ItemId) 31175;
        }
    }
}