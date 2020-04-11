using WCell.Constants;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
  public static class SpellLines
  {
    public static readonly SpellLine[][] SpellLinesByClass = new SpellLine[12][];
    public static readonly SpellLine[] ById = new SpellLine[1178];

    public static SpellLine GetLine(this SpellLineId id)
    {
      if((long) id < ById.Length)
        return ById[(int) id];
      return null;
    }

    public static SpellLine[] GetLines(ClassId clss)
    {
      if((int) clss < SpellLinesByClass.Length)
        return SpellLinesByClass[(int) clss];
      return null;
    }

    internal static void InitSpellLines()
    {
      SetupSpellLines();
    }

    private static void AddSpellLines(ClassId clss, SpellLine[] lines)
    {
      SpellLinesByClass[(int) clss] = lines;
      foreach(SpellLine line in lines)
      {
        ById[(int) line.LineId] = line;
        Spell spell1 = null;
        foreach(Spell spell2 in line)
        {
          if(spell1 != null)
          {
            spell2.PreviousRank = spell1;
            spell1.NextRank = spell2;
          }

          spell1 = spell2;
        }
      }
    }

    private static void SetupSpellLines()
    {
    }
  }
}