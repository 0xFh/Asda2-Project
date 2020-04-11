using WCell.Constants;
using WCell.RealmServer.Lang;
using WCell.Util.Commands;
using WCell.Util.Lang;
using WCell.Util.Threading;

namespace WCell.RealmServer.Commands
{
    public static class Extensions
    {
        public static string Translate(this CmdTrigger<RealmServerCmdArgs> trigger, RealmLangKey key,
            params object[] args)
        {
            return RealmLocalizer.Instance.Translate(trigger.GetLocale(), key, args);
        }

        public static string Translate(this CmdTrigger<RealmServerCmdArgs> trigger, TranslatableItem item)
        {
            return RealmLocalizer.Instance.Translate(trigger.GetLocale(), (TranslatableItem<RealmLangKey>) item);
        }

        public static void Reply(this CmdTrigger<RealmServerCmdArgs> trigger, RealmLangKey key, params object[] args)
        {
            trigger.Reply(RealmLocalizer.Instance.Translate(trigger.GetLocale(), key, args));
        }

        public static void ReplyFormat(this CmdTrigger<RealmServerCmdArgs> trigger, RealmLangKey key,
            params object[] args)
        {
            trigger.ReplyFormat(RealmLocalizer.Instance.Translate(trigger.GetLocale(), key, args));
        }

        public static ClientLocale GetLocale(this CmdTrigger<RealmServerCmdArgs> trigger)
        {
            if (trigger == null || trigger.Args.User == null)
                return RealmServerConfiguration.DefaultLocale;
            return trigger.Args.User.Locale;
        }

        public static bool CheckPossibleContext(this CmdTrigger<RealmServerCmdArgs> trigger, object obj)
        {
            if (!(obj is IContextHandler) || ((IContextHandler) obj).IsInContext)
                return true;
            trigger.Reply("Object requires different context: {0}", obj);
            return false;
        }
    }
}