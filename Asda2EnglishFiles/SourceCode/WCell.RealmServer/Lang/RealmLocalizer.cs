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
                if (!RealmServerConfiguration.Loaded)
                    throw new InvalidOperationException("Must not use RealmLocalizer before Configuration was loaded.");
                try
                {
                    RealmLocalizer.instance = new RealmLocalizer(ClientLocale.English,
                        RealmServerConfiguration.DefaultLocale, RealmServerConfiguration.LangDir);
                    RealmLocalizer.instance.LoadTranslations();
                }
                catch (Exception ex)
                {
                    throw new InitializationException(ex, "Unable to load Localizations", new object[0]);
                }

                return RealmLocalizer.instance;
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
            if (seconds < 60)
            {
                str = seconds.ToString() + " seconds";
            }
            else
            {
                int num = seconds / 60;
                str = num.ToString() + (num == 1 ? (object) " minute" : (object) " minutes");
                if (seconds % 60 != 0)
                    str = str + " and " + (object) seconds + (seconds == 1 ? (object) " second" : (object) " seconds");
            }

            return str;
        }
    }
}