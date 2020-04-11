using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class PushbackCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Pushback");
            this.ParamInfo = new TranslatableItem(RealmLangKey.CmdPushbackParams, new object[0]);
            this.Description = new TranslatableItem(RealmLangKey.CmdPushbackDescription, new object[0]);
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            if (target == null)
                trigger.Reply(RealmLangKey.NoValidTarget);
            else
                target.SpellCast.Pushback(trigger.Text.NextInt(1000));
        }
    }
}