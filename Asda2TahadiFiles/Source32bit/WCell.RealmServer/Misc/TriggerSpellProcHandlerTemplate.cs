using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Misc
{
  /// <summary>Triggers a spell on proc</summary>
  public class TriggerSpellProcHandlerTemplate : ProcHandlerTemplate
  {
    public Spell Spell { get; set; }

    public TriggerSpellProcHandlerTemplate(Spell spell, ProcTriggerFlags triggerFlags,
      ProcHitFlags hitFlags = ProcHitFlags.None, uint procChance = 100, int stackCount = 0)
      : this(spell, triggerFlags, null, hitFlags, procChance, stackCount)
    {
    }

    public TriggerSpellProcHandlerTemplate(Spell spell, ProcTriggerFlags triggerFlags,
      ProcValidator validator = null, ProcHitFlags hitFlags = ProcHitFlags.None, uint procChance = 100,
      int stackCount = 0)
      : base(triggerFlags, hitFlags, null, validator, procChance, stackCount)
    {
      Spell = spell;
      ProcAction = ProcSpell;
    }

    public bool ProcSpell(Unit creator, Unit triggerer, IUnitAction action)
    {
      SpellCast.ValidateAndTriggerNew(Spell, creator, triggerer, null,
        null, action, null);
      return false;
    }
  }
}