using System;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Content;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ContentCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Content", "Cont");
            this.EnglishDescription = "Provides commands to manage the static content.";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public override bool RequiresCharacter
        {
            get { return false; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.None; }
        }

        public class ContentLoadCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Load", "L", "Reload");
                this.EnglishDescription =
                    "Reloads the content-definitions. This is useful when applying changes to the underlying content system.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Reply("Loading content-mapping information...");
                ContentMgr.Load();
                trigger.Reply("Done.");
            }
        }

        public class ContentCheckCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Check", "Ch", "C");
                this.EnglishDescription =
                    "Checks whether all currently loaded content-definitions are correctly reflecting the DB-structure.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Reply("Checking Content-Definitions...");
                int num = ContentMgr.Check(new Action<string>(trigger.Reply));
                trigger.Reply("Done - Found {0} error(s).", (object) num);
            }
        }
    }
}