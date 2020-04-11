using Cell.Core;
using System;
using WCell.Core.DBC;

namespace WCell.RealmServer.NPCs.Armorer
{
    public class DBCDurabilityCostsConverter : AdvancedDBCRecordConverter<DurabilityCost>
    {
        public override DurabilityCost ConvertTo(byte[] rawData, ref int id)
        {
            DurabilityCost durabilityCost = new DurabilityCost();
            id = (int) (durabilityCost.ItemLvl = rawData.GetUInt32(0U));
            for (uint field = 1; field < 30U; ++field)
                durabilityCost.Multipliers[field - 1U] = rawData.GetUInt32(field);
            return durabilityCost;
        }
    }
}