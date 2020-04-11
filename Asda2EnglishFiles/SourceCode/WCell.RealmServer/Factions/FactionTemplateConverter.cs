using System;
using WCell.Constants.Factions;
using WCell.Core.DBC;

namespace WCell.RealmServer.Factions
{
    public class FactionTemplateConverter : AdvancedDBCRecordConverter<FactionTemplateEntry>
    {
        public override FactionTemplateEntry ConvertTo(byte[] rawData, ref int id)
        {
            FactionTemplateEntry entry = new FactionTemplateEntry();
            int num = 0;
            id = (int) (entry.Id = DBCRecordConverter.GetUInt32(rawData, num++));
            entry.FactionId = (FactionId) DBCRecordConverter.GetUInt32(rawData, num++);
            entry.Flags = (FactionTemplateFlags) DBCRecordConverter.GetUInt32(rawData, num++);
            entry.FactionGroup = (FactionGroupMask) DBCRecordConverter.GetUInt32(rawData, num++);
            entry.FriendGroup = (FactionGroupMask) DBCRecordConverter.GetUInt32(rawData, num++);
            entry.EnemyGroup = (FactionGroupMask) DBCRecordConverter.GetUInt32(rawData, num++);
            entry.EnemyFactions = new FactionId[4];
            for (uint i = 0; i < entry.EnemyFactions.Length; i++)
            {
                entry.EnemyFactions[i] = (FactionId) DBCRecordConverter.GetUInt32(rawData, num++);
            }

            entry.FriendlyFactions = new FactionId[4];
            for (uint j = 0; j < entry.FriendlyFactions.Length; j++)
            {
                entry.FriendlyFactions[j] = (FactionId) DBCRecordConverter.GetUInt32(rawData, num++);
            }

            return entry;
        }
    }
}