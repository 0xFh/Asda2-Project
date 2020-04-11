using NLog;
using NLog.Config;
using WCell.Constants.Updates;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class LogCommand : RealmServerCommand
    {
        protected LogCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Log");
            this.EnglishDescription = "Gets and sets logging settings.";
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

        public class ToggleLevelCommand : RealmServerCommand.SubCommand
        {
            protected ToggleLevelCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("ToggleLevel", "Level", "Lvl", "TL");
                this.EnglishParamInfo = "<Trace|Debug|Info|Warn|Error|Fatal> [<1/0>]";
                this.EnglishDescription =
                    "Globally toggles whether messages of the corresponding level should be logged (to console, as well as to file or any other target that is specified).";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                LogLevel level = LogLevel.FromString(trigger.Text.NextWord());
                bool flag1 = trigger.Text.HasNext && trigger.Text.NextBool();
                foreach (LoggingRule loggingRule in LogManager.Configuration.LoggingRules)
                {
                    bool flag2 = flag1 || !loggingRule.IsLoggingEnabledForLevel(level);
                    if (flag2)
                        loggingRule.EnableLoggingForLevel(level);
                    else
                        loggingRule.DisableLoggingForLevel(level);
                    trigger.Reply("{0}-Messages for \"{1}\" {2}.", (object) level,
                        (object) loggingRule.LoggerNamePattern, flag2 ? (object) "enabled" : (object) "disabled");
                }
            }
        }
    }
}