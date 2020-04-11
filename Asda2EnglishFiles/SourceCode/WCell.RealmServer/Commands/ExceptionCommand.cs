using System;
using System.Linq;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Misc;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ExceptionCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("Exception", "Excep");
        }

        public class ListExceptionCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("List", "L");
                this.EnglishParamInfo = "[<match>]";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string remainder = trigger.Text.Remainder;
                int num = 0;
                for (int index = 0; index < ExceptionHandler.Exceptions.Count; ++index)
                {
                    ExceptionInfo exception = ExceptionHandler.Exceptions[index];
                    if (remainder.Length == 0 ||
                        exception.ToString().IndexOf(remainder, StringComparison.InvariantCultureIgnoreCase) > -1)
                        ++num;
                }

                if (num == 0)
                {
                    trigger.Reply("No Exceptions have been triggered so far.");
                }
                else
                {
                    trigger.Reply("Found {0} Exceptions:", (object) num);
                    for (int index = 0; index < ExceptionHandler.Exceptions.Count; ++index)
                    {
                        ExceptionInfo exception = ExceptionHandler.Exceptions[index];
                        if (remainder.Length == 0 || exception.ToString()
                                .IndexOf(remainder, StringComparison.InvariantCultureIgnoreCase) > -1)
                            trigger.Reply("{0}. {1}", (object) (index + 1), (object) exception);
                    }
                }
            }
        }

        public class ShowExceptionCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Show", "S");
                this.EnglishParamInfo = "[<index>]";
                this.EnglishDescription = "If no index is given, will show the last Exception.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint num = trigger.Text.NextUInt(uint.MaxValue);
                ExceptionInfo exceptionInfo = (long) num < (long) ExceptionHandler.Exceptions.Count
                    ? ExceptionHandler.Exceptions[(int) num]
                    : ExceptionHandler.Exceptions.LastOrDefault<ExceptionInfo>();
                if (exceptionInfo != null)
                {
                    trigger.Reply(exceptionInfo.ToString() + (exceptionInfo.Exception.StackTrace == null
                                      ? (object) " (No StackTrace available)"
                                      : (object) ""));
                    if (exceptionInfo.Exception.StackTrace == null)
                        return;
                    string stackTrace = exceptionInfo.Exception.StackTrace;
                    char[] chArray = new char[1] {'\n'};
                    foreach (string str in stackTrace.Split(chArray))
                        trigger.Reply(" " + str.Trim());
                }
                else
                    trigger.Reply("Invalid index specified.");
            }
        }
    }
}