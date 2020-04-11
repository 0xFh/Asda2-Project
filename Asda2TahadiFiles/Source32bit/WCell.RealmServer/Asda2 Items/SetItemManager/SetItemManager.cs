using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Entities
{
  public static class SetItemManager
  {
    public static Dictionary<int, SetItemDataRecord> ItemSetsRecordsByItemIds =
      new Dictionary<int, SetItemDataRecord>();

    [Initialization(InitializationPass.First, Name = "Items sets system.")]
    public static void Init()
    {
      ContentMgr.Load<SetItemDataRecord>();
      foreach(KeyValuePair<int, SetItemDataRecord> setsRecordsByItemId in ItemSetsRecordsByItemIds
      )
        ;
    }

    public static SetItemDataRecord GetSetItemRecord(int id)
    {
      if(!ItemSetsRecordsByItemIds.ContainsKey(id))
        return null;
      return ItemSetsRecordsByItemIds[id];
    }
  }
}