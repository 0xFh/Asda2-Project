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
            this.form = (ShapeshiftForm) this.SpellEffect.MiscValue;
            if (target.ShapeshiftForm != this.form || this.form == ShapeshiftForm.BattleStance ||
                (this.form == ShapeshiftForm.BerserkerStance || this.form == ShapeshiftForm.DefensiveStance))
                return;
            if (this.Aura != null)
                target.Auras.RemoveWhere((Predicate<Aura>) (aura => (int) aura.Spell.Id == (int) this.Aura.Spell.Id));
            failReason = SpellFailedReason.DontReport;
        }

        protected override void Apply()
        {
            this.Owner.ShapeshiftForm = this.form;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ShapeshiftForm = ShapeshiftForm.Normal;
        }
    }
}