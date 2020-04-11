using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Events.Asda2;
using WCell.RealmServer.Events.Asda2.Managers;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class GuessEventCommand : RealmServerCommand
  {
    public override RoleStatus RequiredStatusDefault
    {
      get { return RoleStatus.EventManager; }
    }

    protected override void Initialize()
    {
      Init("guessword", "ge");
    }

    public class StartCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("start");
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        var word = trigger.Text.NextWord();
        if (word.Length < 3)
        {
          trigger.Reply("Minimum length of secret word is 3");
          return;
        }
        if (GuessEventManager.Started)
        {
          trigger.Reply("Guess word event is already started.");
          return;
        }
        var percision = trigger.Text.NextInt(100);
        GuessEventManager.Start(word, percision, trigger.Args.Character.Name);
        trigger.Reply("Ok, guess word event started. Word is {0}, percision is {1}.", word, percision);
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
        if (!GuessEventManager.Started)
        {
          trigger.Reply("Guess word event is not started.");
          return;
        }
        GuessEventManager.Stop();
        trigger.Reply("Guess word event stoped.");
      }
    }
  }
}