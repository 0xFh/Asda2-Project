using WCell.Constants.Items;

namespace WCell.RealmServer.NPCs.Armorer
{
  public class DurabilityCost
  {
    public uint ItemLvl;
    public uint[] Multipliers;

    public uint GetModifierBySubClassId(ItemClass itemClass, ItemSubClass itemSubClass)
    {
      switch(itemClass)
      {
        case ItemClass.Weapon:
          return Multipliers[(int) itemSubClass];
        case ItemClass.Armor:
          return Multipliers[(int) (itemSubClass + 21)];
        default:
          return 0;
      }
    }

    public DurabilityCost()
    {
      Multipliers = new uint[29];
    }
  }
}