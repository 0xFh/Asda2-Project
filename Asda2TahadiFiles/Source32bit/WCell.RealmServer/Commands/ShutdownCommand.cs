using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class ShutdownCommand : RealmServerCommand
  {
    protected ShutdownCommand()
    {
    }

    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Admin; }
    }

    protected override void Initialize()
    {
      Init("Shutdown");
      EnglishParamInfo = "[<delay before shutdown in seconds>]";
      EnglishDescription =
        "Shuts down the server after the given delay (default = 10s). Once started, calling this command again will cancel the shutdown-sequence.";
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      if(ServerApp<RealmServer>.IsPreparingShutdown)
        ServerApp<RealmServer>.Instance.CancelShutdown();
      else
        ServerApp<RealmServer>.Instance.ShutdownIn(trigger.Text.NextUInt(10U) * 1000U);
    }
  }
}