using WCell.Constants.Achievements;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	/// <summary>
	/// TODO: Localize
	/// </summary>
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
                EnglishDescription = "Adds the given achievement entry id to the player completed achievement list.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var achievementId = trigger.Text.NextUInt(0u);
                var achivementEntry = AchievementMgr.GetAchievementEntry(achievementId);
                if (achivementEntry != null)
                {
                    AddAchievement((Character)trigger.Args.Target, achievementId);
                    trigger.Reply("Achievement \"{0}\" added sucessfully.", achivementEntry.Names);
                }
                else
                {
                    trigger.Reply("Invalid AchievementId");
                    return;
                }
            }

            public static bool AddAchievement(Character character, uint achievementEntryId)
            {
                character.Achievements.EarnAchievement(achievementEntryId);
                return true;
            }
        }
    }
    public class Asda2TitlesCommands : RealmServerCommand
    {
        protected override void Initialize()
        {
            Init("Title");
            EnglishDescription = "Provides commands for managing achivements";
        }

        public class AddTitleCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Add", "Create");
                EnglishParamInfo = "<achievement>";
                EnglishDescription = "Adds the given achievement entry id to the player completed achievement list.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (!(trigger.Args.Target is Character))
                {
                    trigger.Reply("Wrong target.");
                    return;
                }
                if (trigger.Text.String.Contains("all"))
                {
                    for (int i = 0; i < (decimal)Asda2TitleId.End; i++)
                    {
                        (trigger.Args.Target as Character).GainTitle((Asda2TitleId)i);
                    }
                }
                var achievementId = trigger.Text.NextUInt(0u);
                if(achievementId >= (decimal) Asda2TitleId.End)
                {
                    trigger.Reply("Wrong title id.");
                    return;
                }

                (trigger.Args.Target as Character).GainTitle((Asda2TitleId) achievementId);
                trigger.Reply("Done.");
            }

            public static bool AddAchievement(Character character, uint achievementEntryId)
            {
                character.Achievements.EarnAchievement(achievementEntryId);
                return true;
            }
        }
        public class DiscoverTitleCommand : SubCommand
        {
            protected override void Initialize()
            {
                Init("Discover", "d");
                EnglishParamInfo = "<achievement>";
                EnglishDescription = "Adds the given achievement entry id to the player completed achievement list.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                var achievementId = trigger.Text.NextUInt(0u);
                if (achievementId >= (decimal)Asda2TitleId.End)
                {
                    trigger.Reply("Wrong title id.");
                    return;
                }
                if (!(trigger.Args.Target is Character))
                {
                    trigger.Reply("Wrong target.");
                    return;
                }
                (trigger.Args.Target as Character).DiscoverTitle((Asda2TitleId)achievementId);
                trigger.Reply("Done.");
            }

            public static bool AddAchievement(Character character, uint achievementEntryId)
            {
                character.Achievements.EarnAchievement(achievementEntryId);
                return true;
            }
        }
    }

}
