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
      ByRace[2] = ByLang[1];
      ByRace[1] = ByLang[7];
      ByRace[3] = ByLang[7];
      ByRace[4] = ByLang[2];
      ByRace[5] = ByLang[33];
      ByRace[6] = ByLang[3];
      ByRace[7] = ByLang[13];
      ByRace[8] = ByLang[14];
      ByRace[9] = ByLang[1];
      ByRace[10] = ByLang[10];
      ByRace[11] = ByLang[35];
      ByRace[12] = ByLang[1];
      ByRace[13] = ByLang[1];
      ByRace[14] = ByLang[8];
      ByRace[15] = ByLang[33];
    }

    /// <summary>Get language description by Type</summary>
    /// <param name="language">the Language type</param>
    /// <returns></returns>
    public static LanguageDescription GetLanguageDescByType(ChatLanguage language)
    {
      return ByLang.Get((uint) language);
    }

    /// <summary>Get language description by Spell Id</summary>
    /// <param name="spell">spell type</param>
    /// <returns></returns>
    public static LanguageDescription GetLanguageDescBySpellId(SpellId spell)
    {
      for(int index = 0; index < ByLang.Length; ++index)
      {
        if(ByLang[index] != null && ByLang[index].SpellId == spell)
          return ByLang[index];
      }

      return null;
    }

    /// <summary>Get language description by Spell Id</summary>
    /// <returns></returns>
    public static LanguageDescription GetLanguageDescByRace(RaceId race)
    {
      return ByRace.Get((uint) race);
    }

    /// <summary>Get language description by Skill Type</summary>
    /// <param name="skillLanguage">Skill type</param>
    /// <returns></returns>
    public static LanguageDescription GetLanguageDescBySkillType(SkillId skillLanguage)
    {
      for(int index = 0; index < ByLang.Length; ++index)
      {
        if(ByLang[index] != null && ByLang[index].SkillId == skillLanguage)
          return ByLang[index];
      }

      return null;
    }
  }
}