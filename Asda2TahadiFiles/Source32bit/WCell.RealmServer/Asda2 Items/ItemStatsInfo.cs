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
      switch(ClassMask)
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
      if(StatSlot == ItemStatsSlots.Any)
      {
        InitByItemType(ItemStatsSlots.Common, proff);
        InitByItemType(ItemStatsSlots.Craft, proff);
        InitByItemType(ItemStatsSlots.Enchant, proff);
        InitByItemType(ItemStatsSlots.Advanced, proff);
      }
      else
        InitByItemType(StatSlot, proff);
    }

    private void InitByItemType(ItemStatsSlots statSlot, Asda2Profession proffession)
    {
      switch(ItemType)
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
        case Asda2EquipmentSlots.Jevelery:
          Init(Asda2EquipmentSlots.RightRing, statSlot, proffession);
          Init(Asda2EquipmentSlots.LeftRing, statSlot, proffession);
          Init(Asda2EquipmentSlots.Nackles, statSlot, proffession);
          break;
        case Asda2EquipmentSlots.AnyAvatarAccecory:
          Init(Asda2EquipmentSlots.AvaratRightHead, statSlot, proffession);
          Init(Asda2EquipmentSlots.Wings, statSlot, proffession);
          Init(Asda2EquipmentSlots.Cape, statSlot, proffession);
          Init(Asda2EquipmentSlots.Accessory, statSlot, proffession);
          break;
        default:
          Init(ItemType, statSlot, proffession);
          break;
      }
    }

    private void Init(Asda2EquipmentSlots itemType, ItemStatsSlots statSlot, Asda2Profession proffession)
    {
      if(!Asda2ItemMgr.ItemStatsInfos.ContainsKey(proffession))
        Asda2ItemMgr.ItemStatsInfos.Add(proffession,
          new Dictionary<ItemStatsSlots, Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>>());
      if(!Asda2ItemMgr.ItemStatsInfos[proffession].ContainsKey(statSlot))
        Asda2ItemMgr.ItemStatsInfos[proffession]
          .Add(statSlot, new Dictionary<Asda2EquipmentSlots, List<ItemStatsInfo>>());
      if(!Asda2ItemMgr.ItemStatsInfos[proffession][statSlot].ContainsKey(itemType))
        Asda2ItemMgr.ItemStatsInfos[proffession][statSlot].Add(itemType, new List<ItemStatsInfo>());
      Asda2ItemMgr.ItemStatsInfos[proffession][statSlot][itemType].Add(this);
    }
  }
}