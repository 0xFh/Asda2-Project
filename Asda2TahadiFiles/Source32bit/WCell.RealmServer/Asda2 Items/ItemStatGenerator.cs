using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Items
{
  public class ItemStatGenerator
  {
    public static ItemStatBonus EmptyBonus = new ItemStatBonus
    {
      Chance = 1000000000,
      Type = Asda2ItemBonusType.None,
      MinValue = 0,
      MaxValue = 0
    };

    public List<ItemStatBonus> PosibleBonuses = new List<ItemStatBonus>();
    public const int MaximumChance = 1000000000;

    public ItemStatBonus GetBonus()
    {
      int num1 = Utility.Random(0, 1000000000);
      int num2 = 0;
      foreach(ItemStatBonus posibleBonuse in PosibleBonuses)
      {
        num2 += posibleBonuse.Chance;
        if(num2 >= num1)
          return posibleBonuse;
      }

      return EmptyBonus;
    }

    public void AlignChances()
    {
      if(PosibleBonuses.Count == 0)
        return;
      int num1 = 1000000000 /
                 PosibleBonuses.Sum(pb => pb.Chance);
      foreach(ItemStatBonus posibleBonuse in PosibleBonuses)
        posibleBonuse.Chance *= num1;
      int num2 = PosibleBonuses.Sum(pb => pb.Chance);
      if(num2 != 1000000000)
      {
        int num3 = 1000000000 - num2;
        ItemStatBonus itemStatBonus = PosibleBonuses[0];
        foreach(ItemStatBonus posibleBonuse in PosibleBonuses)
        {
          if(itemStatBonus.Chance < posibleBonuse.Chance)
            itemStatBonus = posibleBonuse;
        }

        itemStatBonus.Chance += num3;
      }

      if(PosibleBonuses.Sum(pb => pb.Chance) == 1000000000)
        return;
      LogUtil.ErrorException("FAILED TO ALIGN CHANCES!!!!!!!!!!!!!!!!!!!");
    }
  }
}