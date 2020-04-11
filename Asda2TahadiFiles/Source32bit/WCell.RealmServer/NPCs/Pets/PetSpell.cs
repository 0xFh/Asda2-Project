using Castle.ActiveRecord;
using WCell.Constants.Pets;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.NPCs.Pets
{
  public class PetSpell
  {
    public static readonly PetSpell[] EmptyArray = new PetSpell[0];
    private Spell m_Spell;

    [Field("PetSpellState", NotNull = true)]
    private int _petSpellState;

    [PrimaryKey(PrimaryKeyType.GuidComb, "PetSpellId")]
    public long Guid { get; set; }

    public Spell Spell
    {
      get { return m_Spell; }
      set { m_Spell = value; }
    }

    [Property("SpellId", NotNull = true)]
    public int SpellId
    {
      get
      {
        if(m_Spell == null)
          return 0;
        return (int) m_Spell.Id;
      }
      set { m_Spell = SpellHandler.Get((uint) value); }
    }

    public PetSpellState State
    {
      get { return (PetSpellState) _petSpellState; }
      set { _petSpellState = (int) value; }
    }

    public override string ToString()
    {
      return Spell.ToString();
    }
  }
}