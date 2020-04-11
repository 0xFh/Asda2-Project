using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Editor.Commands
{
    /// <summary>
    /// Is not in the default Commands/ folder because this will be moved into it's own Addon
    /// </summary>
    public class EditorCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Editor", "Edit");
            this.EnglishDescription = "Allows Staff members to edit spawns, waypoints etc.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Character target = trigger.Args.Target as Character;
            if (!trigger.Text.HasNext)
            {
                if (target == null)
                    return;
                MapEditor mapEditor = MapEditorMgr.StartEditing(target.Map, target);
                trigger.ShowMenu(mapEditor.Menu);
            }
            else
                base.Process(trigger);
        }
    }
}