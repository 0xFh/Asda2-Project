namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Makes one ignore target's resistances</summary>
  public class ModTargetResistanceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ModTargetResistanceMod(EffectValue, m_spellEffect.MiscBitSet);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ModTargetResistanceMod(-EffectValue, m_spellEffect.MiscBitSet);
    }
  }
}