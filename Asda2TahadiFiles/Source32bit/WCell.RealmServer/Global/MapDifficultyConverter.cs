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
      mapDifficultyEntry.Id = (uint) GetInt32(rawData, 0);
      mapDifficultyEntry.MapId = (MapId) GetUInt32(rawData, 1);
      mapDifficultyEntry.Index = GetUInt32(rawData, 2);
      mapDifficultyEntry.RequirementString = GetString(rawData, 3);
      mapDifficultyEntry.ResetTime = GetInt32(rawData, 20);
      mapDifficultyEntry.MaxPlayerCount = GetInt32(rawData, 21);
      MapTemplate mapTemplate = World.GetMapTemplate(mapDifficultyEntry.MapId);
      if(mapTemplate == null)
        return;
      if(mapDifficultyEntry.Index >= 4.0)
      {
        LogManager.GetCurrentClassLogger().Warn("Invalid MapDifficulty for {0} with Index {1}.",
          mapDifficultyEntry.MapId, mapDifficultyEntry.Index);
      }
      else
      {
        if(mapDifficultyEntry.MaxPlayerCount == 0)
          mapDifficultyEntry.MaxPlayerCount = mapTemplate.MaxPlayerCount;
        if(mapTemplate.Difficulties == null)
          mapTemplate.Difficulties = new MapDifficultyEntry[4];
        mapDifficultyEntry.Finalize(mapTemplate);
        mapTemplate.Difficulties[mapDifficultyEntry.Index] = mapDifficultyEntry;
      }
    }
  }
}