using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class MoneyCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Money");
      EnglishDescription = "Used for manipulation money";
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }

    public class AddCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Add", "a");
        EnglishDescription = "Adds money";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        uint amount = trigger.Text.NextUInt(1U);
        Character target = trigger.Args.Target as Character;
        if(target != null)
        {
          target.AddMoney(amount);
          target.SendMoneyUpdate();
          trigger.Reply("Done.");
        }
        else
          trigger.Reply("Wrong target.");
      }
    }

    public class SubstractCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Substract", "Sub", "S");
        EnglishDescription = "Substacts money";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        uint amount = trigger.Text.NextUInt(1U);
        Character target = trigger.Args.Target as Character;
        if(target != null)
        {
          target.SubtractMoney(amount);
          target.SendMoneyUpdate();
          trigger.Reply("Done.");
        }
        else
          trigger.Reply("Wrong target.");
      }
    }

    public class SetCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Set");
        EnglishDescription = "Sets money";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        uint amount = trigger.Text.NextUInt(1U);
        Character target = trigger.Args.Target as Character;
        if(target != null)
        {
          target.AddMoney(amount);
          target.SubtractMoney(1U);
          target.SendMoneyUpdate();
          trigger.Reply("Done.");
        }
        else
          trigger.Reply("Wrong target.");
      }
    }
  }
}