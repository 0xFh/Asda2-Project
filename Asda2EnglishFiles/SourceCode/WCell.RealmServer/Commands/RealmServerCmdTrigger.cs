using WCell.Util.Commands;
using WCell.Util.Strings;

namespace WCell.RealmServer.Commands
{
    public abstract class RealmServerCmdTrigger : CmdTrigger<RealmServerCmdArgs>
    {
        protected RealmServerCmdTrigger()
        {
        }

        protected RealmServerCmdTrigger(StringStream text, RealmServerCmdArgs args)
            : base(text, args)
        {
        }

        protected RealmServerCmdTrigger(StringStream text, BaseCommand<RealmServerCmdArgs> selectedCmd,
            RealmServerCmdArgs args)
            : base(text, selectedCmd, args)
        {
        }

        protected RealmServerCmdTrigger(RealmServerCmdArgs args)
            : base(args)
        {
        }
    }
}