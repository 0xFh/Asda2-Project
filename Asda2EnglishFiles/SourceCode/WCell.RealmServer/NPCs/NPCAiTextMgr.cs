using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.RealmServer.Lang;
using WCell.Util.Variables;

namespace WCell.RealmServer.NPCs
{
    public static class NPCAiTextMgr
    {
        [NotVariable] internal static readonly Dictionary<int, NPCAiText> Entries = new Dictionary<int, NPCAiText>();

        public static void InitAITexts()
        {
            ContentMgr.Load<NPCAiText>();
        }

        public static IEnumerable<NPCAiText> AllEntries
        {
            get { return (IEnumerable<NPCAiText>) NPCAiTextMgr.Entries.Values; }
        }

        /// <summary>Select entries by mob's ID</summary>
        /// <param name="id">Mob's ID</param>
        public static NPCAiText[] GetEntry(uint id)
        {
            return NPCAiTextMgr.Entries
                .Where<KeyValuePair<int, NPCAiText>>(
                    (Func<KeyValuePair<int, NPCAiText>, bool>) (entry => (long) entry.Value.GetMobId() == (long) id))
                .Select<KeyValuePair<int, NPCAiText>, NPCAiText>(
                    (Func<KeyValuePair<int, NPCAiText>, NPCAiText>) (entry => entry.Value)).ToArray<NPCAiText>();
        }

        /// <summary>
        /// Select the first Text whose english version starts with the given string
        /// </summary>
        /// <param name="englishPrefix">String preposition</param>
        public static NPCAiText GetFirstTextByEnglishPrefix(string englishPrefix, bool warnIfNotFound = true)
        {
            NPCAiText npcAiText = NPCAiTextMgr.Entries.Values.FirstOrDefault<NPCAiText>(
                (Func<NPCAiText, bool>) (entry =>
                    entry.Texts.Localize(ClientLocale.English).StartsWith(englishPrefix)));
            if (npcAiText == null && warnIfNotFound)
                LogManager.GetCurrentClassLogger().Warn("Could not find AIText which starts with: {0}", englishPrefix);
            return npcAiText;
        }

        /// <summary>
        /// Select entries by preposition of yelled text (on any localization)
        /// </summary>
        /// <param name="str">String preposition</param>
        public static NPCAiText[] GetEntries(string str)
        {
            return NPCAiTextMgr.Entries
                .Where<KeyValuePair<int, NPCAiText>>((Func<KeyValuePair<int, NPCAiText>, bool>) (entry =>
                    ((IEnumerable<string>) entry.Value.Texts).Any<string>(
                        (Func<string, bool>) (text => text.StartsWith(str)))))
                .Select<KeyValuePair<int, NPCAiText>, NPCAiText>(
                    (Func<KeyValuePair<int, NPCAiText>, NPCAiText>) (entry => entry.Value)).ToArray<NPCAiText>();
        }

        /// <summary>Select entry by the id of the text</summary>
        /// <param name="id">Id of the text</param>
        public static NPCAiText GetFirstTextById(int id)
        {
            return NPCAiTextMgr.Entries
                .FirstOrDefault<KeyValuePair<int, NPCAiText>>(
                    (Func<KeyValuePair<int, NPCAiText>, bool>) (entry => entry.Value.Id == id)).Value;
        }

        /// <summary>It may be useful... sometime</summary>
        /// <param name="cb">Action</param>
        /// <param name="texts">Texts</param>
        public static void Apply(this Action<NPCAiText> cb, params NPCAiText[] texts)
        {
            foreach (NPCAiText npcAiText in ((IEnumerable<NPCAiText>) texts).Where<NPCAiText>(
                (Func<NPCAiText, bool>) (text => text != null)))
                cb(npcAiText);
        }
    }
}