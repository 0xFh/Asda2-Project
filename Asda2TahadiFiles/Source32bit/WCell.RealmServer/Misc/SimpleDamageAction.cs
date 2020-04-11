using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Misc
{
  public class SimpleDamageAction : IDamageAction, IUnitAction
  {
    public Unit Attacker
    {
      get { return null; }
    }

    public Unit Victim { get; set; }

    public SpellEffect SpellEffect
    {
      get { return null; }
    }

    public int Damage { get; set; }

    public int ActualDamage
    {
      get { return Damage; }
    }

    public bool IsDot
    {
      get { return false; }
    }

    public bool IsCritical
    {
      get { return false; }
    }

    public DamageSchool UsedSchool
    {
      get { return DamageSchool.Physical; }
    }

    public IAsda2Weapon Weapon
    {
      get { return null; }
    }

    public Spell Spell
    {
      get { return null; }
    }

    /// <summary>Does nothing</summary>
    public int ReferenceCount
    {
      get { return 0; }
      set { }
    }
  }
}