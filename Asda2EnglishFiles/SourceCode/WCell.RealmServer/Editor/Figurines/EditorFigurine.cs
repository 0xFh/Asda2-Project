using WCell.RealmServer.Editor.Menus;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Variables;

namespace WCell.RealmServer.Editor.Figurines
{
    /// <summary>
    /// These figurines represent the 3D GUI elements of the editor
    /// </summary>
    public abstract class EditorFigurine : FigurineBase
    {
        /// <summary>
        /// Whether to also spawn a DO to make this Figurine appear clearer
        /// </summary>
        [NotVariable] protected readonly NPCSpawnPoint m_SpawnPoint;

        protected EditorFigurine(MapEditor editor, NPCSpawnPoint spawnPoint)
            : base(spawnPoint.SpawnEntry.Entry.NPCId)
        {
            this.Editor = editor;
            this.m_SpawnPoint = spawnPoint;
        }

        public MapEditor Editor { get; private set; }

        public abstract SpawnEditorMenu CreateEditorMenu();

        protected internal override void OnEncounteredBy(Character chr)
        {
            base.OnEncounteredBy(chr);
            if (this.GossipMenu != null)
                return;
            this.GossipMenu = (GossipMenu) this.CreateEditorMenu();
        }

        /// <summary>Editor is only visible to staff members</summary>
        public override VisibilityStatus DetermineVisibilityFor(Unit observer)
        {
            return observer is Character && this.Editor.Team.ContainsKey(observer.EntityId.Low)
                ? VisibilityStatus.Visible
                : VisibilityStatus.Invisible;
        }
    }
}