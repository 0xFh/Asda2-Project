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
      id = (int) (factionEntry.Id = (FactionId) GetUInt32(rawData, 0));
      factionEntry.FactionIndex = (FactionReputationIndex) GetInt32(rawData, 1);
      factionEntry.RaceMask = new RaceMask[4];
      for(int index = 0; index < factionEntry.RaceMask.Length; ++index)
        factionEntry.RaceMask[index] = (RaceMask) GetUInt32(rawData, 2 + index);
      factionEntry.ClassMask = new ClassMask[4];
      for(int index = 0; index < factionEntry.ClassMask.Length; ++index)
        factionEntry.ClassMask[index] = (ClassMask) GetUInt32(rawData, 6 + index);
      factionEntry.BaseRepValue = new int[4];
      for(int index = 0; index < factionEntry.BaseRepValue.Length; ++index)
        factionEntry.BaseRepValue[index] = GetInt32(rawData, 10 + index);
      factionEntry.BaseFlags = new FactionFlags[4];
      for(int index = 0; index < factionEntry.BaseFlags.Length; ++index)
        factionEntry.BaseFlags[index] = (FactionFlags) GetInt32(rawData, 14 + index);
      factionEntry.ParentId = (FactionId) GetUInt32(rawData, 18);
      factionEntry.Name = GetString(rawData, 23);
      return factionEntry;
    }
  }
}