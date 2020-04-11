using WCell.Constants;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
  public class LevelStatInfo : IDataHolder
  {
    [NotPersistent]public int[] Stats = new int[6];
    public RaceId Race;
    public ClassId Class;
    public int Level;

    public int Strength
    {
      get { return Stats[0]; }
      set { Stats[0] = value; }
    }

    public int Agility
    {
      get { return Stats[1]; }
      set { Stats[1] = value; }
    }

    public int Stamina
    {
      get { return Stats[2]; }
      set { Stats[2] = value; }
    }

    public int Intellect
    {
      get { return Stats[3]; }
      set { Stats[3] = value; }
    }

    public int Spirit
    {
      get { return Stats[4]; }
      set { Stats[4] = value; }
    }

    public void FinalizeDataHolder()
    {
      int num = Level > 0 ? Level : 1;
      if(num > RealmServerConfiguration.MaxCharacterLevel)
        return;
      Archetype archetype = ArchetypeMgr.GetArchetype(Race, Class);
      if(archetype == null)
        return;
      if(Level == 1)
        archetype.FirstLevelStats = this;
      archetype.LevelStats[num - 1] = this;
    }
  }
}