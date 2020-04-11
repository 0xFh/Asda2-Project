using WCell.Constants.NPCs;
using WCell.RealmServer.Editor.Menus;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Variables;

namespace WCell.RealmServer.Editor.Figurines
{
    public class WaypointFigurine : EditorFigurine
    {
        /// <summary>
        /// Scales the figurine in relation to its original version
        /// </summary>
        [NotVariable] public static float WPFigurineScale = 0.4f;

        private readonly WaypointEntry m_Waypoint;

        public WaypointFigurine(MapEditor editor, NPCSpawnPoint spawnPoint, WaypointEntry wp)
            : base(editor, spawnPoint)
        {
            this.m_Waypoint = wp;
            this.NPCFlags = NPCFlags.Gossip;
        }

        public WaypointEntry Waypoint
        {
            get { return this.m_Waypoint; }
        }

        public override SpawnEditorMenu CreateEditorMenu()
        {
            return (SpawnEditorMenu) new WaypointEditorMenu(this.Editor, this.SpawnPoint, (EditorFigurine) this);
        }

        public override float DefaultScale
        {
            get { return WaypointFigurine.WPFigurineScale; }
        }

        public override Faction Faction
        {
            get { return this.SpawnPoint.SpawnEntry.Entry.RandomFaction; }
            set { }
        }
    }
}