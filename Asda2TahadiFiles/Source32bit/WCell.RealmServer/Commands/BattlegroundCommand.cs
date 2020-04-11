using WCell.RealmServer.Asda2BattleGround;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class BattlegroundCommand : RealmServerCommand
  {
    protected BattlegroundCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Battleground", "BG");
    }

    public class Start : SubCommand
    {
      protected override void Initialize()
      {
        Init(nameof(Start));
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int num1 = trigger.Text.NextInt(0);
        int num2 = trigger.Text.NextInt(0);
        Asda2Battleground asda2Battleground =
          Asda2BattlegroundMgr.AllBattleGrounds[(Asda2BattlegroundTown) num1][0];
        asda2Battleground.WarType =
          num2 == 0 ? Asda2BattlegroundType.Occupation : Asda2BattlegroundType.Deathmatch;
        asda2Battleground.Start();
        trigger.Reply("BG started.");
      }
    }

    public class Stop : SubCommand
    {
      protected override void Initialize()
      {
        Init(nameof(Stop));
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int num = trigger.Text.NextInt(0);
        Asda2BattlegroundMgr.AllBattleGrounds[(Asda2BattlegroundTown) num][0].Stop();
        trigger.Reply("BG stoped.");
      }
    }

    public class AddPoints : SubCommand
    {
      protected override void Initialize()
      {
        Init(nameof(AddPoints));
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int num = trigger.Text.NextInt(0);
        Character target = trigger.Args.Target as Character;
        if(target == null)
        {
          trigger.Reply("Wrong target.");
        }
        else
        {
          target.Asda2HonorPoints += num;
          trigger.Reply(target.Name + " get points. And now have " + target.Asda2HonorPoints);
        }
      }
    }

    public class Join : SubCommand
    {
      protected override void Initialize()
      {
        Init(nameof(Join));
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        int num = trigger.Text.NextInt(0);
        Character target = trigger.Args.Target as Character;
        if(target == null)
          trigger.Reply("Wrong target.");
        else if(target.CurrentBattleGround != null)
        {
          trigger.Reply("Already in BG.");
        }
        else
        {
          Asda2BattlegroundMgr.AllBattleGrounds[(Asda2BattlegroundTown) num][0].Join(target);
          trigger.Reply(target.Name + " joined BG.");
        }
      }
    }

    public class Leave : SubCommand
    {
      protected override void Initialize()
      {
        Init(nameof(Leave));
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Character target = trigger.Args.Target as Character;
        if(target == null)
          trigger.Reply("Wrong target.");
        else if(target.CurrentBattleGround == null)
        {
          trigger.Reply("Not in BG.");
        }
        else
        {
          target.CurrentBattleGround.Leave(target);
          trigger.Reply(target.Name + " leaved BG.");
        }
      }
    }
  }
}