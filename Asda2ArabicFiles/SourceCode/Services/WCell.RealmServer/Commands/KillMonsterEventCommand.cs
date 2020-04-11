using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Events.Asda2.Managers;
using WCell.RealmServer.Items;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class KillMonsterEventCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    protected override void Initialize()
    {
      Init("killmonsterevent", "kme");
    }

    public class StartCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("start");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        var monsterId = trigger.Text.NextInt();
        var itemId = trigger.Text.NextInt();
        var template = Asda2ItemMgr.GetTemplate(itemId);
        if (template == null)
        {
          trigger.Reply("Item not found");
          return;
        }
        if (KillMonsterEventManager.Started)
        {
          trigger.Reply("Kill Monster event is already started.");
          return;
        }
        KillMonsterEventManager.Start(monsterId, itemId);
        trigger.Reply("Ok,Kill Monster event stated. monsterId {0}, itemid is {1}.", monsterId, itemId);
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
        if (!KillMonsterEventManager.Started)
        {
          trigger.Reply("Kill Monster event is not started.");
          return;
        }
        KillMonsterEventManager.Stop();
        trigger.Reply("Kill Monster event stoped.");
      }
    }
  }
}