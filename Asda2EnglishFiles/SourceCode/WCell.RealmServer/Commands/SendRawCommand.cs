using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SendRawCommand : RealmServerCommand
    {
        protected SendRawCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("SendRaw");
            this.EnglishParamInfo = "<opcode> [<int value1> [<int value2> ...]]";
            this.EnglishDescription = "Sends a raw packet to the client";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            RealmServerOpCode packetOpCode = trigger.Text.NextEnum<RealmServerOpCode>(RealmServerOpCode.Unknown);
            if (packetOpCode == RealmServerOpCode.Unknown)
            {
                trigger.Reply("Invalid OpCode.");
            }
            else
            {
                using (RealmPacketOut packet = new RealmPacketOut(packetOpCode))
                {
                    while (trigger.Text.HasNext)
                    {
                        int num = trigger.Text.NextInt();
                        packet.Write(num);
                    }

                    ((Character) trigger.Args.Target).Client.Send(packet, false);
                }
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}