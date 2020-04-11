using System.Collections.Generic;
using WCell.Core.DBC;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.NPCs
{
    public class DBCBankBagSlotConverter : AdvancedDBCRecordConverter<uint>
    {
        public override uint ConvertTo(byte[] rawData, ref int id)
        {
            List<Spell> spellList = new List<Spell>(4);
            id = (int) DBCRecordConverter.GetUInt32(rawData, 0);
            return rawData.GetUInt32(1U);
        }
    }
}