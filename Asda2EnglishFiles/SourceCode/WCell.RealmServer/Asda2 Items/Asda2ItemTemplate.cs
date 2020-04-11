using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [DataHolder]
    public class Asda2ItemTemplate : IDataHolder, IAsda2MountableItem, IQuestHolderEntry
    {
        public ItemStatGenerator StatGeneratorCommon = new ItemStatGenerator();
        public ItemStatGenerator StatGeneratorEnchant = new ItemStatGenerator();
        public ItemStatGenerator StatGeneratorCraft = new ItemStatGenerator();
        public ItemStatGenerator StatGeneratorAdvanced = new ItemStatGenerator();
        public byte SowelSocketsCount = 3;
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

        /// <summary>Amount of Items to be sold in one stack</summary>
        public int BuyStackSize;

        public bool CanBuyInRegularShop;

        /// <summary>The ItemSet to which this Item belongs (if any)</summary>
        [NotPersistent] public LockEntry Lock;

        [NotPersistent] public bool IsAmmo;
        public int PackageId;
        public bool IsStackable;
        [NotPersistent] public bool IsWeapon;
        [NotPersistent] public bool IsRangedWeapon;
        [NotPersistent] public bool IsMeleeWeapon;
        [NotPersistent] public bool IsTwoHandWeapon;
        [NotPersistent] public QuestTemplate[] CollectQuests;
        [NotPersistent] public Func<Asda2Item> Creator;
        public short DefaultSoul1Id;
        public byte InventoryType;
        public bool IsArmor;
        public bool IsAvatar;
        public bool IsAccessory;

        public uint Id { get; set; }

        public bool HasQuestRequirements
        {
            get
            {
                if (this.QuestHolderInfo == null)
                    return this.CollectQuests != null;
                return true;
            }
        }

        /// <summary>Set custom fields etc</summary>
        public void FinalizeDataHolder()
        {
            if (this.Id == uint.MaxValue)
                return;
            this.CheckId();
            this.Template.IsAccessory = this.Category == Asda2ItemCategory.RingMaxAtack ||
                                        this.Category == Asda2ItemCategory.RingMaxMAtack ||
                                        (this.Category == Asda2ItemCategory.RingMaxDef ||
                                         this.Category == Asda2ItemCategory.NacklessCriticalChance) ||
                                        (this.Category == Asda2ItemCategory.NacklessHealth ||
                                         this.Category == Asda2ItemCategory.NacklessMana ||
                                         this.Category == Asda2ItemCategory.RingMDef) ||
                                        this.Category == Asda2ItemCategory.NacklessMDef;
            this.Template.IsWeapon = this.Category == Asda2ItemCategory.OneHandedSword ||
                                     this.Category == Asda2ItemCategory.TwoHandedSword ||
                                     (this.Category == Asda2ItemCategory.Spear ||
                                      this.Category == Asda2ItemCategory.Crossbow) ||
                                     (this.Category == Asda2ItemCategory.Bow ||
                                      this.Category == Asda2ItemCategory.Ballista ||
                                      this.Category == Asda2ItemCategory.Staff) ||
                                     this.Category == Asda2ItemCategory.Showel;
            this.Template.IsArmor = this.Category == Asda2ItemCategory.Helmet ||
                                    this.Category == Asda2ItemCategory.Shield ||
                                    (this.Category == Asda2ItemCategory.Shirt ||
                                     this.Category == Asda2ItemCategory.Pants) ||
                                    this.Category == Asda2ItemCategory.Gloves ||
                                    this.Category == Asda2ItemCategory.Boots;
            this.Template.IsAvatar = this.Category == Asda2ItemCategory.AvatarCloses ||
                                     this.Category == Asda2ItemCategory.AvatarAccesory ||
                                     (this.Category == Asda2ItemCategory.AvatarCape ||
                                      this.Category == Asda2ItemCategory.AvatarGloves) ||
                                     (this.Category == Asda2ItemCategory.AvatarHemlet ||
                                      this.Category == Asda2ItemCategory.AvatarPants ||
                                      (this.Category == Asda2ItemCategory.AvatarShirt ||
                                       this.Category == Asda2ItemCategory.AvatarShoes)) ||
                                     (this.Category == Asda2ItemCategory.AvatarWings ||
                                      this.EquipmentSlot == Asda2EquipmentSlots.AvatarHead ||
                                      (this.EquipmentSlot == Asda2EquipmentSlots.AvatarBoots ||
                                       this.EquipmentSlot == Asda2EquipmentSlots.AvatarGloves) ||
                                      this.EquipmentSlot == Asda2EquipmentSlots.AvatarPans) ||
                                     this.EquipmentSlot == Asda2EquipmentSlots.AvatarShirt;
            this.Template.IsEquipment = this.Template.IsWeapon || this.Template.IsArmor;
            this.IsRangedWeapon = this.Category == Asda2ItemCategory.Crossbow ||
                                  this.Category == Asda2ItemCategory.Bow || this.Category == Asda2ItemCategory.Ballista;
            this.IsMeleeWeapon = this.Category == Asda2ItemCategory.OneHandedSword ||
                                 this.Category == Asda2ItemCategory.TwoHandedSword ||
                                 this.Category == Asda2ItemCategory.Spear || this.Category == Asda2ItemCategory.Staff;
            this.IsTwoHandWeapon = this.Category == Asda2ItemCategory.TwoHandedSword ||
                                   this.Category == Asda2ItemCategory.Spear ||
                                   (this.Category == Asda2ItemCategory.Staff ||
                                    this.Category == Asda2ItemCategory.Crossbow) ||
                                   this.Category == Asda2ItemCategory.Bow ||
                                   this.Category == Asda2ItemCategory.Ballista;
            this.IsAmmo = this.Category == Asda2ItemCategory.BowAmmo || this.Category == Asda2ItemCategory.Crossbow;
            this.IsRod = this.Category == Asda2ItemCategory.PremiumFishRod ||
                         this.Category == Asda2ItemCategory.RodFishingSkill ||
                         this.Category == Asda2ItemCategory.RodGauge ||
                         this.Category == Asda2ItemCategory.RodFishingSkillAndGauge;
            this.IsBait = this.Category == Asda2ItemCategory.BaitAnchous ||
                          this.Category == Asda2ItemCategory.BaitElite ||
                          this.Category == Asda2ItemCategory.BaitPorridge ||
                          this.Category == Asda2ItemCategory.BaitWorm;
            if (this.IsWeapon && this.RequiredLevel >= 10U)
            {
                switch (this.Category)
                {
                    case Asda2ItemCategory.OneHandedSword:
                        this.RequiredProfession = Asda2Profession.Warrior;
                        break;
                    case Asda2ItemCategory.TwoHandedSword:
                        this.RequiredProfession = Asda2Profession.Warrior;
                        break;
                    case Asda2ItemCategory.Staff:
                        this.RequiredProfession = Asda2Profession.Mage;
                        break;
                    case Asda2ItemCategory.Crossbow:
                        this.RequiredProfession = Asda2Profession.Archer;
                        break;
                    case Asda2ItemCategory.Bow:
                        this.RequiredProfession = Asda2Profession.Archer;
                        break;
                    case Asda2ItemCategory.Ballista:
                        this.RequiredProfession = Asda2Profession.Archer;
                        break;
                    case Asda2ItemCategory.Spear:
                        this.RequiredProfession = Asda2Profession.Warrior;
                        break;
                }
            }

            if (Asda2ItemMgr.AvalibleRegularShopItems.ContainsKey((int) this.Id))
                this.CanBuyInRegularShop = true;
            this.SowelSocketsCount = this.RequiredLevel >= 38U ? (byte) 3 : (byte) 2;
            if (!this.IsWeapon && !this.IsArmor && !this.IsAvatar)
                this.SowelSocketsCount = (byte) 0;
            AuctionLevelCriterion alc;
            this.AuctionCategory = this.CalcAuctionCategory(out alc);
            this.AuctionLevelCriterion = alc;
            this.AtackRange += (short) 2;
            this.InitializeStatBonuses();
            ArrayUtil.Set<Asda2ItemTemplate>(ref Asda2ItemMgr.Templates, this.Id, this);
        }

        private void InitializeStatBonuses()
        {
            if (!Asda2ItemMgr.ItemStatsInfos.ContainsKey(this.RequiredProfession))
                return;
            if (Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession].ContainsKey(ItemStatsSlots.Common) &&
                Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][ItemStatsSlots.Common]
                    .ContainsKey(this.EquipmentSlot) &&
                (Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession].ContainsKey(ItemStatsSlots.Common) &&
                 Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][ItemStatsSlots.Common]
                     .ContainsKey(this.EquipmentSlot)))
                this.SetupStatGerenrator(
                    (IEnumerable<ItemStatsInfo>) Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][
                        ItemStatsSlots.Common][this.EquipmentSlot], this.StatGeneratorCommon);
            if (Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession].ContainsKey(ItemStatsSlots.Advanced) &&
                Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][ItemStatsSlots.Advanced]
                    .ContainsKey(this.EquipmentSlot))
                this.SetupStatGerenrator(
                    (IEnumerable<ItemStatsInfo>) Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][
                        ItemStatsSlots.Advanced][this.EquipmentSlot], this.StatGeneratorAdvanced);
            if (Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession].ContainsKey(ItemStatsSlots.Craft) &&
                Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][ItemStatsSlots.Craft]
                    .ContainsKey(this.EquipmentSlot))
                this.SetupStatGerenrator(
                    (IEnumerable<ItemStatsInfo>) Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][
                        ItemStatsSlots.Craft][this.EquipmentSlot], this.StatGeneratorCraft);
            if (!Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession].ContainsKey(ItemStatsSlots.Enchant) ||
                !Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][ItemStatsSlots.Enchant]
                    .ContainsKey(this.EquipmentSlot))
                return;
            this.SetupStatGerenrator(
                (IEnumerable<ItemStatsInfo>) Asda2ItemMgr.ItemStatsInfos[this.RequiredProfession][
                    ItemStatsSlots.Enchant][this.EquipmentSlot], this.StatGeneratorEnchant);
        }

        private void SetupStatGerenrator(IEnumerable<ItemStatsInfo> templs, ItemStatGenerator statGen)
        {
            foreach (ItemStatsInfo templ in templs)
            {
                if (this.Template.Quality >= templ.ReqiredQuality)
                {
                    int num1 = (int) (((double) templ.BaseValue +
                                       (double) this.RequiredLevel * (double) templ.PerLevelInc) *
                                      (double) this.QualityBonus);
                    float num2 = (float) templ.SpreadingPrc / 100f;
                    if ((double) num2 < 0.0 || (double) num2 > 0.699999988079071)
                        num2 = 0.5f;
                    statGen.PosibleBonuses.Add(new ItemStatBonus()
                    {
                        Chance = templ.Chance,
                        MaxValue = (short) ((double) num1 * (1.0 + (double) num2)),
                        MinValue = (short) num1,
                        Type = templ.StatType
                    });
                }
            }

            statGen.AlignChances();
        }

        public bool IsEquipment { get; set; }

        public AuctionLevelCriterion AuctionLevelCriterion { get; set; }

        public Asda2ItemAuctionCategory AuctionCategory { get; set; }

        private Asda2ItemAuctionCategory CalcAuctionCategory(out AuctionLevelCriterion alc)
        {
            if (this.RequiredLevel < 11)
            {
                alc = AuctionLevelCriterion.One;
            }
            else if (this.RequiredLevel < 0x15)
            {
                alc = AuctionLevelCriterion.Two;
            }
            else if (this.RequiredLevel < 0x1f)
            {
                alc = AuctionLevelCriterion.Three;
            }
            else if (this.RequiredLevel < 0x29)
            {
                alc = AuctionLevelCriterion.Four;
            }
            else if (this.RequiredLevel < 0x33)
            {
                alc = AuctionLevelCriterion.Five;
            }
            else if (this.RequiredLevel < 0x3d)
            {
                alc = AuctionLevelCriterion.Six;
            }
            else if (this.RequiredLevel < 0x47)
            {
                alc = AuctionLevelCriterion.Seven;
            }
            else if (this.RequiredLevel < 0x51)
            {
                alc = AuctionLevelCriterion.Eight;
            }
            else if (this.RequiredLevel < 0x5b)
            {
                alc = AuctionLevelCriterion.Nine;
            }
            else
            {
                alc = AuctionLevelCriterion.Ten;
            }

            switch (this.Category)
            {
                case Asda2ItemCategory.Sowel:
                    switch (this.SowelItemType)
                    {
                        case SowelItemType.Other:
                            if (!this.IsAvatarSowel)
                            {
                                switch (this.SowelBonusType)
                                {
                                    case ItemBonusType.Defence:
                                        return Asda2ItemAuctionCategory.SowelArmor;

                                    case ItemBonusType.Strength:
                                        return Asda2ItemAuctionCategory.SowelStrengs;

                                    case ItemBonusType.Agility:
                                        return Asda2ItemAuctionCategory.SowelDexterity;

                                    case ItemBonusType.Stamina:
                                        return Asda2ItemAuctionCategory.SowelStamina;

                                    case ItemBonusType.Energy:
                                        return Asda2ItemAuctionCategory.SowelSpirit;

                                    case ItemBonusType.Intelect:
                                        return Asda2ItemAuctionCategory.SowelIntellect;

                                    case ItemBonusType.Luck:
                                        return Asda2ItemAuctionCategory.SowelLuck;

                                    case ItemBonusType.StrengthByPrc:
                                        return Asda2ItemAuctionCategory.SowelStrengs;

                                    case ItemBonusType.StaminaByPrc:
                                        return Asda2ItemAuctionCategory.SowelStamina;

                                    case ItemBonusType.IntelegenceByPrc:
                                        return Asda2ItemAuctionCategory.SowelIntellect;

                                    case ItemBonusType.ErengyByPrc:
                                        return Asda2ItemAuctionCategory.SowelSpirit;

                                    case ItemBonusType.LuckByPrc:
                                        return Asda2ItemAuctionCategory.SowelLuck;
                                }

                                return Asda2ItemAuctionCategory.SowelMisc;
                            }

                            switch (this.SowelBonusType)
                            {
                                case ItemBonusType.Defence:
                                    return Asda2ItemAuctionCategory.RuneMisc;

                                case ItemBonusType.Strength:
                                    return Asda2ItemAuctionCategory.RuneStrength;

                                case ItemBonusType.Agility:
                                    return Asda2ItemAuctionCategory.RuneDexterity;

                                case ItemBonusType.Stamina:
                                    return Asda2ItemAuctionCategory.RuneStamina;

                                case ItemBonusType.Energy:
                                    return Asda2ItemAuctionCategory.RuneSpirit;

                                case ItemBonusType.Intelect:
                                    return Asda2ItemAuctionCategory.RuneIntellect;

                                case ItemBonusType.Luck:
                                    return Asda2ItemAuctionCategory.RuneLuck;

                                case ItemBonusType.StrengthByPrc:
                                    return Asda2ItemAuctionCategory.RuneStrength;

                                case ItemBonusType.StaminaByPrc:
                                    return Asda2ItemAuctionCategory.RuneStamina;

                                case ItemBonusType.IntelegenceByPrc:
                                    return Asda2ItemAuctionCategory.RuneIntellect;

                                case ItemBonusType.ErengyByPrc:
                                    return Asda2ItemAuctionCategory.RuneSpirit;

                                case ItemBonusType.LuckByPrc:
                                    return Asda2ItemAuctionCategory.RuneLuck;
                            }

                            return Asda2ItemAuctionCategory.RuneMisc;

                        case SowelItemType.Ohs:
                            return Asda2ItemAuctionCategory.SowelOHS;

                        case SowelItemType.Spear:
                            return Asda2ItemAuctionCategory.SowelSpear;

                        case SowelItemType.Ths:
                            return Asda2ItemAuctionCategory.SowelThs;

                        case SowelItemType.Staff:
                            return Asda2ItemAuctionCategory.SowelStaff;

                        case SowelItemType.Crossbow:
                            return Asda2ItemAuctionCategory.SowelCrossBow;

                        case SowelItemType.Bow:
                            return Asda2ItemAuctionCategory.SowelBow;

                        case SowelItemType.Balista:
                            return Asda2ItemAuctionCategory.SowelCrossBow;
                    }

                    break;

                case Asda2ItemCategory.PremiumPetEgg:
                    return Asda2ItemAuctionCategory.Premium;

                case Asda2ItemCategory.PremiumPotions:
                    return Asda2ItemAuctionCategory.Premium;

                case Asda2ItemCategory.OneHandedSword:
                    this.CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponOhs;

                case Asda2ItemCategory.TwoHandedSword:
                    this.CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponThs;

                case Asda2ItemCategory.Staff:
                    this.CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponStaff;

                case Asda2ItemCategory.Crossbow:
                    this.CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponCrossbow;

                case Asda2ItemCategory.Bow:
                    this.CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponBow;

                case Asda2ItemCategory.Spear:
                    this.CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.WeaponSpear;

                case Asda2ItemCategory.Premium:
                    return Asda2ItemAuctionCategory.Premium;

                case Asda2ItemCategory.Fish:
                    return Asda2ItemAuctionCategory.PotionFish;

                case Asda2ItemCategory.HealthPotion:
                    return Asda2ItemAuctionCategory.PotionHp;

                case Asda2ItemCategory.ManaPotion:
                    return Asda2ItemAuctionCategory.PotionMp;

                case Asda2ItemCategory.Recipe:
                    return Asda2ItemAuctionCategory.Recipe;

                case Asda2ItemCategory.Boots:
                    switch (this.RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorBoots;

                        case Asda2Profession.Archer:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherBoots;

                        case Asda2Profession.Mage:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageBoots;
                    }

                    goto Label_053B;

                case Asda2ItemCategory.Pants:
                    switch (this.RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorPants;

                        case Asda2Profession.Archer:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherPants;

                        case Asda2Profession.Mage:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MagePants;
                    }

                    goto Label_053B;

                case Asda2ItemCategory.Gloves:
                    switch (this.RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorGloves;

                        case Asda2Profession.Archer:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherHelm;

                        case Asda2Profession.Mage:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageGloves;
                    }

                    goto Label_053B;

                case Asda2ItemCategory.Shirt:
                    switch (this.RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorArmor;

                        case Asda2Profession.Archer:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherArmor;

                        case Asda2Profession.Mage:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageArmor;
                    }

                    goto Label_053B;

                case Asda2ItemCategory.Shield:
                    this.CalcAlcGrade(out alc);
                    return Asda2ItemAuctionCategory.Shield;

                case Asda2ItemCategory.RingMaxAtack:
                    return Asda2ItemAuctionCategory.Ring;

                case Asda2ItemCategory.RingMaxMAtack:
                    return Asda2ItemAuctionCategory.Ring;

                case Asda2ItemCategory.RingMaxDef:
                    return Asda2ItemAuctionCategory.Ring;

                case Asda2ItemCategory.NacklessCriticalChance:
                    return Asda2ItemAuctionCategory.Nackless;

                case Asda2ItemCategory.NacklessHealth:
                    return Asda2ItemAuctionCategory.Nackless;

                case Asda2ItemCategory.NacklessMana:
                    return Asda2ItemAuctionCategory.Nackless;

                case Asda2ItemCategory.HealthElixir:
                    return Asda2ItemAuctionCategory.PotionHp;

                case Asda2ItemCategory.EnchantWeaponStoneD:
                    alc = AuctionLevelCriterion.Zero;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;

                case Asda2ItemCategory.RingMDef:
                    return Asda2ItemAuctionCategory.Ring;

                case Asda2ItemCategory.NacklessMDef:
                    return Asda2ItemAuctionCategory.Nackless;

                case Asda2ItemCategory.EnchantWeaponStoneC:
                    alc = AuctionLevelCriterion.One;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;

                case Asda2ItemCategory.Helmet:
                    switch (this.RequiredProfession)
                    {
                        case Asda2Profession.Warrior:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.WarriorHelm;

                        case Asda2Profession.Archer:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.ArcherHelm;

                        case Asda2Profession.Mage:
                            this.CalcAlcGrade(out alc);
                            return Asda2ItemAuctionCategory.MageHelm;
                    }

                    goto Label_053B;

                case Asda2ItemCategory.EnchantWeaponStoneB:
                    alc = AuctionLevelCriterion.Two;
                    return Asda2ItemAuctionCategory.UpgradeWeapon;

                case Asda2ItemCategory.ManaElixir:
                    return Asda2ItemAuctionCategory.PotionMp;

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

                case Asda2ItemCategory.PremiumFishRod:
                    return Asda2ItemAuctionCategory.Premium;

                case Asda2ItemCategory.CraftMaterial:
                    return Asda2ItemAuctionCategory.Materials;

                case Asda2ItemCategory.Booster:
                    return Asda2ItemAuctionCategory.Boosters;

                default:
                    goto Label_053B;
            }

            return Asda2ItemAuctionCategory.PotionMp;
            Label_053B:
            return Asda2ItemAuctionCategory.Misc;
        }

        private void CalcAlcGrade(out AuctionLevelCriterion alc)
        {
            if (this.RequiredLevel < 20U)
                alc = AuctionLevelCriterion.Zero;
            else if (this.RequiredLevel < 40U)
                alc = AuctionLevelCriterion.One;
            else if (this.RequiredLevel < 60U)
                alc = AuctionLevelCriterion.Two;
            else
                alc = this.RequiredLevel < 80U ? AuctionLevelCriterion.Three : AuctionLevelCriterion.Four;
        }

        protected bool IsAvatarSowel
        {
            get
            {
                if (this.SowelEquipmentType != SowelEquipmentType.AvatarShirt &&
                    this.SowelEquipmentType != SowelEquipmentType.AvatarPans &&
                    (this.SowelEquipmentType != SowelEquipmentType.AvatarHead &&
                     this.SowelEquipmentType != SowelEquipmentType.AvatarGloves) &&
                    (this.SowelEquipmentType != SowelEquipmentType.AvatarBoots &&
                     this.SowelEquipmentType != SowelEquipmentType.AvaratRightHead))
                    return this.SowelEquipmentType == SowelEquipmentType.Wings;
                return true;
            }
        }

        internal void InitializeTemplate()
        {
            this.ItemId = (Asda2ItemId) this.Id;
            this.Creator = (Func<Asda2Item>) (() => new Asda2Item());
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
        public Asda2InventoryError CheckEquip(Character chr)
        {
            if (chr.GodMode)
                return Asda2InventoryError.Ok;
            if ((long) chr.Level < (long) this.RequiredLevel)
            {
                chr.SendNotifyMsg(string.Format("You must reach level {0} to equip this item.",
                    (object) this.RequiredLevel));
                return Asda2InventoryError.Fail;
            }

            if (this.RequiredProfession == chr.Profession)
                return Asda2InventoryError.Ok;
            chr.SendNotifyMsg(string.Format("This item can equiped only by {0} profesion.",
                (object) this.RequiredProfession));
            return Asda2InventoryError.Fail;
        }

        private void CheckId()
        {
            if (this.Id > 100000U)
                throw new Exception("Found item-template (" + (object) this.Id + ") with Id > " + (object) 100000U +
                                    ". Items with such a high ID would blow the item storage array.");
        }

        public Asda2ItemTemplate Template
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

        public static IEnumerable<Asda2ItemTemplate> GetAllDataHolders()
        {
            return (IEnumerable<Asda2ItemTemplate>) Asda2ItemMgr.Templates;
        }

        /// <summary>
        /// Contains the quests that this item can start (items usually can only start one)
        /// </summary>
        public QuestHolderInfo QuestHolderInfo { get; internal set; }

        public string Name { get; set; }

        public bool IsShopInventoryItem
        {
            get { return this.InventoryType == (byte) 1; }
            set { this.InventoryType = value ? (byte) 1 : (byte) 0; }
        }

        public int UniqueCount { get; set; }

        public int MaxAmount { get; set; }

        [NotPersistent] public ItemSet Set { get; set; }

        public bool IsMultiloot { get; set; }

        public float QualityBonus
        {
            get
            {
                switch (this.Quality)
                {
                    case Asda2ItemQuality.White:
                        return 1f;
                    case Asda2ItemQuality.Yello:
                        return 1.1f;
                    case Asda2ItemQuality.Purple:
                        return 1.2f;
                    case Asda2ItemQuality.Green:
                        return 1.3f;
                    case Asda2ItemQuality.Orange:
                        return 1.4f;
                    default:
                        return 1f;
                }
            }
        }

        public bool IsRod { get; set; }

        public bool IsBait { get; set; }

        public IWorldLocation[] GetInWorldTemplates()
        {
            return (IWorldLocation[]) null;
        }

        public Asda2Item Create()
        {
            return this.Creator();
        }

        private void OnRecordCreated(Asda2ItemRecord record)
        {
        }

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
            this.OnRecordCreated(record);
            Action<Asda2ItemRecord> created = this.Created;
            if (created == null)
                return;
            created(record);
        }

        internal void NotifyEquip(Asda2Item item)
        {
            Action<Asda2Item> equipped = this.Equipped;
            if (equipped == null)
                return;
            equipped(item);
        }

        internal void NotifyUnequip(Asda2Item item)
        {
            Action<Asda2Item> unequipped = this.Unequipped;
            if (unequipped == null)
                return;
            unequipped(item);
        }

        internal void NotifyUsed(Asda2Item item)
        {
            Action<Asda2Item> used = this.Used;
            if (used == null)
                return;
            used(item);
        }

        public override string ToString()
        {
            return string.Format("{0} (Id: {1})", (object) this.Name, (object) this.Id);
        }
    }
}