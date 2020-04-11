using System.Reflection;
using WCell.Constants.Updates;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ModAurasCommand : RealmServerCommand
    {
        protected ModAurasCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("ModAuras", "MAura");
            this.EnglishParamInfo = "<n> <subcommand> ...";
            this.EnglishDescription = "Modifies the nth Aura of the target";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            this.ProcessNth(trigger);
        }

        public override bool RequiresCharacter
        {
            get { return false; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }

        public class ModLevelCommand : RealmServerCommand.SubCommand
        {
            protected ModLevelCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Level", "Lvl", "L");
                this.EnglishParamInfo = "<AuraLevel>";
                this.EnglishDescription = "Modifies the Level of the nth Aura.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint n = ((RealmServerNCmdArgs) trigger.Args).N - 1U;
                Aura at = trigger.Args.Target.Auras.GetAt(n);
                if (at != null)
                    ModPropCommand.ModProp((object) at, (MemberInfo) at.GetType().GetProperty("Level"), trigger);
                else
                    trigger.Reply("There aren't " + (object) n + " Auras.");
            }
        }

        public class ModFlagsCommand : RealmServerCommand.SubCommand
        {
            protected ModFlagsCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Flags", "Fl", "F");
                this.EnglishParamInfo = "<AuraFlags>";
                this.EnglishDescription = "Modifies the Flags of the nth Aura.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint n = ((RealmServerNCmdArgs) trigger.Args).N - 1U;
                Aura at = trigger.Args.Target.Auras.GetAt(n);
                if (at != null)
                    ModPropCommand.ModProp((object) at, (MemberInfo) at.GetType().GetProperty("Flags"), trigger);
                else
                    trigger.Reply("There aren't " + (object) n + " Auras.");
            }
        }
    }
}