using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;
using WCell.Util.Variables;

namespace WCell.RealmServer.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Config", "Cfg");
            this.EnglishDescription = "Provides commands to manage the Configuration.";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public class SetGlobalCommand : RealmServerCommand.SubCommand
        {
            protected SetGlobalCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Set", "S");
                this.EnglishParamInfo = "<globalVar> <value>";
                this.EnglishDescription = "Sets the value of the given global variable.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IConfiguration config = CommandUtil.GetConfig((IConfiguration) RealmServerConfiguration.Instance,
                    (ITriggerer) trigger);
                if (config == null)
                    return;
                CommandUtil.SetCfgValue(config, (ITriggerer) trigger);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class GetGlobalCommand : RealmServerCommand.SubCommand
        {
            protected GetGlobalCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Get", "G");
                this.EnglishParamInfo = "<globalVar>";
                this.EnglishDescription = "Gets the value of the given global variable.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IConfiguration config = CommandUtil.GetConfig((IConfiguration) RealmServerConfiguration.Instance,
                    (ITriggerer) trigger);
                if (config == null)
                    return;
                CommandUtil.GetCfgValue(config, (ITriggerer) trigger);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ListGlobalsCommand : RealmServerCommand.SubCommand
        {
            protected ListGlobalsCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("List", "L");
                this.EnglishParamInfo = "[<name Part>]";
                this.EnglishDescription =
                    "Lists all global variables. If specified only shows variables that contain the given name Part.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IConfiguration config = CommandUtil.GetConfig((IConfiguration) RealmServerConfiguration.Instance,
                    (ITriggerer) trigger);
                if (config == null)
                    return;
                CommandUtil.ListCfgValues(config, (ITriggerer) trigger);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class SaveConfigCommand : RealmServerCommand.SubCommand
        {
            protected SaveConfigCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Save");
                this.EnglishParamInfo = "";
                this.EnglishDescription = "Saves the current configuration.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IConfiguration config = CommandUtil.GetConfig((IConfiguration) RealmServerConfiguration.Instance,
                    (ITriggerer) trigger);
                if (config == null)
                    return;
                config.Save(true, false);
                trigger.Reply("Done.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class LoadConfigCommand : RealmServerCommand.SubCommand
        {
            protected LoadConfigCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Load");
                this.EnglishParamInfo = "";
                this.EnglishDescription = "Loads the configuration again.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                IConfiguration config = CommandUtil.GetConfig((IConfiguration) RealmServerConfiguration.Instance,
                    (ITriggerer) trigger);
                if (config == null)
                    return;
                config.Load();
                trigger.Reply("Done.");
            }
        }
    }
}