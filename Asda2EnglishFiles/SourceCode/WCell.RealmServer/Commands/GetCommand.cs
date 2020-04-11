using System;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    /// <summary>
    /// General set-command. Can be used to register further set-handlers
    /// </summary>
    public class GetCommand : RealmServerCommand
    {
        protected GetCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Get", "G");
            this.EnglishParamInfo = "<prop.subprop.otherprop.etc>";
            this.EnglishDescription = "Gets the value of the given prop";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            object obj = trigger.EvalNextOrTargetOrUser();
            if (obj is IContextHandler)
                ((IContextHandler) obj).ExecuteInContext((Action) (() => GetCommand.GetAndReply(trigger, obj)));
            else
                GetCommand.GetAndReply(trigger, obj);
        }

        public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            return GetCommand.Eval(trigger, trigger.EvalNextOrTargetOrUser());
        }

        public static void GetAndReply(CmdTrigger<RealmServerCmdArgs> trigger, object target)
        {
            if (trigger.Text.HasNext)
            {
                string accessName = trigger.Text.NextWord();
                object val;
                if (ReflectUtil.Instance.GetPropValue((IRoleGroup) trigger.Args.Role, target, ref accessName, out val))
                    trigger.Reply("{0} = {1}", (object) accessName,
                        val != null ? (object) Utility.GetStringRepresentation(val) : (object) "<null>");
                else
                    trigger.Reply("Invalid property.");
            }
            else
            {
                trigger.Reply("Invalid arguments:");
                trigger.Reply(trigger.Command.CreateInfo(trigger));
            }
        }

        public static object Eval(CmdTrigger<RealmServerCmdArgs> trigger, object target)
        {
            if (!trigger.CheckPossibleContext(target))
                return (object) null;
            string accessName = trigger.Text.NextWord();
            object obj;
            ReflectUtil.Instance.GetPropValue((IRoleGroup) trigger.Args.Role, target, ref accessName, out obj);
            return obj;
        }
    }
}