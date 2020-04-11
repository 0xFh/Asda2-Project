using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Entities
{
    public static class SetItemManager
    {
        public static Dictionary<int, SetItemDataRecord> ItemSetsRecordsByItemIds =
            new Dictionary<int, SetItemDataRecord>();

        [WCell.Core.Initialization.Initialization(InitializationPass.First, Name = "Items sets system.")]
        public static void Init()
        {
            ContentMgr.Load<SetItemDataRecord>();
            foreach (KeyValuePair<int, SetItemDataRecord> setsRecordsByItemId in SetItemManager.ItemSetsRecordsByItemIds
            )
                ;
        }

        public static SetItemDataRecord GetSetItemRecord(int id)
        {
            if (!SetItemManager.ItemSetsRecordsByItemIds.ContainsKey(id))
                return (SetItemDataRecord) null;
            return SetItemManager.ItemSetsRecordsByItemIds[id];
        }
    }
}