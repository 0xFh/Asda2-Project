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
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Chat;
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
using WCell.Util.NLog;

namespace WCell.RealmServer.Items
{
    public class ItemStatBonus
    {
        public Asda2ItemBonusType Type;
        public short MinValue;
        public short MaxValue;
        public int Chance;
        public short GetValue()
        {
            return (short)Utility.Random(MinValue, MaxValue);
        }
    }

    public class ItemStatGenerator
    {
        public static ItemStatBonus EmptyBonus = new ItemStatBonus { Chance = MaximumChance, Type = Asda2ItemBonusType.None, MinValue = 0, MaxValue = 0 };
        public const int MaximumChance = 1000000000;
        public List<ItemStatBonus> PosibleBonuses = new List<ItemStatBonus>();

        public ItemStatBonus GetBonus()
        {
            var rnd = Utility.Random(0, MaximumChance);
            var currentChance = 0;
            foreach (var itemStatBonus in PosibleBonuses)
            {
                currentChance += itemStatBonus.Chance;
                if (currentChance >= rnd)
                    return itemStatBonus;
            }
            return EmptyBonus;
        }

        public void AlignChances()
        {
            if (PosibleBonuses.Count == 0) return;
            var totalChance = PosibleBonuses.Sum(pb => pb.Chance);
            var mult = MaximumChance / totalChance;
            foreach (var itemStatBonuse in PosibleBonuses)
            {
                itemStatBonuse.Chance *= mult;
            }
            totalChance = PosibleBonuses.Sum(pb => pb.Chance);
            if (totalChance != MaximumChance)
            {
                var diff = MaximumChance - totalChance;
                var maxBonus = PosibleBonuses[0];
                foreach (var itemStatBonuse in PosibleBonuses)
                {
                    if (maxBonus.Chance < itemStatBonuse.Chance)
                        maxBonus = itemStatBonuse;
                }
                maxBonus.Chance += diff;
            }
            totalChance = PosibleBonuses.Sum(pb => pb.Chance);
            if (totalChance != MaximumChance)
            {
                LogUtil.ErrorException("FAILED TO ALIGN CHANCES!!!!!!!!!!!!!!!!!!!");
            }
        }
    }

    [DataHolder]
    public partial class Asda2ItemTemplate : IDataHolder, IAsda2MountableItem, IQuestHolderEntry
    {
        #region Standard Fields

        public int Enchant { get; set; }

        public uint Id { get; set; }
        public ItemStatGenerator StatGeneratorCommon = new ItemStatGenerator();
        public ItemStatGenerator StatGeneratorEnchant = new ItemStatGenerator();
        public ItemStatGenerator StatGeneratorCraft = new ItemStatGenerator();
        public ItemStatGenerator StatGeneratorAdvanced = new ItemStatGenerator();


        public Asda2ItemId ItemId;
        public ItemBonusType SowelBonusType;
        public SowelItemType SowelItemType;
        public SowelEquipmentType SowelEquipmentType;
        public int SowelBonusValue;
        public int Unk0;
        public Asda2ItemQuality Quality;
        public int ValueOnUse;
        public uint BuyPrice;

        public int Weight;

        public uint SellPrice;
        public short UsingType;
        public Asda2EquipmentSlots EquipmentSlot;

        public uint RequiredLevel;

        public Asda2Profession RequiredProfession;
        public int BoosterId;

        public uint RequiredPvPRank;

        public int AttackTime;

        public Asda2WeaponType WeaponType;

        public short AtackRange;

        public ItemBondType BondType;

        /// <summary>
        /// The Id of the Quest that will be started when this Item is used
        /// </summary>
        public uint QuestId;

        public uint BlockValue;

        public ItemSetId SetId;

        public byte MaxDurability;

        public Asda2ItemCategory Category;

        public int Duration;

        #endregion

        /// <summary>
        /// Amount of Items to be sold in one stack
        /// </summary>
        public int BuyStackSize;

        public bool CanBuyInRegularShop;
        //[NotPersistent]
        /// <summary>
        /// The ItemSet to which this Item belongs (if any)
        /// </summary>
        //public ItemSet Set;

        [NotPersistent]
        public LockEntry Lock;


        [NotPersistent]
        /// <summary>
        /// whether this is ammo
        /// </summary>
        public bool IsAmmo;


        public int PackageId;
        public bool IsStackable;

        [NotPersistent]
        /// <summary>
        /// whether this is a weapon
        /// </summary>
        public bool IsWeapon;

        [NotPersistent]
        /// <summary>
        /// whether this is a ranged weapon
        /// </summary>
        public bool IsRangedWeapon;

        [NotPersistent]
        public bool IsMeleeWeapon;

        [NotPersistent]
        /// <summary>
        /// whether this is a 2h weapon
        /// </summary>
        public bool IsTwoHandWeapon;

        [NotPersistent]
        /// <summary>
        /// The Quests for which this Item needs to be collected
        /// </summary>
        public QuestTemplate[] CollectQuests;

        public bool HasQuestRequirements
        {
            get { return QuestHolderInfo != null || CollectQuests != null; }
        }

        [NotPersistent]
        public Func<Asda2Item> Creator;

        public short DefaultSoul1Id;


        #region Init
        /// <summary>
        /// Set custom fields etc
        /// </summary>
        public void FinalizeDataHolder()
        {
            if (Id == uint.MaxValue)
                return;
            CheckId();
            Template.IsWeapon = Category == Asda2ItemCategory.OneHandedSword || Category == Asda2ItemCategory.TwoHandedSword ||
                                Category == Asda2ItemCategory.Spear || Category == Asda2ItemCategory.Crossbow ||
                                Category == Asda2ItemCategory.Bow || Category == Asda2ItemCategory.Ballista ||
                                Category == Asda2ItemCategory.Staff || Category == Asda2ItemCategory.Showel;
            Template.IsArmor = Category == Asda2ItemCategory.Helmet || Category == Asda2ItemCategory.Shield ||
                               Category == Asda2ItemCategory.Shirt || Category == Asda2ItemCategory.Pants ||
                               Category == Asda2ItemCategory.Gloves || Category == Asda2ItemCategory.Boots;
            Template.IsAvatar = Category == Asda2ItemCategory.AvatarCloses || Category == Asda2ItemCategory.AvatarAccesory ||
                                Category == Asda2ItemCategory.AvatarCape || Category == Asda2ItemCategory.AvatarGloves ||
                               Category == Asda2ItemCategory.AvatarHemlet || Category == Asda2ItemCategory.AvatarPants ||
                               Category == Asda2ItemCategory.AvatarShirt || Category == Asda2ItemCategory.AvatarShoes ||
                                Category == Asda2ItemCategory.AvatarWings || EquipmentSlot == Asda2EquipmentSlots.AvatarHead ||
                                EquipmentSlot == Asda2EquipmentSlots.AvatarBoots || EquipmentSlot == Asda2EquipmentSlots.AvatarGloves ||
                                EquipmentSlot == Asda2EquipmentSlots.AvatarPans || EquipmentSlot == Asda2EquipmentSlots.AvatarShirt;
            Template.IsEquipment = Template.IsWeapon || Template.IsArmor;
            IsRangedWeapon = Category == Asda2ItemCategory.Crossbow ||
                             Category == Asda2ItemCategory.Bow || Category == Asda2ItemCategory.Ballista;
            IsMeleeWeapon = Category == Asda2ItemCategory.OneHandedSword || Category == Asda2ItemCategory.TwoHandedSword ||
                                Category == Asda2ItemCategory.Spear || Category == Asda2ItemCategory.Staff;
            IsTwoHandWeapon = Category == Asda2ItemCategory.TwoHandedSword ||
                                Category == Asda2ItemCategory.Spear || Category == Asda2ItemCategory.Staff || Category == Asda2ItemCategory.Crossbow ||
                             Category == Asda2ItemCategory.Bow || Category == Asda2ItemCategory.Ballista;
            IsAmmo = Category == Asda2ItemCategory.BowAmmo || Category == Asda2ItemCategory.Crossbow;
            IsRod = Category == Asda2ItemCategory.PremiumFishRod || Category == Asda2ItemCategory.RodFishingSkill ||
                    Category == Asda2ItemCategory.RodGauge || Category == Asda2ItemCategory.RodFishingSkillAndGauge;
            IsBait = Category == Asda2ItemCategory.BaitAnchous || Category == Asda2ItemCategory.BaitElite ||
                    Category == Asda2ItemCategory.BaitPorridge || Category == Asda2ItemCategory.BaitWorm;
            IsPotion = Category == Asda2ItemCategory.HealthPotion || Category == Asda2ItemCategory.ManaPotion ||
                      Category == Asda2ItemCategory.ManaElixir || Category == Asda2ItemCategory.HealthElixir;

            IsAccessories = EquipmentSlot == Asda2EquipmentSlots.Accessory || EquipmentSlot == Asda2EquipmentSlots.RightRing ||
                            EquipmentSlot == Asda2EquipmentSlots.LeftRing;
            if (IsWeapon && RequiredLevel >= 10)
            {
                switch (Category)
                {
                    case Asda2ItemCategory.Staff:
                        RequiredProfession = Asda2Profession.Mage;
                        break;
                    case Asda2ItemCategory.OneHandedSword:
                        RequiredProfession = Asda2Profession.Warrior;
                        break;
                    case Asda2ItemCategory.TwoHandedSword:
                        RequiredProfession = Asda2Profession.Warrior;
                        break;
                    case Asda2ItemCategory.Spear:
                        RequiredProfession = Asda2Profession.Warrior;
                        break;
                    case Asda2ItemCategory.Bow:
                        RequiredProfession = Asda2Profession.Archer;
                        break;
                    case Asda2ItemCategory.Crossbow:
                        RequiredProfession = Asda2Profession.Archer;
                        break;
                    case Asda2ItemCategory.Ballista:
                        RequiredProfession = Asda2Profession.Archer;
                        break;
                }
            }
            if (Asda2ItemMgr.AvalibleRegularShopItems.ContainsKey((int)Id))
                CanBuyInRegularShop = true;

            if (RequiredLevel < 38)
                SowelSocketsCount = 2;
            else
                SowelSocketsCount = 3;
            if (!IsWeapon && !IsArmor && !IsAvatar)
                SowelSocketsCount = 0;
            AuctionLevelCriterion alc;
            AuctionCategory = CalcAuctionCategory(out alc);
            AuctionLevelCriterion = alc;
            AtackRange += 2;
            InitializeStatBonuses();
            ArrayUtil.Set(ref Asda2ItemMgr.Templates, Id, this);
        }

        private void InitializeStatBonuses()
        {
            {
                if (!Asda2ItemMgr.ItemStatsInfos.ContainsKey(RequiredProfession))
                    return;
                if (Asda2ItemMgr.ItemStatsInfos[RequiredProfession].ContainsKey(ItemStatsSlots.Common) && Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Common].ContainsKey(EquipmentSlot))
                    if (Asda2ItemMgr.ItemStatsInfos[RequiredProfession].ContainsKey(ItemStatsSlots.Common) && Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Common].ContainsKey(EquipmentSlot))
                        SetupStatGerenrator(Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Common][EquipmentSlot], StatGeneratorCommon);
                if (Asda2ItemMgr.ItemStatsInfos[RequiredProfession].ContainsKey(ItemStatsSlots.Advanced) && Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Advanced].ContainsKey(EquipmentSlot))
                    SetupStatGerenrator(Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Advanced][EquipmentSlot], StatGeneratorAdvanced);
                if (Asda2ItemMgr.ItemStatsInfos[RequiredProfession].ContainsKey(ItemStatsSlots.Craft) && Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Craft].ContainsKey(EquipmentSlot))
                    SetupStatGerenrator(Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Craft][EquipmentSlot], StatGeneratorCraft);
                if (Asda2ItemMgr.ItemStatsInfos[RequiredProfession].ContainsKey(ItemStatsSlots.Enchant) && Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Enchant].ContainsKey(EquipmentSlot))
                    SetupStatGerenrator(Asda2ItemMgr.ItemStatsInfos[RequiredProfession][ItemStatsSlots.Enchant][EquipmentSlot], StatGeneratorEnchant);
            }
        }

        private void SetupStatGerenrator(IEnumerable<ItemStatsInfo> templs, ItemStatGenerator statGen)
        {
            foreach (var itemStatsInfo in templs)
            {
                if (Template.Quality < itemStatsInfo.ReqiredQuality)
                    continue;
                var val = (int)((itemStatsInfo.BaseValue + RequiredLevel * itemStatsInfo.PerLevelInc) * QualityBonus);
                var spreadingVal = (float)itemStatsInfo.SpreadingPrc / 100;
                if (spreadingVal < 0 || spreadingVal > 0.7f)
                    spreadingVal = 0.5f;
                statGen.PosibleBonuses.Add(new ItemStatBonus
                {
                    Chance = itemStatsInfo.Chance,
                    MaxValue = (short)(val * (1 + spreadingVal)),
                    MinValue = (short)(val),
                    Type = itemStatsInfo.StatType
                });
            }
            statGen.AlignChances();
        }

        public bool IsEquipment { get; set; }

        public AuctionLevelCriterion AuctionLevelCriterion { get; set; }
        public Asda2ItemAuctionCategory AuctionCategory { get; set; }
        private Asda2ItemAuctionCategory CalcAuctionCategory(out AuctionLevelCriterion alc)
        {
            if (RequiredLevel < 11)
                alc = AuctionLevelCriterion.One;
            else if (RequiredLevel < 21)
                alc = AuctionLevelCriterion.Two;
            else if (RequiredLevel < 31)
                alc = AuctionLevelCriterion.Three;
            else if (RequiredLevel < 41)
                alc = AuctionLevelCriterion.Four;
            else if (RequiredLevel < 51)
                alc = AuctionLevelCriterion.Five;
            else if (RequiredLevel < 61)
                alc = AuctionLevelCriterion.Six;
            else if (RequiredLevel < 71)
                alc = AuctionLevelCriterion.Seven;
            else if (RequiredLevel < 81)
                alc = AuctionLevelCriterion.Eight;
            else if (RequiredLevel < 91)
                alc = AuctionLevelCriterion.Nine;
            else
                alc = AuctionLevelCriterion.Ten;
            switch (Category)
            {
                case Asda2ItemCategory.RingMaxMAtack:
                    return Asda2ItemAuctionCategory.Ring;
                case Asda2ItemCategory.RingMDef:
                    return Asda2ItemAuctionCategory.Ring;
                case Asda2ItemCategory.RingMaxDef:
                    return Asda2ItemAuctionCategory.Ring;
                case Asda2ItemCategory.RingMaxAtack:
                    return Asda2ItemAuctionCategory.Ring;
                case Asda2ItemCategory.NacklessHealth:
                    return Asda2ItemAuctionCategory.Nackless;
                case Asda2ItemCategory.NacklessMDef:
                    return Asda2ItemAuctionCategory.Nackless;
                case Asda2ItemCategory.NacklessMana:
                    return Asda2ItemAuctionCategory.Nackless;
                case Asda2ItemCategory.NacklessCriticalChance:
                    return Asda2ItemAuctionCategory.Nackless;
                case Asda2ItemCategory.EnchantWeaponStoneD:
                    alc = AuctionLevelCriterion.Zero;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;
                case Asda2ItemCategory.EnchantWeaponStoneC:
                    alc = AuctionLevelCriterion.One;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;
                case Asda2ItemCategory.EnchantWeaponStoneB:
                    alc = AuctionLevelCriterion.Two;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;
                case Asda2ItemCategory.EnchantWeaponStoneA:
                    alc = AuctionLevelCriterion.Three;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;
                case Asda2ItemCategory.EnchantWeaponStoneS:
                    alc = AuctionLevelCriterion.Four;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;
                case Asda2ItemCategory.EnchantArmorStoneD:
                    alc = AuctionLevelCriterion.Zero;
                    return Asda2ItemAuctionCategory.UpgradeArmor;
                case Asda2ItemCategory.EnchantArmorStoneC:
                    alc = AuctionLevelCriterion.One;
                    return Asda2ItemAuctionCategory.UpgradeArmor;
                case Asda2ItemCategory.EnchantArmorStoneB:
                    alc = AuctionLevelCriterion.Two;
                    return Asda2ItemAuctionCategory.UpgradeArmor;
                case Asda2ItemCategory.EnchantArmorStoneA:
                    alc = AuctionLevelCriterion.Three;
                    return Asda2ItemAuctionCategory.UpgradeArmor;
                case Asda2ItemCategory.EnchantArmorStoneS:
                    alc = AuctionLevelCriterion.Four;
                    return Asda2ItemAuctionCategory.UpgradeArmor;
                case Asda2ItemCategory.Bow:
                    CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponBow;
                case Asda2ItemCategory.OneHandedSword:
                    CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponOhs;
                case Asda2ItemCategory.TwoHandedSword:
                    CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponThs;
                case Asda2ItemCategory.Spear:
                    CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponSpear;
                case Asda2ItemCategory.Crossbow:
                    CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponCrossbow;
                case Asda2ItemCategory.Staff:
                    CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponStaff;
                case Asda2ItemCategory.Helmet:
                    switch (RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorHelm;
                        case Asda2Profession.Mage:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageHelm;
                        case Asda2Profession.Archer:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherHelm;
                    }
                    break;
                case Asda2ItemCategory.Gloves:
                    switch (RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorGloves;
                        case Asda2Profession.Mage:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageGloves;
                        case Asda2Profession.Archer:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherHelm;
                    }
                    break;
                case Asda2ItemCategory.Boots:
                    switch (RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorBoots;
                        case Asda2Profession.Mage:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageBoots;
                        case Asda2Profession.Archer:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherBoots;
                    }
                    break;
                case Asda2ItemCategory.Shirt:
                    switch (RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorArmor;
                        case Asda2Profession.Mage:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageArmor;
                        case Asda2Profession.Archer:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherArmor;
                    }
                    break;
                case Asda2ItemCategory.Pants:
                    switch (RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorPants;
                        case Asda2Profession.Mage:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MagePants;
                        case Asda2Profession.Archer:
                            CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherPants;
                    }
                    break;
                case Asda2ItemCategory.HealthElixir:
                    return Asda2ItemAuctionCategory.PotionHp;
                case Asda2ItemCategory.HealthPotion:
                    return Asda2ItemAuctionCategory.PotionHp;
                case Asda2ItemCategory.Fish:
                    return Asda2ItemAuctionCategory.PotionFish;
                case Asda2ItemCategory.ManaElixir:
                    return Asda2ItemAuctionCategory.PotionMp;
                case Asda2ItemCategory.ManaPotion:
                    return Asda2ItemAuctionCategory.PotionMp;
                case Asda2ItemCategory.Recipe:
                    return Asda2ItemAuctionCategory.Recipe;
                case Asda2ItemCategory.CraftMaterial:
                    return Asda2ItemAuctionCategory.Materials;
                case Asda2ItemCategory.Booster:
                    return Asda2ItemAuctionCategory.Boosters;
                case Asda2ItemCategory.Shield:
                    CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.Shield;
                case Asda2ItemCategory.Premium:
                    return Asda2ItemAuctionCategory.Premium;
                case Asda2ItemCategory.PremiumFishRod:
                    return Asda2ItemAuctionCategory.Premium;
                case Asda2ItemCategory.PremiumPetEgg:
                    return Asda2ItemAuctionCategory.Premium;
                case Asda2ItemCategory.PremiumPotions:
                    return Asda2ItemAuctionCategory.Premium;
                case Asda2ItemCategory.Sowel:
                    switch (SowelItemType)
                    {

                        case SowelItemType.Ohs:
                            return Asda2ItemAuctionCategory.SowelOHS;
                        case SowelItemType.Ths:
                            return Asda2ItemAuctionCategory.SowelThs;
                        case SowelItemType.Spear:
                            return Asda2ItemAuctionCategory.SowelSpear;
                        case SowelItemType.Staff:
                            return Asda2ItemAuctionCategory.SowelStaff;
                        case SowelItemType.Bow:
                            return Asda2ItemAuctionCategory.SowelBow;
                        case SowelItemType.Crossbow:
                            return Asda2ItemAuctionCategory.SowelCrossBow;
                        case SowelItemType.Balista:
                            return Asda2ItemAuctionCategory.SowelCrossBow;
                        case SowelItemType.Other:
                            if (IsAvatarSowel)
                            {
                                switch (SowelBonusType)
                                {
                                    case ItemBonusType.Intelect:
                                        return Asda2ItemAuctionCategory.RuneIntellect;
                                    case ItemBonusType.Energy:
                                        return Asda2ItemAuctionCategory.RuneSpirit;
                                    case ItemBonusType.Agility:
                                        return Asda2ItemAuctionCategory.RuneDexterity;
                                    case ItemBonusType.Defence:
                                        return Asda2ItemAuctionCategory.RuneMisc;
                                    case ItemBonusType.ErengyByPrc:
                                        return Asda2ItemAuctionCategory.RuneSpirit;
                                    case ItemBonusType.IntelegenceByPrc:
                                        return Asda2ItemAuctionCategory.RuneIntellect;
                                    case ItemBonusType.Luck:
                                        return Asda2ItemAuctionCategory.RuneLuck;
                                    case ItemBonusType.LuckByPrc:
                                        return Asda2ItemAuctionCategory.RuneLuck;
                                    case ItemBonusType.Stamina:
                                        return Asda2ItemAuctionCategory.RuneStamina;
                                    case ItemBonusType.StaminaByPrc:
                                        return Asda2ItemAuctionCategory.RuneStamina;
                                    case ItemBonusType.Strength:
                                        return Asda2ItemAuctionCategory.RuneStrength;
                                    case ItemBonusType.StrengthByPrc:
                                        return Asda2ItemAuctionCategory.RuneStrength;
                                }
                                return Asda2ItemAuctionCategory.RuneMisc;
                            }
                            else
                            {
                                switch (SowelBonusType)
                                {
                                    case ItemBonusType.Intelect:
                                        return Asda2ItemAuctionCategory.SowelIntellect;
                                    case ItemBonusType.Energy:
                                        return Asda2ItemAuctionCategory.SowelSpirit;
                                    case ItemBonusType.Agility:
                                        return Asda2ItemAuctionCategory.SowelDexterity;
                                    case ItemBonusType.Defence:
                                        return Asda2ItemAuctionCategory.SowelArmor;
                                    case ItemBonusType.ErengyByPrc:
                                        return Asda2ItemAuctionCategory.SowelSpirit;
                                    case ItemBonusType.IntelegenceByPrc:
                                        return Asda2ItemAuctionCategory.SowelIntellect;
                                    case ItemBonusType.Luck:
                                        return Asda2ItemAuctionCategory.SowelLuck;
                                    case ItemBonusType.LuckByPrc:
                                        return Asda2ItemAuctionCategory.SowelLuck;
                                    case ItemBonusType.Stamina:
                                        return Asda2ItemAuctionCategory.SowelStamina;
                                    case ItemBonusType.StaminaByPrc:
                                        return Asda2ItemAuctionCategory.SowelStamina;
                                    case ItemBonusType.Strength:
                                        return Asda2ItemAuctionCategory.SowelStrengs;
                                    case ItemBonusType.StrengthByPrc:
                                        return Asda2ItemAuctionCategory.SowelStrengs;
                                }
                            }
                            return Asda2ItemAuctionCategory.SowelMisc;
                    }
                    return Asda2ItemAuctionCategory.PotionMp;
            }
            return Asda2ItemAuctionCategory.Misc;
        }

        private void CalcAlcGrade(out AuctionLevelCriterion alc)
        {
            if (RequiredLevel < 20)
                alc = AuctionLevelCriterion.Zero;
            else if (RequiredLevel < 40)
                alc = AuctionLevelCriterion.One;
            else if (RequiredLevel < 60)
                alc = AuctionLevelCriterion.Two;
            else
                alc = RequiredLevel < 80 ? AuctionLevelCriterion.Three : AuctionLevelCriterion.Four;
        }

        protected bool IsAvatarSowel
        {
            get
            {
                return SowelEquipmentType == SowelEquipmentType.AvatarShirt ||
                       SowelEquipmentType == SowelEquipmentType.AvatarPans ||
                       SowelEquipmentType == SowelEquipmentType.AvatarHead ||
                       SowelEquipmentType == SowelEquipmentType.AvatarGloves ||
                       SowelEquipmentType == SowelEquipmentType.AvatarBoots ||
                       SowelEquipmentType == SowelEquipmentType.AvaratRightHead ||
                       SowelEquipmentType == SowelEquipmentType.Wings;
            }
        }

        internal void InitializeTemplate()
        {
            ItemId = (Asda2ItemId)Id;
            Creator = () => new Asda2Item();
        }
        #endregion

        #region Checks
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
            if (!HasQuestRequirements)			// no quest requirements
                return true;

            if (looter == null)
            {
                // cannot determine quest constraints if looter is offline
                return false;
            }

            if (QuestHolderInfo != null)
            {
                // starts a quest
                if (QuestHolderInfo.QuestStarts.Any(quest => looter.QuestLog.HasActiveQuest(quest)))
                {
                    return false;
                }
            }

            if (CollectQuests != null)
            {
                // is collectable for one or more quests
                // check whether the looter has any of the required quests
                for (var i = 0; i < CollectQuests.Length; i++)
                {
                    var q = CollectQuests[i];
                    if (q != null)
                    {
                        if (looter.QuestLog.HasActiveQuest(q.Id))
                        {
                            for (int it = 0; it < q.CollectableItems.Length; it++)
                            {
                                if (q.CollectableItems[it].ItemId == ItemId)
                                {
                                    if (q.CollectableItems[it].Amount > looter.QuestLog.GetActiveQuest(q.Id).CollectedItems[it])
                                    {
                                        return true;
                                    }
                                }
                            }
                            for (int it = 0; it < q.CollectableSourceItems.Length; it++)
                            {
                                if (q.CollectableSourceItems[it].ItemId == ItemId)
                                {
                                    if (q.CollectableSourceItems[it].Amount > looter.QuestLog.GetActiveQuest(q.Id).CollectedSourceItems[it])
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns what went wrong (if anything) when the given unit tries to equip or use Items of this Template.
        /// </summary>
        public Asda2InventoryError CheckEquip(Character chr)
        {
            if (chr.GodMode)
            {
                return Asda2InventoryError.Ok;
            }

            // level
            if (chr.Level < RequiredLevel)
            {
                chr.SendNotifyMsg(string.Format("You must reach level {0} to equip this item.", RequiredLevel));
                return Asda2InventoryError.Fail;
            }

            // class
            if (RequiredProfession != chr.Profession)
            {
                chr.SendNotifyMsg(string.Format("This item can equiped only by {0} profesion.", RequiredProfession));
                return Asda2InventoryError.Fail;
            }

            // TODO: Add missing restrictions

            return Asda2InventoryError.Ok;
        }

        private void CheckId()
        {
            // sanity check
            if (Id > ItemMgr.MaxId)
            {
                throw new Exception("Found item-template (" + Id + ") with Id > " + ItemMgr.MaxId + ". Items with such a high ID would blow the item storage array.");
            }
        }
        #endregion

        #region Interface implementations
        public Asda2ItemTemplate Template
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

        public static IEnumerable<Asda2ItemTemplate> GetAllDataHolders()
        {
            return Asda2ItemMgr.Templates;
        }

        /// <summary>
        /// Contains the quests that this item can start (items usually can only start one)
        /// </summary>
        public QuestHolderInfo QuestHolderInfo
        {
            get;
            internal set;
        }

        public string Name { get; set; }

        public byte InventoryType;
        public bool IsArmor;
        public bool IsAvatar;
        public byte SowelSocketsCount = 3;

        public bool IsShopInventoryItem
        {
            get { return InventoryType == 1; }
            set { InventoryType = (byte)(value ? 1 : 0); }
        }

        public int UniqueCount { get; set; }

        public int MaxAmount { get; set; }
        //[NotPersistent]
        //public DamageInfo[] Damages { get; set; }
        [NotPersistent]
        public ItemSet Set { get; set; }

        public bool IsMultiloot { get; set; }

        public float QualityBonus
        {
            get
            {
                switch (Quality)
                {
                    case Asda2ItemQuality.White:
                        return 1;
                    case Asda2ItemQuality.Yello:
                        return 1.1f;
                    case Asda2ItemQuality.Purple:
                        return 1.2f;
                    case Asda2ItemQuality.Green:
                        return 1.3f;
                    case Asda2ItemQuality.Orange:
                        return 1.4f;
                }
                return 1;
            }
        }

        public bool IsRod { get; set; }

        public bool IsBait { get; set; }
        public bool IsPotion { get; set; }
        public bool IsAccessories { get; set; }

        public IWorldLocation[] GetInWorldTemplates()
        {
            return null;
        }

        public Asda2Item Create()
        {
            return Creator();
        }
        #endregion

        private void OnRecordCreated(Asda2ItemRecord record)
        {
            /*if (IsCharter)
            {
                if (!record.IsNew)
                {
                    // this is executed in the IO-context
                    PetitionRecord.LoadRecord(record.OwnerId);
                }
            }*/
        }


        #region events

        /// <summary>
        /// Called when an ItemRecord of this ItemTemplate has been created (if newly created or loaded from DB).
        /// That is before the actual Item object has been created.
        /// Called from the IO context if loaded from DB.
        /// </summary>
        public event Action<Asda2ItemRecord> Created;

        /// <summary>
        /// Called whenever an Item of this ItemTemplate is equipped
        /// </summary>
        public event Action<Asda2Item> Equipped;

        /// <summary>
        /// Called whenever an Item of this ItemTemplate is unequipped
        /// </summary>
        public event Action<Asda2Item> Unequipped;

        /// <summary>
        /// Called whenever an item of this ItemTemplate has been used
        /// </summary>
        public event Action<Asda2Item> Used;

        internal void NotifyCreated(Asda2ItemRecord record)
        {
            OnRecordCreated(record);
            var evt = Created;
            if (evt != null)
            {
                evt(record);
            }
        }

        internal void NotifyEquip(Asda2Item item)
        {
            var evt = Equipped;
            if (evt != null)
            {
                evt(item);
            }
        }

        internal void NotifyUnequip(Asda2Item item)
        {
            var evt = Unequipped;
            if (evt != null)
            {
                evt(item);
            }
        }

        internal void NotifyUsed(Asda2Item item)
        {
            var evt = Used;
            if (evt != null)
            {
                evt(item);
            }
        }
        #endregion
        public override string ToString()
        {
            return string.Format("{0} (Id: {1})", Name, Id);
        }

    }

    public enum SowelItemType
    {
        Other = 0,
        Ohs = 2,
        Spear = 4,
        Ths = 8,
        Staff = 32,
        Crossbow = 512,
        Bow = 1024,
        Balista = 2048
    }
    public enum AuctionLevelCriterion
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        All = 100
    }
    public enum Asda2ItemAuctionCategory
    {
        Ring,
        Nackless,
        SowelOHS,
        SowelSpear,
        SowelThs,
        SowelBow,
        SowelCrossBow,
        SowelStaff,
        SowelStrengs,
        SowelDexterity,
        SowelStamina,
        SowelSpirit,
        SowelIntellect,
        SowelLuck,
        SowelArmor,
        SowelMisc,
        RuneStrength,
        RuneDexterity,
        RuneStamina,
        RuneSpirit,
        RuneIntellect,
        RuneLuck,
        RuneMisc,
        UpgradeWeapon,
        UpgradeArmor,
        PotionHp,
        PotionMp,
        PotionFish,
        Recipe,
        Materials,
        Boosters,
        Misc,
        Shield,
        WeaponOhs,
        WeaponSpear,
        WeaponThs,
        WeaponStaff,
        WeaponCrossbow,
        WeaponBow,
        WarriorHelm,
        WarriorArmor,
        WarriorPants,
        WarriorBoots,
        WarriorGloves,
        MageHelm,
        MageArmor,
        MagePants,
        MageBoots,
        MageGloves,
        ArcherHelm,
        ArcherArmor,
        ArcherPants,
        ArcherBoots,
        ArcherGloves,
        Premium,
    }
}