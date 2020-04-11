using System;
using System.IO;
using System.Xml.Serialization;

namespace WCell.Util.Lang
{
    [XmlRoot("Translation")]
    public class Translation<L, K> : XmlFile<Translation<L, K>> where L : IConvertible where K : IConvertible
    {
        private static readonly string Extension = ".xml";

        [XmlArray("Items")] [XmlArrayItem("Item")]
        public TranslatedItem<K>[] Items;

        public static string GetFile(string folder, L locale)
        {
            return Path.Combine(folder, locale.ToString() + Translation<L, K>.Extension);
        }

        public static Translation<L, K> Load(ILocalizer<L> localizer, L locale)
        {
            string file = Translation<L, K>.GetFile(localizer.Folder, locale);
            if (!File.Exists(file))
                return (Translation<L, K>) null;
            try
            {
                Translation<L, K> translation = XmlFile<Translation<L, K>>.Load(file);
                translation.Localizer = localizer;
                translation.Locale = locale;
                translation.SortItems();
                return translation;
            }
            catch (Exception ex)
            {
                throw new IOException("Unable to load Localization file " + file, ex);
            }
        }

        [XmlIgnore] public ILocalizer<L> Localizer { get; internal set; }

        [XmlIgnore] public L Locale { get; private set; }

        protected override void OnLoad()
        {
        }

        public string GetValue(K key)
        {
            return this.Items[key.ToInt32((IFormatProvider) null)]?.Value;
        }

        public string Translate(K key, params object[] args)
        {
            TranslatedItem<K> translatedItem = this.Items[key.ToInt32((IFormatProvider) null)];
            if (translatedItem != null)
                return string.Format(translatedItem.Value, args);
            return (string) null;
        }

        private void SortItems()
        {
            TranslatedItem<K>[] translatedItemArray = new TranslatedItem<K>[this.Localizer.MaxKeyValue + 1];
            if (this.Items != null)
            {
                foreach (TranslatedItem<K> translatedItem in this.Items)
                    translatedItemArray[translatedItem.Key.ToInt32((IFormatProvider) null)] = translatedItem;
            }

            this.Items = translatedItemArray;
        }
    }
}