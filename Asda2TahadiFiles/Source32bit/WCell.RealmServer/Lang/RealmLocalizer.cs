using System;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Util.Lang;

namespace WCell.RealmServer.Lang
{
  public class RealmLocalizer : Localizer<ClientLocale, RealmLangKey>
  {
    private static RealmLocalizer instance;

    public static RealmLocalizer Instance
    {
      get
      {
        if(!RealmServerConfiguration.Loaded)
          throw new InvalidOperationException("Must not use RealmLocalizer before Configuration was loaded.");
        try
        {
          instance = new RealmLocalizer(ClientLocale.English,
            RealmServerConfiguration.DefaultLocale, RealmServerConfiguration.LangDir);
          instance.LoadTranslations();
        }
        catch(Exception ex)
        {
          throw new InitializationException(ex, "Unable to load Localizations");
        }

        return instance;
      }
    }

    public RealmLocalizer(ClientLocale baseLocale, ClientLocale defaultLocale, string folder)
      : base(baseLocale, defaultLocale, folder)
    {
    }

    /// <summary>TODO: Localize (use TranslatableItem)</summary>
    public static string FormatTimeSecondsMinutes(int seconds)
    {
      string str;
      if(seconds < 60)
      {
        str = seconds + " seconds";
      }
      else
      {
        int num = seconds / 60;
        str = num.ToString() + (num == 1 ? " minute" : (object) " minutes");
        if(seconds % 60 != 0)
          str = str + " and " + seconds + (seconds == 1 ? " second" : (object) " seconds");
      }

      return str;
    }
  }
}