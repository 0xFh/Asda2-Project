using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Core.DBC;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
    public class SummonPropertiesConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            SpellSummonEntry spellSummonEntry = new SpellSummonEntry()
            {
                Id = (SummonType) rawData.GetInt32(0U),
                Group = (SummonGroup) rawData.GetInt32(1U),
                FactionTemplateId = (FactionTemplateId) rawData.GetInt32(2U),
                Type = (SummonPropertyType) rawData.GetInt32(3U),
                Slot = rawData.GetUInt32(4U),
                Flags = (SummonFlags) rawData.GetInt32(5U)
            };
            SpellHandler.SummonEntries[spellSummonEntry.Id] = spellSummonEntry;
        }
    }
}