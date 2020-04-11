using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WCell.Constants.Items;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
  [DataHolder]
  public class AvatarDisasembleRecord : IDataHolder
  {
    public int Id { get; set; }

    public int IsRegular { get; set; }

    public int Level { get; set; }

    [Persistent(Length = 10)]
    public int[] ItemIds { get; set; }

    [Persistent(Length = 10)]
    public int[] Chances { get; set; }

    public string ChancesAsString
    {
      get
      {
        return Chances.Aggregate("",
          (current, i) =>
            current + i.ToString(CultureInfo.InvariantCulture) + ",");
      }
    }

    public void FinalizeDataHolder()
    {
      if(IsRegular == 0)
        Asda2ItemMgr.RegularAvatarRecords.SetValue(this, Id);
      else
        Asda2ItemMgr.PremiumAvatarRecords.SetValue(this, Id);
    }

    public Asda2ItemId GetRandomItemId()
    {
      int num1 = Utility.Random(0, 100000);
      int num2 = 0;
      for(int index = 0; index < ItemIds.Length; ++index)
      {
        num2 += Chances[index];
        if(num2 >= num1)
          return (Asda2ItemId) ItemIds[index];
      }

      return Asda2ItemId.BoosterLv90CommonRune31175;
    }
  }
}