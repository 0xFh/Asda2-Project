using WCell.Util.Variables;

namespace WCell.RealmServer.Guilds
{
    public class GuildSkillTemplate
    {
        [NotVariable] public static GuildSkillTemplate[] Templates = new GuildSkillTemplate[10];

        public int[] ActivationCosts { get; set; }

        public int[] LearnCosts { get; set; }

        public int[] MaitenceCosts { get; set; }

        public int[] BonusValuses { get; set; }

        public int MaxLevel { get; set; }
    }
}