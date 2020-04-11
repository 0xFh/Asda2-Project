using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Lang;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
  /// <summary>Texts yelled by mobs</summary>
  public class NPCAiText : IDataHolder
  {
    /// <summary>
    /// Texts on all languages : 0 - eng, ... , 7 - rus, requied for .Localize()
    /// </summary>
    [Persistent(8)]public string[] Texts = new string[8];

    public int Id;
    public int Sound;

    /// <summary>
    /// Custom chat message type that indicates how the message is being transmitted
    /// </summary>
    public uint Type;

    public int Language;
    public int Emote;

    /// <summary>Mob's ID or string like "Common Kobold Text"</summary>
    public string Comment;

    public string Text
    {
      get { return Texts.LocalizeWithDefaultLocale(); }
      set { Texts[(int) RealmServerConfiguration.DefaultLocale] = value; }
    }

    public int GetMobId()
    {
      int result;
      if(!int.TryParse(Comment, out result))
        return 0;
      return result;
    }

    /// <summary>
    /// Is called to initialize the object; usually after a set of other operations have been performed or if
    /// the right time has come and other required steps have been performed.
    /// </summary>
    public void FinalizeDataHolder()
    {
      Texts = Texts
        .Select(x => x ?? "").ToArray();
      NPCAiTextMgr.Entries.Add(Id, this);
    }
  }
}