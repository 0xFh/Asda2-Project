using WCell.RealmServer.Editor.Figurines;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs.Spawns;

namespace WCell.RealmServer.Editor.Menus
{
    public class SpawnPointEditorMenu : SpawnEditorMenu
    {
        public override string GetText(GossipConversation convo)
        {
            return this.SpawnPoint.ToString();
        }

        public SpawnPointEditorMenu(MapEditor editor, NPCSpawnPoint spawnPoint, EditorFigurine figurine)
            : base(editor, spawnPoint, figurine)
        {
            this.AddItem((GossipMenuItemBase) new LocalizedGossipMenuItem(
                (GossipActionHandler) (convo => this.MoveTo(convo.Character)),
                RealmLangKey.EditorSpawnPointMenuMoveOverHere, new object[0]));
            this.AddQuitMenuItem(RealmLangKey.Done);
        }

        private void MoveTo(Character chr)
        {
            this.SpawnPoint.SpawnEntry.Position = chr.Position;
            this.Figurine.TeleportTo(chr.Position);
        }
    }
}