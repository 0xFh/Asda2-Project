using WCell.Constants;
using WCell.RealmServer.Modifiers;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModInvisibilityHandler : AuraEffectHandler
  {
    [NotVariable]public static float SpeedDecreaceOnInvis = 0.5f;

    protected override void Apply()
    {
      int miscValue = m_spellEffect.MiscValue;
      Owner.IsVisible = false;
      Owner.ChangeModifier(StatModifierFloat.Speed, -SpeedDecreaceOnInvis);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.IsVisible = true;
      Owner.ChangeModifier(StatModifierFloat.Speed, SpeedDecreaceOnInvis);
    }
  }
}