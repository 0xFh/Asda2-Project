using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SendPacketCommand : RealmServerCommand
    {
        protected SendPacketCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("SendPacket", "SendP");
            this.EnglishParamInfo = "<packet> <args>";
            this.EnglishDescription = "Sends the given packet with corresponding args to the client";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }

        public class SendSpellDamageLogCommand : RealmServerCommand.SubCommand
        {
            protected SendSpellDamageLogCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("SpellLog", "SLog");
                this.EnglishParamInfo =
                    "[<unkBool> [<flags> [<spell> [<damage> [<overkill> [<schools> [<absorbed> [<resisted> [<blocked>]]]]]]]]]";
                this.EnglishDescription =
                    "Sends a SpellMissLog packet to everyone in the area where you are the caster and everyone within 10y radius is the targets.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
            }
        }

        public class SendBGErrorCommand : RealmServerCommand.SubCommand
        {
            protected SendBGErrorCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("BGError", "BGErr");
                this.EnglishParamInfo = "<err> [<bg>]";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                BattlegroundJoinError err = trigger.Text.NextEnum<BattlegroundJoinError>(BattlegroundJoinError.None);
                BattlegroundHandler.SendBattlegroundError((IPacketReceiver) trigger.Args.Character, err);
            }
        }

        public class SendTotemCreated : RealmServerCommand.SubCommand
        {
            protected SendTotemCreated()
            {
            }

            protected override void Initialize()
            {
                this.Init("TotemCreated", "TC");
                this.EnglishParamInfo = "[<spellId>]";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
            }
        }
    }
}