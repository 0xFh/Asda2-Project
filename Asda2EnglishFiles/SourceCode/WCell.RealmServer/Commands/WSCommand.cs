using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class WSCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("WorldState", "WS");
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }

        public class InitWSCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Init", "I");
                this.EnglishParamInfo = "<area> [<state> <value>[ <state2> <value2> ...]]";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Character target = (Character) trigger.Args.Target;
                int num = trigger.Text.NextInt(-1);
                if (num < 0)
                {
                    trigger.Reply("No area given");
                }
                else
                {
                    List<WorldState> worldStateList = new List<WorldState>();
                    while (trigger.Text.HasNext)
                    {
                        if (trigger.Text.NextEnum<WorldStateId>(WorldStateId.End) == WorldStateId.End)
                        {
                            trigger.Reply("Found invalid state.");
                            return;
                        }

                        trigger.Text.NextInt();
                    }

                    WorldStateHandler.SendInitWorldStates((IPacketReceiver) target, target.Map.Id, target.ZoneId,
                        (uint) num, worldStateList.ToArray());
                }
            }
        }
    }
}