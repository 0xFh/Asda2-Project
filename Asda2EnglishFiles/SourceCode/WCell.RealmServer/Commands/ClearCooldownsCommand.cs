using WCell.Constants.Updates;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ClearCooldownsCommand : RealmServerCommand
    {
        protected ClearCooldownsCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("ClearCooldowns");
            this.Description = new TranslatableItem(RealmLangKey.CmdSpellClearDescription, new object[0]);
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger.Args.Target.HasSpells)
            {
                trigger.Args.Target.Spells.ClearCooldowns();
                trigger.Reply(RealmLangKey.CmdSpellClearResponse);
            }
            else
                trigger.Reply(RealmLangKey.CmdSpellClearError);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}