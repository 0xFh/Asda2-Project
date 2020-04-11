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
      GossipTexts = new StaticGossipText[8];
      for(int index = 0; index < GossipTexts.Length; ++index)
        GossipTexts[index] = new StaticGossipText();
    }

    public StaticGossipEntry(uint id, params string[] texts)
    {
      GossipId = id;
      GossipTexts = new StaticGossipText[texts.Length];
      float probability = 1f / texts.Length;
      for(int index = 0; index < texts.Length; ++index)
        GossipTexts[index] =
          new StaticGossipText(texts[index], probability, ChatLanguage.Universal);
      FinalizeDataHolder();
    }

    public StaticGossipEntry(uint id, ChatLanguage lang, params string[] texts)
    {
      GossipId = id;
      GossipTexts = new StaticGossipText[texts.Length];
      float probability = 1f / texts.Length;
      for(int index = 0; index < texts.Length; ++index)
      {
        string text = texts[index];
        GossipTexts[index] = new StaticGossipText(text, probability, lang);
      }

      FinalizeDataHolder();
    }

    public StaticGossipEntry(uint id, params StaticGossipText[] entries)
    {
      GossipId = id;
      GossipTexts = entries;
      FinalizeDataHolder();
    }

    /// <summary>GossipEntry's from DB are always cached</summary>
    public override bool IsDynamic
    {
      get { return false; }
    }

    public StaticGossipText GetText(int i)
    {
      return (StaticGossipText) m_Texts[i];
    }

    public void FinalizeDataHolder()
    {
      if(m_Texts == null)
        ContentMgr.OnInvalidDBData("Entries is null in: " + this);
      else if(GossipId > 0U)
      {
        m_Texts = m_Texts.Where(
          entry =>
          {
            if(string.IsNullOrEmpty(((StaticGossipText) entry).TextFemale))
              return !string.IsNullOrEmpty(((StaticGossipText) entry).TextMale);
            return true;
          }).ToArray();
        GossipMgr.GossipEntries[GossipId] = this;
        foreach(StaticGossipText text in m_Texts)
        {
          bool flag1 = string.IsNullOrEmpty(text.TextMale);
          bool flag2 = string.IsNullOrEmpty(text.TextFemale);
          if(flag1 && flag2)
          {
            text.TextMale = " ";
            text.TextFemale = " ";
          }
          else if(flag1)
            text.TextMale = text.TextFemale;
          else if(flag2)
            text.TextFemale = text.TextMale;
        }
      }
      else
        ContentMgr.OnInvalidDBData("Invalid id: " + this);
    }

    public DataHolderState DataHolderState { get; set; }

    public static IEnumerable<StaticGossipEntry> GetAllDataHolders()
    {
      List<StaticGossipEntry> staticGossipEntryList = new List<StaticGossipEntry>(GossipMgr.GossipEntries.Count);
      staticGossipEntryList.AddRange(GossipMgr.GossipEntries.Values
        .Where(entry => entry is StaticGossipEntry)
        .OfType<StaticGossipEntry>());
      return staticGossipEntryList;
    }
  }
}