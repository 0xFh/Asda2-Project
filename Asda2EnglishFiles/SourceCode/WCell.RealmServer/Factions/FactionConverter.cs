using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.DBC;

namespace WCell.RealmServer.Factions
{
    public class FactionConverter : AdvancedDBCRecordConverter<FactionEntry>
    {
        public override FactionEntry ConvertTo(byte[] rawData, ref int id)
        {
            FactionEntry factionEntry = new FactionEntry();
            id = (int) (factionEntry.Id = (FactionId) DBCRecordConverter.GetUInt32(rawData, 0));
            factionEntry.FactionIndex = (FactionReputationIndex) DBCRecordConverter.GetInt32(rawData, 1);
            factionEntry.RaceMask = new RaceMask[4];
            for (int index = 0; index < factionEntry.RaceMask.Length; ++index)
                factionEntry.RaceMask[index] = (RaceMask) DBCRecordConverter.GetUInt32(rawData, 2 + index);
            factionEntry.ClassMask = new ClassMask[4];
            for (int index = 0; index < factionEntry.ClassMask.Length; ++index)
                factionEntry.ClassMask[index] = (ClassMask) DBCRecordConverter.GetUInt32(rawData, 6 + index);
            factionEntry.BaseRepValue = new int[4];
            for (int index = 0; index < factionEntry.BaseRepValue.Length; ++index)
                factionEntry.BaseRepValue[index] = DBCRecordConverter.GetInt32(rawData, 10 + index);
            factionEntry.BaseFlags = new FactionFlags[4];
            for (int index = 0; index < factionEntry.BaseFlags.Length; ++index)
                factionEntry.BaseFlags[index] = (FactionFlags) DBCRecordConverter.GetInt32(rawData, 14 + index);
            factionEntry.ParentId = (FactionId) DBCRecordConverter.GetUInt32(rawData, 18);
            factionEntry.Name = this.GetString(rawData, 23);
            return factionEntry;
        }
    }
}