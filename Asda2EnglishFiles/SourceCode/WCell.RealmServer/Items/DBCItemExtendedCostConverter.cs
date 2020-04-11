using Cell.Core;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Core.DBC;

namespace WCell.RealmServer.Items
{
    public class DBCItemExtendedCostConverter : AdvancedDBCRecordConverter<ItemExtendedCostEntry>
    {
        public override ItemExtendedCostEntry ConvertTo(byte[] rawData, ref int id)
        {
            uint num = 0;
            id = (int) rawData.GetUInt32(num++);
            uint num2 = rawData.GetUInt32(num++);
            uint num3 = rawData.GetUInt32(num++);
            num++;
            uint num4 = rawData.GetUInt32(num++);
            Asda2ItemId id2 = (Asda2ItemId) rawData.GetUInt32(num++);
            Asda2ItemId id3 = (Asda2ItemId) rawData.GetUInt32(num++);
            Asda2ItemId id4 = (Asda2ItemId) rawData.GetUInt32(num++);
            Asda2ItemId id5 = (Asda2ItemId) rawData.GetUInt32(num++);
            Asda2ItemId id6 = (Asda2ItemId) rawData.GetUInt32(num++);
            int num5 = rawData.GetInt32(num++);
            int num6 = rawData.GetInt32(num++);
            int num7 = rawData.GetInt32(num++);
            int num8 = rawData.GetInt32(num++);
            int num9 = rawData.GetInt32(num++);
            uint num10 = rawData.GetUInt32(num++);
            List<ItemExtendedCostEntry.RequiredItem> list2 = new List<ItemExtendedCostEntry.RequiredItem>(5);
            ItemExtendedCostEntry.RequiredItem item = new ItemExtendedCostEntry.RequiredItem
            {
                Id = id2,
                Cost = num5
            };
            list2.Add(item);
            ItemExtendedCostEntry.RequiredItem item2 = new ItemExtendedCostEntry.RequiredItem
            {
                Id = id3,
                Cost = num6
            };
            list2.Add(item2);
            ItemExtendedCostEntry.RequiredItem item3 = new ItemExtendedCostEntry.RequiredItem
            {
                Id = id4,
                Cost = num7
            };
            list2.Add(item3);
            ItemExtendedCostEntry.RequiredItem item4 = new ItemExtendedCostEntry.RequiredItem
            {
                Id = id5,
                Cost = num8
            };
            list2.Add(item4);
            ItemExtendedCostEntry.RequiredItem item5 = new ItemExtendedCostEntry.RequiredItem
            {
                Id = id6,
                Cost = num9
            };
            list2.Add(item5);
            List<ItemExtendedCostEntry.RequiredItem> list = list2;
            return new ItemExtendedCostEntry
            {
                Id = (uint) id,
                HonorCost = num2,
                ArenaPointCost = num3,
                Unk_322 = num4,
                RequiredItems = list,
                ReqArenaRating = num10
            };
        }
    }
}