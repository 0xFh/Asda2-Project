using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
  [Serializable]
  public class SpellTriggerInfo
  {
    [NotPersistent]public Spell Spell;
    private SpellId m_SpellId;
    public uint QuestId;

    public SpellId SpellId
    {
      get { return m_SpellId; }
      set
      {
        m_SpellId = value;
        if(value == SpellId.None)
          return;
        Spell = SpellHandler.Get(value);
      }
    }
  }
}