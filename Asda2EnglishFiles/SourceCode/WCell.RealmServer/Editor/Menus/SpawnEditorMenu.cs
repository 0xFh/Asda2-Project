using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.NPCs.Spawns;

namespace WCell.RealmServer.Editor.Menus
{
    public abstract class SpawnEditorMenu : DynamicTextGossipMenu
    {
        public MapEditor Editor { get; private set; }

        public NPCSpawnPoint SpawnPoint { get; set; }

        public EditorFigurine Figurine { get; set; }

        protected SpawnEditorMenu(MapEditor editor, NPCSpawnPoint spawnPoint, EditorFigurine figurine)
        {
            this.Editor = editor;
            this.SpawnPoint = spawnPoint;
            this.Figurine = figurine;
            this.KeepOpen = true;
        }
    }
}