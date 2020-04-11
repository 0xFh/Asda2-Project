using NLog;
using System;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Global
{
    public class MapDifficultyConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            MapDifficultyEntry mapDifficultyEntry = new MapDifficultyEntry();
            mapDifficultyEntry.Id = (uint) DBCRecordConverter.GetInt32(rawData, 0);
            mapDifficultyEntry.MapId = (MapId) DBCRecordConverter.GetUInt32(rawData, 1);
            mapDifficultyEntry.Index = DBCRecordConverter.GetUInt32(rawData, 2);
            mapDifficultyEntry.RequirementString = this.GetString(rawData, 3);
            mapDifficultyEntry.ResetTime = DBCRecordConverter.GetInt32(rawData, 20);
            mapDifficultyEntry.MaxPlayerCount = DBCRecordConverter.GetInt32(rawData, 21);
            MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(mapDifficultyEntry.MapId);
            if (mapTemplate == null)
                return;
            if ((double) mapDifficultyEntry.Index >= 4.0)
            {
                LogManager.GetCurrentClassLogger().Warn("Invalid MapDifficulty for {0} with Index {1}.",
                    (object) mapDifficultyEntry.MapId, (object) mapDifficultyEntry.Index);
            }
            else
            {
                if (mapDifficultyEntry.MaxPlayerCount == 0)
                    mapDifficultyEntry.MaxPlayerCount = mapTemplate.MaxPlayerCount;
                if (mapTemplate.Difficulties == null)
                    mapTemplate.Difficulties = new MapDifficultyEntry[4];
                mapDifficultyEntry.Finalize(mapTemplate);
                mapTemplate.Difficulties[mapDifficultyEntry.Index] = mapDifficultyEntry;
            }
        }
    }
}