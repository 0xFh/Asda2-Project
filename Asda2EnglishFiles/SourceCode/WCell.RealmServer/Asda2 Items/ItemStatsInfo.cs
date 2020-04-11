using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
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
            switch (this.ClassMask)
            {
                case Asda2Profession.Any:
                    this.InitByStatSlot(Asda2Profession.Mage);
                    this.InitByStatSlot(Asda2Profession.Warrior);
                    this.InitByStatSlot(Asda2Profession.Archer);
                    this.InitByStatSlot(Asda2Profession.NoProfession);
                    break;
                case Asda2Profession.ArcherAndWarrior:
                    this.InitByStatSlot(Asda2Profession.Warrior);
                    this.InitByStatSlot(Asda2Profession.Archer);
                    break;
                default:
                    this.InitByStatSlot(this.ClassMask);
                    break;
            }
        }

        private void InitByStatSlot(Asda2Profession proff)
        {
            if (this.StatSlot == ItemStatsSlots.Any)
            {
                this.InitByItemType(ItemStatsSlots.Common, proff);
                this.InitByItemType(ItemStatsSlots.Craft, proff);
                this.InitByItemType(ItemStatsSlots.Enchant, proff);
                this.InitByItemType(ItemStatsSlots.Advanced, proff);
            }
            else
                this.InitByItemType(this.StatSlot, proff);
        }

        private void InitByItemType(ItemStatsSlots statSlot, Asda2Profession proffession)
        {
            switch (this.ItemType)
            {
                case Asda2EquipmentSlots.AnyArmor:
                    this.Init(Asda2EquipmentSlots.Head, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Shirt, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Boots, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Gloves, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Pans, statSlot, proffession);
                    break;
                case Asda2EquipmentSlots.AnyAvatar:
                    this.Init(Asda2EquipmentSlots.AvatarBoots, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.AvatarGloves, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.AvatarHead, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.AvatarPans, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.AvatarShirt, statSlot, proffession);
                    break;
                case Asda2EquipmentSlots.Jevelery:
                    this.Init(Asda2EquipmentSlots.RightRing, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.LeftRing, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Nackles, statSlot, proffession);
                    break;
                case Asda2EquipmentSlots.AnyAvatarAccecory:
                    this.Init(Asda2EquipmentSlots.AvaratRightHead, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Wings, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Cape, statSlot, proffession);
                    this.Init(Asda2EquipmentSlots.Accessory, statSlot, proffession);
                    break;
                default:
                    this.Init(this.ItemType, statSlot, proffession);
                    break;
            }
        }

        private void Init(Asda2EquipmentSlots itemType, ItemStatsSlots statSlot, Asda2Profession proffession)
        {
            if (!Asda2ItemMgr.ItemStatsInfos.ContainsKey(proffession))
                Asda2ItemMgr.ItemStatsInfos.Add(proffession,
                    new Dictionary<ItemStatsSlots, Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>>());
            if (!Asda2ItemMgr.ItemStatsInfos[proffession].ContainsKey(statSlot))
                Asda2ItemMgr.ItemStatsInfos[proffession]
                    .Add(statSlot, new Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>());
            if (!Asda2ItemMgr.ItemStatsInfos[proffession][statSlot].ContainsKey(itemType))
                Asda2ItemMgr.ItemStatsInfos[proffession][statSlot].Add(itemType, new List<ItemStatsInfo>());
            Asda2ItemMgr.ItemStatsInfos[proffession][statSlot][itemType].Add(this);
        }
    }
}