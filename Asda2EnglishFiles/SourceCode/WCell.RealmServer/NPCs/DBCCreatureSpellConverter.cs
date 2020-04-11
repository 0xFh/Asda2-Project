using System.Collections.Generic;
using WCell.Core.DBC;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.NPCs
{
    public class DBCCreatureSpellConverter : AdvancedDBCRecordConverter<Spell[]>
    {
        public override Spell[] ConvertTo(byte[] rawData, ref int id)
        {
            List<Spell> spellList = new List<Spell>(4);
            id = (int) DBCRecordConverter.GetUInt32(rawData, 0);
            for (int field = 1; field <= 4; ++field)
            {
                uint uint32 = DBCRecordConverter.GetUInt32(rawData, field);
                Spell spell;
                if (uint32 != 0U && (spell = SpellHandler.Get(uint32)) != null)
                    spellList.Add(spell);
            }

            return spellList.ToArray();
        }
    }
}