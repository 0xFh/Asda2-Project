using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Changes the owner's form.
  /// TODO: The act of shapeshifting frees the caster of Polymorph and Movement Impairing effects.
  /// </summary>
  public class ShapeshiftHandler : AuraEffectHandler
  {
    private ShapeshiftForm form;

    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
      Unit target, ref SpellFailedReason failReason)
    {
      form = (ShapeshiftForm) SpellEffect.MiscValue;
      if(target.ShapeshiftForm != form || form == ShapeshiftForm.BattleStance ||
         (form == ShapeshiftForm.BerserkerStance || form == ShapeshiftForm.DefensiveStance))
        return;
      if(Aura != null)
        target.Auras.RemoveWhere(aura => (int) aura.Spell.Id == (int) Aura.Spell.Id);
      failReason = SpellFailedReason.DontReport;
    }

    protected override void Apply()
    {
      Owner.ShapeshiftForm = form;
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ShapeshiftForm = ShapeshiftForm.Normal;
    }
  }
}