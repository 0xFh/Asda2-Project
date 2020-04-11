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
            if ((long) id < (long) SpellLines.ById.Length)
                return SpellLines.ById[(int) id];
            return (SpellLine) null;
        }

        public static SpellLine[] GetLines(ClassId clss)
        {
            if ((int) clss < SpellLines.SpellLinesByClass.Length)
                return SpellLines.SpellLinesByClass[(int) clss];
            return (SpellLine[]) null;
        }

        internal static void InitSpellLines()
        {
            SpellLines.SetupSpellLines();
        }

        private static void AddSpellLines(ClassId clss, SpellLine[] lines)
        {
            SpellLines.SpellLinesByClass[(int) clss] = lines;
            foreach (SpellLine line in lines)
            {
                SpellLines.ById[(int) line.LineId] = line;
                Spell spell1 = (Spell) null;
                foreach (Spell spell2 in line)
                {
                    if (spell1 != null)
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