using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class AICommand : RealmServerCommand
  {
    protected AICommand()
    {
    }

    protected override void Initialize()
    {
      Init("AI");
      EnglishDescription = "Provides Commands to interact with AI.";
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Unit; }
    }

    public class AIActiveCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Active");
        EnglishParamInfo = "<1/0>";
        EnglishDescription = "Activates/Deactivates AI of target.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Unit target = trigger.Args.Target;
        if(target == trigger.Args.Character)
          target = trigger.Args.Character.Target;
        if(!(target is NPC))
        {
          trigger.Reply("Must target NPC.");
        }
        else
        {
          IBrain brain = target.Brain;
          if(brain == null)
          {
            trigger.Reply(target.Name + " doesn't have a brain.");
          }
          else
          {
            bool flag = !trigger.Text.HasNext ? !brain.IsRunning : trigger.Text.NextBool();
            brain.IsRunning = flag;
            trigger.Reply(target.Name + "'s Brain is now: " + (flag ? "Activated" : "Deactivated"));
          }
        }
      }
    }

    public class AIMoveToMeCommand : SubCommand
    {
      protected AIMoveToMeCommand()
      {
      }

      protected override void Initialize()
      {
        Init("MoveToMe", "Come");
        EnglishDescription = "Moves a target NPC to the character.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Unit target = trigger.Args.Target;
        if(target == trigger.Args.Character)
          target = trigger.Args.Character.Target;
        if(!(target is NPC))
          trigger.Reply("Can only command NPCs.");
        else
          target.MoveToThenIdle(trigger.Args.Character);
      }
    }

    public class AIFollowCommand : SubCommand
    {
      protected AIFollowCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Follow");
        EnglishDescription = "Moves a target NPC to the character.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        Unit target = trigger.Args.Target;
        if(target == trigger.Args.Character)
          target = trigger.Args.Character.Target;
        if(!(target is NPC))
          trigger.Reply("Can only command NPCs.");
        else
          target.Follow(trigger.Args.Character);
      }
    }
  }
}