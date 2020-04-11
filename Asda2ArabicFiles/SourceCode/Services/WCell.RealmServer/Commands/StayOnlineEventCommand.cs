using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Events.Asda2.Managers;
using WCell.RealmServer.Items;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class StayOnlineEventCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    protected override void Initialize()
    {
      Init("stayonlineevent", "soe");
    }

    public class StartCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("start");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        var interval = trigger.Text.NextInt();
        var itemId = trigger.Text.NextInt();
        var template = Asda2ItemMgr.GetTemplate(itemId);
        if (interval < 1)
        {
          trigger.Reply("Min inteval is 1");
          return;
        }
        if (template == null)
        {
          trigger.Reply("Item not found");
          return;
        }
        if (StayOnlineEventManager.Started)
        {
          trigger.Reply("Stay online event is already started.");
          return;
        }
        StayOnlineEventManager.Start(interval, itemId);
        trigger.Reply("Ok, stay online event stated. interval {0}, itemid is {1}.", interval, itemId);
      }
    }

    public class StopCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("stop");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        if (!StayOnlineEventManager.Started)
        {
          trigger.Reply("Stay online event is not started.");
          return;
        }
        StayOnlineEventManager.Stop();
        trigger.Reply("Stay online event stoped.");
      }
    }
  }
}