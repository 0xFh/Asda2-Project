using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Talents;
using WCell.Constants.Updates;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class SpellGetCommand : RealmServerCommand
  {
    public static Spell[] RetrieveSpells(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      string[] strArray = trigger.Text.Remainder.Split(new string[1]
      {
        ","
      }, StringSplitOptions.RemoveEmptyEntries);
      List<Spell> spellList = new List<Spell>(strArray.Length);
      foreach(string input in strArray)
      {
        Spell spell = null;
        SpellId result1;
        if(EnumUtil.TryParse(input, out result1))
          spell = SpellHandler.Get(result1);
        if(spell == null)
        {
          SpellLineId result2;
          if(EnumUtil.TryParse(input, out result2))
          {
            SpellLine line = result2.GetLine();
            if(line != null)
              spell = line.HighestRank;
          }

          if(spell == null)
          {
            TalentEntry entry = TalentMgr.GetEntry(trigger.Text.NextEnum(TalentId.None));
            if(entry != null && entry.Spells != null && entry.Spells.Length > 0)
              spell = entry.Spells[entry.Spells.Length - 1];
            if(spell == null)
              continue;
          }
        }

        spellList.Add(spell);
      }

      return spellList.ToArray();
    }

    protected override void Initialize()
    {
      Init("GetSpell", "SpellGet");
      Description = new TranslatableItem(RealmLangKey.CmdSpellGetDescription);
      ParamInfo = new TranslatableItem(RealmLangKey.CmdSpellGetParamInfo);
    }

    public override object Eval(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Spell[] spellArray = RetrieveSpells(trigger);
      if(spellArray.Length == 0)
        return null;
      if(spellArray.Length <= 1)
        return spellArray[0];
      return spellArray;
    }

    public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
    {
      Spell[] spellArray = RetrieveSpells(trigger);
      trigger.Reply(spellArray.ToString());
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.None; }
    }
  }
}