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
      Spell spell1 = SpellHandler.Get(SpellId);
      Spell spell2 = SpellHandler.Get(AddSpellId);
      if(spell1 == null || spell2 == null)
        ContentMgr.OnInvalidDBData("Invalid SpellLearnRelation: Spell {0} (#{1}) and AddSpell {2} (#{3})",
          (object) SpellId, (object) SpellId, (object) AddSpellId, (object) AddSpellId);
      else
        spell1.AdditionallyTaughtSpells.Add(spell2);
    }
  }
}