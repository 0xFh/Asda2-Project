using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Misc;
using WCell.RealmServer.Content;
using WCell.Util.Data;

namespace WCell.RealmServer.Gossips
{
    /// <summary>Cacheable GossipEntry from DB</summary>
    public class StaticGossipEntry : GossipEntry, IDataHolder
    {
        public StaticGossipEntry()
        {
            this.GossipTexts = (GossipTextBase[]) new StaticGossipText[8];
            for (int index = 0; index < this.GossipTexts.Length; ++index)
                this.GossipTexts[index] = (GossipTextBase) new StaticGossipText();
        }

        public StaticGossipEntry(uint id, params string[] texts)
        {
            this.GossipId = id;
            this.GossipTexts = (GossipTextBase[]) new StaticGossipText[texts.Length];
            float probability = 1f / (float) texts.Length;
            for (int index = 0; index < texts.Length; ++index)
                this.GossipTexts[index] =
                    (GossipTextBase) new StaticGossipText(texts[index], probability, ChatLanguage.Universal);
            this.FinalizeDataHolder();
        }

        public StaticGossipEntry(uint id, ChatLanguage lang, params string[] texts)
        {
            this.GossipId = id;
            this.GossipTexts = (GossipTextBase[]) new StaticGossipText[texts.Length];
            float probability = 1f / (float) texts.Length;
            for (int index = 0; index < texts.Length; ++index)
            {
                string text = texts[index];
                this.GossipTexts[index] = (GossipTextBase) new StaticGossipText(text, probability, lang);
            }

            this.FinalizeDataHolder();
        }

        public StaticGossipEntry(uint id, params StaticGossipText[] entries)
        {
            this.GossipId = id;
            this.GossipTexts = (GossipTextBase[]) entries;
            this.FinalizeDataHolder();
        }

        /// <summary>GossipEntry's from DB are always cached</summary>
        public override bool IsDynamic
        {
            get { return false; }
        }

        public StaticGossipText GetText(int i)
        {
            return (StaticGossipText) this.m_Texts[i];
        }

        public void FinalizeDataHolder()
        {
            if (this.m_Texts == null)
                ContentMgr.OnInvalidDBData("Entries is null in: " + (object) this);
            else if (this.GossipId > 0U)
            {
                this.m_Texts = ((IEnumerable<GossipTextBase>) this.m_Texts).Where<GossipTextBase>(
                    (Func<GossipTextBase, bool>) (entry =>
                    {
                        if (string.IsNullOrEmpty(((StaticGossipText) entry).TextFemale))
                            return !string.IsNullOrEmpty(((StaticGossipText) entry).TextMale);
                        return true;
                    })).ToArray<GossipTextBase>();
                GossipMgr.GossipEntries[this.GossipId] = (IGossipEntry) this;
                foreach (StaticGossipText text in this.m_Texts)
                {
                    bool flag1 = string.IsNullOrEmpty(text.TextMale);
                    bool flag2 = string.IsNullOrEmpty(text.TextFemale);
                    if (flag1 && flag2)
                    {
                        text.TextMale = " ";
                        text.TextFemale = " ";
                    }
                    else if (flag1)
                        text.TextMale = text.TextFemale;
                    else if (flag2)
                        text.TextFemale = text.TextMale;
                }
            }
            else
                ContentMgr.OnInvalidDBData("Invalid id: " + (object) this);
        }

        public DataHolderState DataHolderState { get; set; }

        public static IEnumerable<StaticGossipEntry> GetAllDataHolders()
        {
            List<StaticGossipEntry> staticGossipEntryList = new List<StaticGossipEntry>(GossipMgr.GossipEntries.Count);
            staticGossipEntryList.AddRange(GossipMgr.GossipEntries.Values
                .Where<IGossipEntry>((Func<IGossipEntry, bool>) (entry => entry is StaticGossipEntry))
                .OfType<StaticGossipEntry>());
            return (IEnumerable<StaticGossipEntry>) staticGossipEntryList;
        }
    }
}