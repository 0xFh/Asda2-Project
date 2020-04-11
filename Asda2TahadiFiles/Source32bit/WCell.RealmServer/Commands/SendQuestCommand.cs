using WCell.Constants.Quests;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Quests;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class SendQuestCommand : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("QuestSend", "SendQuest");
      EnglishParamInfo = "";
      EnglishDescription = "Provides a set of debug commands to send quest packets dynamically.";
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }

    public class SendQuestInvalidCommand : SubCommand
    {
      protected SendQuestInvalidCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Invalid");
        EnglishParamInfo = "<reason>";
        EnglishDescription = "Sends the SendQuestInvalid packet with the given reason";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        QuestInvalidReason reason = trigger.Text.NextEnum(QuestInvalidReason.Ok);
        Character target = trigger.Args.Character.Target as Character;
        if(target != null)
          QuestHandler.SendQuestInvalid(target, reason);
        trigger.Reply("Done.");
      }
    }

    public class SendQuestPushResultCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("PushResult");
        EnglishParamInfo = "<reason>";
        EnglishDescription =
          "Sends the SendQuestPushResult packet with the given reason, currently sends from triggering char";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        QuestPushResponse qpr = trigger.Text.NextEnum(QuestPushResponse.Busy);
        Character target = trigger.Args.Character.Target as Character;
        if(target != null)
          QuestHandler.SendQuestPushResult(trigger.Args.Character, qpr, target);
        trigger.Reply("Done.");
      }
    }

    public class SendQuestGiverQuestDetailsCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("GiverQuestDetails");
        EnglishParamInfo = "<quest id>";
        EnglishDescription = "Sends the QuestGiverQuestDetails packet with the given quest id";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        QuestTemplate template = QuestMgr.GetTemplate(trigger.Text.NextUInt());
        if(template != null)
        {
          IQuestHolder character = trigger.Args.Character as IQuestHolder;
          Character target = (Character) trigger.Args.Character.Target;
          if(character != null)
            QuestHandler.SendDetails(character, template, target, false);
        }

        trigger.Reply("Done.");
      }
    }

    public class SendQuestGiverQuestQuestComplete : SubCommand
    {
      protected override void Initialize()
      {
        Init("GiverQuestComplete");
        EnglishParamInfo = "<quest id>";
        EnglishDescription = "Sends the QuestGiverQuestComplete packet with the given quest id";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        QuestTemplate template = QuestMgr.GetTemplate(trigger.Text.NextUInt());
        if(template != null)
        {
          Character target = trigger.Args.Character.Target as Character;
          if(target != null)
            QuestHandler.SendComplete(template, target);
        }

        trigger.Reply("Done.");
      }
    }
  }
}