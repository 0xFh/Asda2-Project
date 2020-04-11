using System;
using System.Reflection;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class SetCommand : RealmServerCommand
    {
        protected SetCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Set", "S");
            this.EnglishParamInfo = "<prop.subprop.otherprop.etc> <value>";
            this.EnglishDescription = "Sets the value of the given prop";
        }

        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            SetCommand.Set(trigger, trigger.EvalNextOrTargetOrUser());
        }

        /// <summary>Whether we have a valid target.</summary>
        public static bool Set(CmdTrigger<RealmServerCmdArgs> trigger, object target)
        {
            if (target == null)
                trigger.Reply("Nothing selected.");
            else if (trigger.Text.HasNext)
            {
                object propHolder;
                MemberInfo prop = ReflectUtil.Instance.GetProp((IRoleGroup) trigger.Args.Role, target,
                    trigger.Text.NextWord(), target.GetType(), out propHolder);
                SetCommand.SetProp(propHolder, prop, trigger);
            }
            else
            {
                trigger.Reply("Invalid arguments.");
                trigger.Reply(trigger.Command.CreateInfo(trigger));
            }

            return true;
        }

        public static void SetProp(object propHolder, MemberInfo prop, CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (prop != (MemberInfo) null && ReflectUtil.Instance.CanWrite(prop, (IRoleGroup) trigger.Args.Role))
            {
                string str1 = trigger.Text.Remainder.Trim();
                if (str1.Length == 0)
                {
                    trigger.Reply("No expression given");
                }
                else
                {
                    Type variableType = prop.GetVariableType();
                    object obj = (object) null;
                    if (variableType.IsInteger())
                    {
                        object error = (object) null;
                        long val = 0;
                        if (!StringParser.Eval(variableType, ref val, str1, ref error, false))
                        {
                            trigger.Reply("Invalid expression: " + error);
                            return;
                        }

                        obj = SetCommand.ConvertActualType(val, variableType);
                    }
                    else if (!StringParser.Parse(str1, variableType, ref obj))
                    {
                        trigger.Reply("Could not change value \"{0}\" to Type: {1}", (object) str1,
                            (object) variableType);
                        return;
                    }

                    prop.SetUnindexedValue(propHolder, obj);
                    string str2 = !variableType.IsEnum ? obj.ToString() : Enum.Format(variableType, obj, "g");
                    trigger.Reply("{0} is now {1}.", (object) prop.Name, (object) str2);
                }
            }
            else
                trigger.Reply("Invalid field.");
        }

        public static object ConvertActualType(long val, Type type)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            return Convert.ChangeType((object) val, type);
        }
    }
}