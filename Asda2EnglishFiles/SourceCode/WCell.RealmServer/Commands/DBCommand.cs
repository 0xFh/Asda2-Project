using NHibernate.Cfg;
using NHibernate.Engine;
using WCell.Core.Database;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class DBCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("DB", "Database");
            this.EnglishParamInfo = "";
            this.EnglishDescription = "Offers commands to manipulate or interact with the DB.";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public class DropDBCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Drop", "Purge");
                this.EnglishParamInfo = "";
                this.EnglishDescription =
                    "WARNING: This drops and re-creates the entire internal WCell Database Schema.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Reply("Recreating Database Schema...");
                trigger.Reply("Idi NAHUI!.");
            }
        }

        public class DBInfoCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Info", "?");
                this.EnglishParamInfo = "";
                this.EnglishDescription = "Shows some info about the DB currently being used.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Settings settings = DatabaseUtil.Settings;
                ISessionImplementor session = DatabaseUtil.Session;
                trigger.Reply("DB Provider: " + settings.Dialect.GetType().Name);
                trigger.Reply(" State: " + (object) session.Connection.State);
                trigger.Reply(" Database: " + session.Connection.Database);
                trigger.Reply(" Connection String: " + session.Connection.ConnectionString);
            }
        }
    }
}