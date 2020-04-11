using System;
using System.Reflection;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    public class ModPropCommand : RealmServerCommand
    {
        protected ModPropCommand()
        {
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("Mod", "M");
            this.EnglishParamInfo = "<prop> <op> <value> [<op> <value> [<op> <value>...]]";
            this.EnglishDescription = "Modifies the given prop by applying it with an evaluated expression";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            object obj = trigger.EvalNextOrTargetOrUser();
            if (obj is IContextHandler)
                ((IContextHandler) obj).ExecuteInContext((Action) (() => ModPropCommand.ModProp(trigger, obj)));
            else
                ModPropCommand.ModProp(trigger, obj);
        }

        public static void ModProp(CmdTrigger<RealmServerCmdArgs> trigger, object target)
        {
            string name = trigger.Text.NextWord();
            object propHolder;
            MemberInfo prop = ReflectUtil.Instance.GetProp((IRoleGroup) trigger.Args.Role, target, name,
                target.GetType(), out propHolder);
            ModPropCommand.ModProp(propHolder, prop, trigger);
        }

        public static void ModProp(object propHolder, MemberInfo prop, CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (prop != (MemberInfo) null && ReflectUtil.Instance.CanWrite(prop, (IRoleGroup) trigger.Args.Role))
            {
                Type variableType = prop.GetVariableType();
                if (!variableType.IsInteger())
                {
                    trigger.Reply("Can only modify Integer-values.");
                }
                else
                {
                    string expr = trigger.Text.Remainder.Trim();
                    if (expr.Length == 0)
                    {
                        trigger.Reply("No expression given");
                    }
                    else
                    {
                        object error = (object) null;
                        long int64 = Convert.ToInt64(prop.GetUnindexedValue(propHolder));
                        if (!StringParser.Eval(variableType, ref int64, expr, ref error, true))
                        {
                            trigger.Reply("Invalid expression: " + error);
                        }
                        else
                        {
                            object obj = ModPropCommand.TryParseEnum(int64, variableType);
                            prop.SetUnindexedValue(propHolder, obj);
                            string str = !variableType.IsEnum ? int64.ToString() : Enum.Format(variableType, obj, "g");
                            trigger.Reply("Success: {0} is now {1}.", (object) prop.Name, (object) str);
                        }
                    }
                }
            }
            else
                trigger.Reply("Invalid field.");
        }

        public static object TryParseEnum(long val, Type type)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            return Convert.ChangeType((object) val, type);
        }
    }
}