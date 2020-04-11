using System;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Global
{
  [Serializable]
  public class MapDifficultyEntry : MapDifficultyDBCEntry
  {
    public static readonly int HeroicResetTime = 86400;
    public static readonly int MaxDungeonPlayerCount = 5;
    public MapTemplate Map;
    public bool IsHeroic;
    public bool IsRaid;

    /// <summary>
    /// Softly bound instances can always be reset but you only x times per hour.
    /// </summary>
    public BindingType BindingType;

    internal void Finalize(MapTemplate map)
    {
      Map = map;
      if(ResetTime == 0)
        ResetTime = map.DefaultResetTime;
      IsHeroic = ResetTime == HeroicResetTime;
      IsRaid = MaxPlayerCount == MaxDungeonPlayerCount;
      BindingType = IsDungeon ? BindingType.Soft : BindingType.Hard;
    }

    public bool IsDungeon
    {
      get
      {
        if(!IsHeroic)
          return !IsRaid;
        return false;
      }
    }
  }
}