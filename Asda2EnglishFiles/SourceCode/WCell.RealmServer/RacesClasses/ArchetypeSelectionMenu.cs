using WCell.RealmServer.Gossips;

namespace WCell.RealmServer.RacesClasses
{
    public class ArchetypeSelectionMenu : GossipMenu
    {
        public static readonly StaticGossipEntry RaceTextEntry = new StaticGossipEntry(813255U, new string[1]
        {
            "Select your Race"
        });

        public static readonly StaticGossipEntry ClassTextEntry = new StaticGossipEntry(813256U, new string[1]
        {
            "Select your Class"
        });

        public ArchetypeSelectionMenu(ArchetypeSelectionHandler callback)
            : this(callback, ArchetypeSelectionMenu.RaceTextEntry.GossipId,
                ArchetypeSelectionMenu.ClassTextEntry.GossipId)
        {
        }

        public ArchetypeSelectionMenu(ArchetypeSelectionHandler callback, uint raceTextId, uint clssTextId)
            : base(raceTextId)
        {
            foreach (BaseClass baseClass in ArchetypeMgr.BaseClasses)
            {
                if (baseClass != null)
                    this.AddItem((GossipMenuItemBase) new GossipMenuItem(baseClass.Id.ToString(),
                        (GossipMenu) new ClassSelectionMenu(baseClass, callback, clssTextId)));
            }
        }
    }
}