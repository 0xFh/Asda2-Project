using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
  [Serializable]
  public class NPCAddonData
  {
    [NotPersistent]public INPCDataHolder DataHolder;
    public uint Bytes;
    public uint Bytes2;
    public EmoteType EmoteState;
    public uint MountModelId;
    public string AuraIdStr;
    [NotPersistent]public List<Spell> Auras;

    public uint SheathType
    {
      get { return (byte) Bytes2; }
      set { Bytes2 = Bytes2 & 4294967040U | value; }
    }

    public uint PvPState
    {
      get { return (byte) (Bytes2 >> 8); }
      set { Bytes2 = (uint) ((int) Bytes2 & -65281 | (int) value << 8); }
    }

    public void AddAura(SpellId spellId)
    {
      Spell spell = SpellHandler.Get(spellId);
      if(spell == null)
        LogManager.GetCurrentClassLogger().Warn("Tried to add invalid Aura-Spell \"{0}\" to NPCEntry: {1}",
          spellId, this);
      else
        Auras.Add(spell);
    }

    internal void InitAddonData(INPCDataHolder dataHolder)
    {
      DataHolder = dataHolder;
      NPCEntry entry = dataHolder.Entry;
      if(!string.IsNullOrEmpty(AuraIdStr))
      {
        SpellId[] spellIdArray = AuraIdStr.Split(new char[1]
        {
          ' '
        }, StringSplitOptions.RemoveEmptyEntries).TransformArray(
          idStr =>
          {
            uint result;
            if(!uint.TryParse(idStr.Trim(), out result))
              LogManager.GetCurrentClassLogger()
                .Warn("Invalidly formatted Aura ({0}) in AuraString for SpawnEntry: {1}",
                  idStr, this);
            return (SpellId) result;
          });
        if(spellIdArray != null)
        {
          Auras = new List<Spell>(spellIdArray.Length);
          foreach(SpellId spellId in spellIdArray)
          {
            Spell spell = SpellHandler.Get(spellId);
            if(spell != null)
            {
              if(!spell.IsAura || spell.Durations.Min > 0 && spell.Durations.Min < int.MaxValue)
              {
                if(entry.Spells == null || !entry.Spells.ContainsKey(spell.SpellId))
                  entry.AddSpell(spell);
              }
              else
                Auras.Add(spell);
            }
          }
        }
      }

      if(Auras != null)
        return;
      Auras = new List<Spell>(2);
    }
  }
}