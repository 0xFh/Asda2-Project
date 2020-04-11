using WCell.Constants.Updates;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class RealmIPCCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("IPC");
            this.EnglishParamInfo = "[0/1]";
            this.EnglishDescription = "Toggles the IPC-device that connects Realm- and Auth-Server.";
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

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            ServerApp<WCell.RealmServer.RealmServer>.Instance.AuthClient.IsRunning = !trigger.Text.HasNext
                ? !ServerApp<WCell.RealmServer.RealmServer>.Instance.AuthClient.IsRunning
                : trigger.Text.NextBool();
            trigger.Reply("Done.");
        }
    }
}