using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.Skills
{
    public class SpellLearnRelation : IDataHolder
    {
        public SpellId SpellId;
        public SpellId AddSpellId;

        public void FinalizeDataHolder()
        {
            Spell spell1 = SpellHandler.Get(this.SpellId);
            Spell spell2 = SpellHandler.Get(this.AddSpellId);
            if (spell1 == null || spell2 == null)
                ContentMgr.OnInvalidDBData("Invalid SpellLearnRelation: Spell {0} (#{1}) and AddSpell {2} (#{3})",
                    (object) this.SpellId, (object) this.SpellId, (object) this.AddSpellId, (object) this.AddSpellId);
            else
                spell1.AdditionallyTaughtSpells.Add(spell2);
        }
    }
}