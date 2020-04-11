using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.RacesClasses
{
  public class ArchetypeSelectionMenu : GossipMenu
  {
    public static readonly StaticGossipEntry RaceTextEntry = new StaticGossipEntry(813255U, "Select your Race");

    public static readonly StaticGossipEntry ClassTextEntry = new StaticGossipEntry(813256U, "Select your Class");

    public ArchetypeSelectionMenu(ArchetypeSelectionHandler callback)
      : this(callback, RaceTextEntry.GossipId,
        ClassTextEntry.GossipId)
    {
    }

    public ArchetypeSelectionMenu(ArchetypeSelectionHandler callback, uint raceTextId, uint clssTextId)
      : base(raceTextId)
    {
      foreach(BaseClass baseClass in ArchetypeMgr.BaseClasses)
      {
        if(baseClass != null)
          AddItem(new GossipMenuItem(baseClass.Id.ToString(),
            new ClassSelectionMenu(baseClass, callback, clssTextId)));
      }
    }
  }
}