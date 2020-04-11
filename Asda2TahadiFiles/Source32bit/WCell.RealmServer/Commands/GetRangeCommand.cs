using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class GetRangeCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.Admin; }
    }

    protected override void Initialize()
    {
      Init("GetRange", "gr");
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      if(trigger.Args.Target == null)
        trigger.Reply("Wrong target.");
      else
        trigger.Reply(string.Format("Range {0}.",
          trigger.Args.Target.GetDistance(trigger.Args.Character)));
    }
  }
}