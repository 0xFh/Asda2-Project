using System;
using System.IO;

namespace WCell.Util.Lang
{
    /// <summary>
    /// Localizer class that converts the elements of the Locale and Key enums to array indices to look up strings with minimal
    /// overhead. Values defined in supplied Enum types must all be positive and not too big.
    /// <typeparam name="Locale">
    /// int-Enum that contains a set of usable Locales. For every Locale, one XML file is created in the supplied folder to contain
    /// all pairs of keys their string-representations.
    /// </typeparam>
    /// <typeparam name="Key">int-Enum that contains language keys which are mapped to string values in an XML file</typeparam>
    /// </summary>
    public class Localizer<Locale, Key> : ILocalizer<Locale> where Locale : IConvertible where Key : IConvertible
    {
        private static readonly int _MaxLocaleValue = Localizer<Locale, Key>.GetMaxEnumValue(typeof(Locale), 100);
        private static readonly int _MaxKeyValue = Localizer<Locale, Key>.GetMaxEnumValue(typeof(Key), 50000);
        public readonly Translation<Locale, Key>[] Translations;
        private Locale m_DefaultLocale;

        private static int GetMaxEnumValue(Type type, int maxx)
        {
            int num = 0;
            foreach (IConvertible convertible in Enum.GetValues(type))
            {
                int int32 = convertible.ToInt32((IFormatProvider) null);
                if (int32 > num)
                    num = int32;
                else if (int32 < 0)
                    throw new InvalidDataException("Cannot use Enum " + (object) type +
                                                   " because it defines negative values.");
            }

            if (num > maxx)
                throw new ArgumentException(string.Format("Enum {0} has members with too big values ({1} > {2})",
                    (object) type, (object) num, (object) maxx));
            return num;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLocale">The locale of the translation that is the most complete (usually English)</param>
        /// <param name="defaultLocale"></param>
        /// <param name="folder"></param>
        public Localizer(Locale baseLocale, Locale defaultLocale, string folder)
        {
            this.BaseLocale = baseLocale;
            this.m_DefaultLocale = defaultLocale;
            this.Folder = folder;
            this.Translations = new Translation<Locale, Key>[Localizer<Locale, Key>._MaxLocaleValue + 1];
        }

        /// <summary>
        /// The BaseLocale is the locale of the translation that is the most complete (usually English)
        /// </summary>
        public Locale BaseLocale { get; private set; }

        public Locale DefaultLocale
        {
            get { return this.m_DefaultLocale; }
            set
            {
                this.m_DefaultLocale = value;
                this.VerifyIntegrity();
            }
        }

        /// <summary>
        /// The BaseTranslation is the translation that is the most complete (usually English)
        /// </summary>
        public Translation<Locale, Key> BaseTranslation { get; set; }

        public Translation<Locale, Key> DefaultTranslation { get; set; }

        public string Folder { get; set; }

        public int MaxLocaleValue
        {
            get { return Localizer<Locale, Key>._MaxLocaleValue; }
        }

        public int MaxKeyValue
        {
            get { return Localizer<Locale, Key>._MaxKeyValue; }
        }

        /// <summary>Not Thread-Safe!</summary>
        public void LoadTranslations()
        {
            this.BaseTranslation = (Translation<Locale, Key>) null;
            this.DefaultTranslation = (Translation<Locale, Key>) null;
            foreach (Locale locale in Enum.GetValues(typeof(Locale)))
                this.LoadTranslations(locale);
            this.VerifyIntegrity();
        }

        /// <summary>Not Thread-Safe!</summary>
        public void Resync()
        {
            this.LoadTranslations();
        }

        /// <summary>
        /// Loads all translations for the given locale from the folder
        /// with the name of the locale.
        /// </summary>
        private void LoadTranslations(Locale locale)
        {
            Translation<Locale, Key> translation = Translation<Locale, Key>.Load((ILocalizer<Locale>) this, locale);
            if (translation == null)
                return;
            translation.Localizer = (ILocalizer<Locale>) this;
            this.Translations[locale.ToInt32((IFormatProvider) null)] = translation;
            if (this.BaseLocale.Equals((object) locale))
                this.BaseTranslation = translation;
            if (this.DefaultLocale.Equals((object) locale))
                this.DefaultTranslation = translation;
        }

        private void VerifyIntegrity()
        {
            if (this.BaseTranslation == null)
                throw new InvalidDataException("Could not find file for BaseLocale: " +
                                               Translation<Locale, Key>.GetFile(this.Folder, this.BaseLocale));
            if (this.DefaultTranslation == null)
                throw new InvalidDataException("Could not find file for DefaultLocale: " +
                                               Translation<Locale, Key>.GetFile(this.Folder, this.DefaultLocale));
            for (int index = 0; index < this.Translations.Length; ++index)
            {
                Translation<Locale, Key> translation = this.Translations[index];
                if (translation == null || translation.Locale.ToInt32((IFormatProvider) null) != index)
                    this.Translations[index] = this.DefaultTranslation;
            }
        }

        public Translation<Locale, Key> Translation(Locale locale)
        {
            return this.Translations[locale.ToInt32((IFormatProvider) null)];
        }

        public string Translate(TranslatableItem<Key> item)
        {
            return this.Translate(item.Key, item.Args);
        }

        public string Translate(Locale locale, TranslatableItem<Key> item)
        {
            return this.Translate(locale, item.Key, item.Args);
        }

        public string Translate(Key key, params object[] args)
        {
            return this.Translate(this.DefaultLocale, key, args);
        }

        public string Translate(Locale locale, Key key, params object[] args)
        {
            string str = (this.Translation(locale) ?? this.DefaultTranslation).Translate(key, args);
            if (string.IsNullOrEmpty(str))
            {
                str = this.DefaultTranslation.Translate(key, args);
                if (string.IsNullOrEmpty(str))
                {
                    str = this.BaseTranslation.Translate(key, args);
                    if (string.IsNullOrEmpty(str))
                        str = "No translation available for Key [" + (object) key + "]";
                }
            }

            return str;
        }

        /// <summary>
        /// Get all translations of the given key, in an array which is indexed by Locale.
        /// You can use the returned array to get a translated string, like this:
        /// <code>
        /// var translations = GetTranslations(key);
        /// var translation = translation[(int)mylocale];
        /// </code>
        /// </summary>
        public string[] GetTranslations(Key key)
        {
            string[] strArray = new string[this.MaxLocaleValue + 1];
            foreach (Translation<Locale, Key> translation in this.Translations)
            {
                if (translation != null)
                {
                    int int32 = translation.Locale.ToInt32((IFormatProvider) null);
                    strArray[int32] = translation.GetValue(key);
                }
            }

            return strArray;
        }
    }
}