using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AuthRemoteCommand : RealmServerCommand
    {
        public override RoleStatus RequiredStatusDefault
        {
            get { return RoleStatus.Admin; }
        }

        protected override void Initialize()
        {
            this.Init("AuthRemote", "Auth");
            this.EnglishParamInfo = "<Command <args>>";
            this.EnglishDescription = "Executes a command on the AuthServer.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (ServerApp<WCell.RealmServer.RealmServer>.Instance.AuthClient.Channel == null ||
                !ServerApp<WCell.RealmServer.RealmServer>.Instance.AuthClient.IsConnected)
            {
                trigger.Reply("Connection to AuthServer is currently not established.");
            }
            else
            {
                BufferedCommandResponse bufferedCommandResponse =
                    ServerApp<WCell.RealmServer.RealmServer>.Instance.AuthClient.Channel.ExecuteCommand(trigger.Text
                        .Remainder);
                if (bufferedCommandResponse != null)
                {
                    if (bufferedCommandResponse.Replies.Count > 0)
                    {
                        foreach (string reply in bufferedCommandResponse.Replies)
                            trigger.Reply(reply);
                    }
                    else
                        trigger.Reply("Done.");
                }
                else
                    trigger.Reply("Failed to execute remote-command.");
            }
        }
    }
}