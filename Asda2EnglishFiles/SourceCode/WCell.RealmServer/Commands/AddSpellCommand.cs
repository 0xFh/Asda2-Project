using WCell.Constants.Updates;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class AddSpellCommand : RealmServerCommand
    {
        protected AddSpellCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("spelladd", "addspell");
            this.EnglishParamInfo = "";
            this.EnglishDescription = "Deprecated - Use \"spell add\" instead.";
            this.Enabled = false;
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            SpellCommand.SpellAddCommand.Instance.Process(trigger);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }
    }
}