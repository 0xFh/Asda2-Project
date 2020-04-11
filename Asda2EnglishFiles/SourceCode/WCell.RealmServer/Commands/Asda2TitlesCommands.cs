using System;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class Asda2TitlesCommands : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Title");
            this.EnglishDescription = "Provides commands for managing achivements";
        }

        public class AddTitleCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Add", "Create");
                this.EnglishParamInfo = "<achievement>";
                this.EnglishDescription =
                    "Adds the given achievement entry id to the player completed achievement list.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                if (!(trigger.Args.Target is Character))
                {
                    trigger.Reply("Wrong target.");
                }
                else
                {
                    if (trigger.Text.String.Contains("all"))
                    {
                        for (int index = 0; (Decimal) index < new Decimal(418); ++index)
                            (trigger.Args.Target as Character).GetTitle((Asda2TitleId) index);
                    }

                    uint num = trigger.Text.NextUInt(0U);
                    if ((Decimal) num >= new Decimal(418))
                    {
                        trigger.Reply("Wrong title id.");
                    }
                    else
                    {
                        (trigger.Args.Target as Character).GetTitle((Asda2TitleId) num);
                        trigger.Reply("Done.");
                    }
                }
            }

            public static bool AddAchievement(Character character, uint achievementEntryId)
            {
                character.Achievements.EarnAchievement(achievementEntryId);
                return true;
            }
        }

        public class DiscoverTitleCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Discover", "d");
                this.EnglishParamInfo = "<achievement>";
                this.EnglishDescription =
                    "Adds the given achievement entry id to the player completed achievement list.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint num = trigger.Text.NextUInt(0U);
                if ((Decimal) num >= new Decimal(418))
                    trigger.Reply("Wrong title id.");
                else if (!(trigger.Args.Target is Character))
                {
                    trigger.Reply("Wrong target.");
                }
                else
                {
                    (trigger.Args.Target as Character).DiscoverTitle((Asda2TitleId) num);
                    trigger.Reply("Done.");
                }
            }

            public static bool AddAchievement(Character character, uint achievementEntryId)
            {
                character.Achievements.EarnAchievement(achievementEntryId);
                return true;
            }
        }
    }
}