using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Util;

namespace WCell.RealmServer.Chat
{
    /// <summary>Chat System class</summary>
    public static class LanguageHandler
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();
        public static readonly LanguageDescription[] ByLang = new LanguageDescription[36];
        public static readonly LanguageDescription[] ByRace = new LanguageDescription[16];

        static LanguageHandler()
        {
            LanguageHandler.ByRace[2] = LanguageHandler.ByLang[1];
            LanguageHandler.ByRace[1] = LanguageHandler.ByLang[7];
            LanguageHandler.ByRace[3] = LanguageHandler.ByLang[7];
            LanguageHandler.ByRace[4] = LanguageHandler.ByLang[2];
            LanguageHandler.ByRace[5] = LanguageHandler.ByLang[33];
            LanguageHandler.ByRace[6] = LanguageHandler.ByLang[3];
            LanguageHandler.ByRace[7] = LanguageHandler.ByLang[13];
            LanguageHandler.ByRace[8] = LanguageHandler.ByLang[14];
            LanguageHandler.ByRace[9] = LanguageHandler.ByLang[1];
            LanguageHandler.ByRace[10] = LanguageHandler.ByLang[10];
            LanguageHandler.ByRace[11] = LanguageHandler.ByLang[35];
            LanguageHandler.ByRace[12] = LanguageHandler.ByLang[1];
            LanguageHandler.ByRace[13] = LanguageHandler.ByLang[1];
            LanguageHandler.ByRace[14] = LanguageHandler.ByLang[8];
            LanguageHandler.ByRace[15] = LanguageHandler.ByLang[33];
        }

        /// <summary>Get language description by Type</summary>
        /// <param name="language">the Language type</param>
        /// <returns></returns>
        public static LanguageDescription GetLanguageDescByType(ChatLanguage language)
        {
            return LanguageHandler.ByLang.Get<LanguageDescription>((uint) language);
        }

        /// <summary>Get language description by Spell Id</summary>
        /// <param name="spell">spell type</param>
        /// <returns></returns>
        public static LanguageDescription GetLanguageDescBySpellId(SpellId spell)
        {
            for (int index = 0; index < LanguageHandler.ByLang.Length; ++index)
            {
                if (LanguageHandler.ByLang[index] != null && LanguageHandler.ByLang[index].SpellId == spell)
                    return LanguageHandler.ByLang[index];
            }

            return (LanguageDescription) null;
        }

        /// <summary>Get language description by Spell Id</summary>
        /// <returns></returns>
        public static LanguageDescription GetLanguageDescByRace(RaceId race)
        {
            return LanguageHandler.ByRace.Get<LanguageDescription>((uint) race);
        }

        /// <summary>Get language description by Skill Type</summary>
        /// <param name="skillLanguage">Skill type</param>
        /// <returns></returns>
        public static LanguageDescription GetLanguageDescBySkillType(SkillId skillLanguage)
        {
            for (int index = 0; index < LanguageHandler.ByLang.Length; ++index)
            {
                if (LanguageHandler.ByLang[index] != null && LanguageHandler.ByLang[index].SkillId == skillLanguage)
                    return LanguageHandler.ByLang[index];
            }

            return (LanguageDescription) null;
        }
    }
}