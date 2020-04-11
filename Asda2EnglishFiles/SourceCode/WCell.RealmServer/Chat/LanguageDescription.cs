using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Chat
{
    /// <summary>The Language description class</summary>
    public class LanguageDescription
    {
        public LanguageDescription(ChatLanguage lang, SpellId spell, SkillId skill)
        {
            this.Language = lang;
            this.SpellId = spell;
            this.SkillId = skill;
        }

        public ChatLanguage Language { get; set; }

        public SpellId SpellId { get; set; }

        public SkillId SkillId { get; set; }
    }
}