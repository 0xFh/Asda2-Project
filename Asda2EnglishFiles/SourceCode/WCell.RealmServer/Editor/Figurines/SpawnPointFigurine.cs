using WCell.Constants.NPCs;
using WCell.RealmServer.Editor.Menus;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Variables;

namespace WCell.RealmServer.Editor.Figurines
{
    /// <summary>The visual component of a spawnpoint</summary>
    public class SpawnPointFigurine : EditorFigurine
    {
        /// <summary>
        /// Scales the figurine in relation to its original version
        /// </summary>
        [NotVariable] public static float SpawnFigScale = 0.5f;

        public SpawnPointFigurine(MapEditor editor, NPCSpawnPoint spawnPoint)
            : base(editor, spawnPoint)
        {
            this.m_position = spawnPoint.Position;
            this.NPCFlags = NPCFlags.Gossip;
        }

        public override SpawnEditorMenu CreateEditorMenu()
        {
            return (SpawnEditorMenu) new SpawnPointEditorMenu(this.Editor, this.SpawnPoint, (EditorFigurine) this);
        }

        public override float DefaultScale
        {
            get { return SpawnPointFigurine.SpawnFigScale; }
        }

        public override Faction Faction
        {
            get { return this.m_SpawnPoint.SpawnEntry.Entry.RandomFaction; }
            set { }
        }
    }
}