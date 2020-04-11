using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  /// <summary>TODO: Localize</summary>
  public class AchievementCommands : RealmServerCommand
  {
    protected override void Initialize()
    {
      Init("Achievement");
      EnglishDescription = "Provides commands for managing achivements";
    }

    public class AddAchievementCommand : SubCommand
    {
      protected override void Initialize()
      {
        Init("Add", "Create");
        EnglishParamInfo = "<achievement>";
        EnglishDescription =
          "Adds the given achievement entry id to the player completed achievement list.";
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        uint achievementEntryId = trigger.Text.NextUInt(0U);
        AchievementEntry achievementEntry = AchievementMgr.GetAchievementEntry(achievementEntryId);
        if(achievementEntry != null)
        {
          AddAchievement((Character) trigger.Args.Target,
            achievementEntryId);
          trigger.Reply("Achievement \"{0}\" added sucessfully.", achievementEntry.Names);
        }
        else
          trigger.Reply("Invalid AchievementId");
      }

      public static bool AddAchievement(Character character, uint achievementEntryId)
      {
        character.Achievements.EarnAchievement(achievementEntryId);
        return true;
      }
    }
  }
}