using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Data;

namespace WCell.RealmServer.Entities
{
  [DataHolder]
  public class SetItemDataRecord : IDataHolder
  {
    public List<Asda2SetBonus> SetBonuses = new List<Asda2SetBonus>();

    public string Name { get; set; }

    public int Id { get; set; }

    [Persistent(Length = 10)]
    public int[] ItemIds { get; set; }

    public int Stat1Type { get; set; }

    public int Stat1Value { get; set; }

    public int Stat2Type { get; set; }

    public int Stat2Value { get; set; }

    public int Stat3Type { get; set; }

    public int Stat3Value { get; set; }

    public int MaxItemsCount { get; set; }

    public int Steps { get; set; }

    public void FinalizeDataHolder()
    {
      foreach(int key in ItemIds.Where(itemId => itemId != -1)
        .Where(itemId => !SetItemManager.ItemSetsRecordsByItemIds.ContainsKey(itemId)))
        SetItemManager.ItemSetsRecordsByItemIds.Add(key, this);
      MaxItemsCount = ItemIds.Count(i => i > 0);
      if(Stat1Type > 0)
        SetBonuses.Add(new Asda2SetBonus
        {
          Type = Stat1Type,
          Value = Stat1Value
        });
      if(Stat2Type > 0)
        SetBonuses.Add(new Asda2SetBonus
        {
          Type = Stat2Type,
          Value = Stat2Value
        });
      if(Stat3Type > 0)
        SetBonuses.Add(new Asda2SetBonus
        {
          Type = Stat3Type,
          Value = Stat3Value
        });
      foreach(Asda2SetBonus setBonuse in SetBonuses)
      {
        if(setBonuse.Type == 10)
          setBonuse.Value = (int) ((setBonuse.Value + 0.5) / 5.0);
      }
    }

    public Asda2SetBonus GetBonus(byte itemsAppliedCount)
    {
      int index = SetBonuses.Count - 1 - (MaxItemsCount - itemsAppliedCount);
      if(index >= 0 && index < SetBonuses.Count)
        return SetBonuses[index];
      return null;
    }
  }
}