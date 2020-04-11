using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class POICommand : RealmServerCommand
    {
        protected POICommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("POI");
            this.EnglishParamInfo = "[-[d][f] <Data> <Flags>] <x> <y> [<name>]";
            this.EnglishDescription =
                "Sends a Point of interest entry to the target (shows up on the minimap while not too close).";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            string str = trigger.Text.NextModifiers();
            int Data = !str.Contains("d") ? 0 : trigger.Text.NextInt(0);
            GossipPOIFlags Flags = !str.Contains("f")
                ? GossipPOIFlags.Six
                : trigger.Text.NextEnum<GossipPOIFlags>(GossipPOIFlags.None);
            float X = trigger.Text.NextFloat();
            float Y = trigger.Text.NextFloat();
            int Icon = 7;
            string Name = trigger.Text.Remainder;
            if (Name.Length == 0)
                Name = trigger.Args.User.Name;
            GossipHandler.SendGossipPOI((IPacketReceiver) (trigger.Args.Target as Character), Flags, X, Y, Data, Icon,
                Name);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }
    }
}