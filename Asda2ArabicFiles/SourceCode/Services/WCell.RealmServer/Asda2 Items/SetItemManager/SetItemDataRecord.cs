using System.Linq;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.Util.Data;
using System.Collections.Generic;

namespace WCell.RealmServer.Entities
{
    public static class SetItemManager
    {
        public static Dictionary<int,SetItemDataRecord> ItemSetsRecordsByItemIds = new Dictionary<int, SetItemDataRecord>();
        [Initialization (InitializationPass.First,Name = "Items sets system.")]
        public static void Init()
        {
            ContentMgr.Load<SetItemDataRecord>();
            foreach (var itemRecords in ItemSetsRecordsByItemIds)
            {
                
            }
        }

        public static SetItemDataRecord GetSetItemRecord(int id)
        {
            return ItemSetsRecordsByItemIds.ContainsKey(id) ? ItemSetsRecordsByItemIds[id] : null;
        }
    }

    [DataHolder]
    public class SetItemDataRecord : IDataHolder
    {
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
        public List<Asda2SetBonus> SetBonuses  = new List<Asda2SetBonus>();
        public int MaxItemsCount { get; set; }
        public int Steps { get; set; }
        public void FinalizeDataHolder()
        {
            foreach (var itemId in ItemIds.Where(itemId => itemId != -1).Where(itemId => !SetItemManager.ItemSetsRecordsByItemIds.ContainsKey(itemId)))
                SetItemManager.ItemSetsRecordsByItemIds.Add(itemId, this);
            MaxItemsCount = ItemIds.Count(i => i > 0);
            if (Stat1Type > 0)
                SetBonuses.Add(new Asda2SetBonus{Type = Stat1Type,Value = Stat1Value});
            if (Stat2Type > 0)
                SetBonuses.Add(new Asda2SetBonus { Type = Stat2Type, Value = Stat2Value });
            if (Stat3Type > 0)
                SetBonuses.Add(new Asda2SetBonus { Type = Stat3Type, Value = Stat3Value });
            foreach (var asda2SetBonuse in SetBonuses)
            {
                if (((Asda2ItemBonusType)asda2SetBonuse.Type) == Asda2ItemBonusType.HpRegeneration)
                    asda2SetBonuse.Value = (int) ((asda2SetBonuse.Value + 0.5f)/5);
            }
        }

        public Asda2SetBonus GetBonus(byte itemsAppliedCount)
        {
            var diff = MaxItemsCount - itemsAppliedCount;
            var bonusIndex = SetBonuses.Count - 1 - diff;
            if (bonusIndex>=0&&bonusIndex<SetBonuses.Count)
                return SetBonuses[bonusIndex];
            return null;
        }

    }
    public class Asda2SetBonus
    {
        public int Type;
        public int Value;
    }
       
}