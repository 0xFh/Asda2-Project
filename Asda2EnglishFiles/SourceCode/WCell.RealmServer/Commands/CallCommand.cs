using System;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    /// <summary>Calls public methods.</summary>
    public class CallCommand : RealmServerCommand
    {
        private static CallCommand s_instance;

        public static CallCommand Instance
        {
            get { return CallCommand.s_instance; }
        }

        protected CallCommand()
        {
            CallCommand.s_instance = this;
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("Call", "C");
            this.EnglishParamInfo = "<some.Method> [param1, [param2 [...]]]";
            this.EnglishDescription = "Calls the given method with the given params";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            object obj = trigger.EvalNextOrTargetOrUser();
            if (obj is IContextHandler)
                ((IContextHandler) obj).ExecuteInContext((Action) (() => CallCommand.Call(trigger, obj, true)));
            else
                CallCommand.Call(trigger, obj, true);
        }

        public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            return this.Eval(trigger, false);
        }

        public object Eval(CmdTrigger<RealmServerCmdArgs> trigger, bool replySuccess)
        {
            object obj = trigger.EvalNextOrTargetOrUser();
            if (!trigger.CheckPossibleContext(obj))
                return (object) null;
            return CallCommand.Call(trigger, obj, replySuccess);
        }

        public static object Call(CmdTrigger<RealmServerCmdArgs> trigger, object obj, bool replySuccess = true)
        {
            if (trigger.Text.HasNext)
            {
                string accessName = trigger.Text.NextWord();
                string[] args = trigger.Text.Remainder.Split(new char[1]
                {
                    ','
                }, StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < args.Length; ++index)
                    args[index] = args[index].Trim();
                try
                {
                    object result;
                    if (ReflectUtil.Instance.CallMethod((IRoleGroup) trigger.Args.Character.Role, obj, ref accessName,
                        args, out result))
                    {
                        if (replySuccess)
                            trigger.Reply("Success! {0}",
                                result != null ? (object) ("- Return value: " + result) : (object) "");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    trigger.Reply("Exception thrown: " + (object) ex);
                }
            }

            trigger.Reply("Invalid method, parameter count or parameters.");
            return (object) null;
        }
    }
}