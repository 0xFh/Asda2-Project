using WCell.Constants;
using WCell.Util.Lang;

namespace WCell.RealmServer.Lang
{
    public class TranslatableItem : TranslatableItem<RealmLangKey>
    {
        public TranslatableItem(RealmLangKey key, params object[] args)
            : base(key, args)
        {
        }

        public string Translate(ClientLocale locale)
        {
            return RealmLocalizer.Instance.Translate(locale, (TranslatableItem<RealmLangKey>) this);
        }

        public string TranslateDefault()
        {
            return this.Translate(RealmServerConfiguration.DefaultLocale);
        }
    }
}