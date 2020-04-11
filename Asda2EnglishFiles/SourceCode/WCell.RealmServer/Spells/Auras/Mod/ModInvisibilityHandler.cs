using WCell.Constants;
using WCell.RealmServer.Modifiers;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModInvisibilityHandler : AuraEffectHandler
    {
        [NotVariable] public static float SpeedDecreaceOnInvis = 0.5f;

        protected override void Apply()
        {
            int miscValue = this.m_spellEffect.MiscValue;
            this.Owner.IsVisible = false;
            this.Owner.ChangeModifier(StatModifierFloat.Speed, -ModInvisibilityHandler.SpeedDecreaceOnInvis);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.IsVisible = true;
            this.Owner.ChangeModifier(StatModifierFloat.Speed, ModInvisibilityHandler.SpeedDecreaceOnInvis);
        }
    }
}