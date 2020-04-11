using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.NPCs.Spawns;

namespace WCell.RealmServer.Editor.Menus
{
    public class WaypointEditorMenu : SpawnEditorMenu
    {
        public override string GetText(GossipConversation convo)
        {
            return "";
        }

        public WaypointEditorMenu(MapEditor editor, NPCSpawnPoint spawnPoint, EditorFigurine figurine)
            : base(editor, spawnPoint, figurine)
        {
        }
    }
}