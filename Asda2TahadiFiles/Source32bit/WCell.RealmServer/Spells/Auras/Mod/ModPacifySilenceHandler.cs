using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>
  /// Prevents carrier from attacking or using "physical abilities"
  /// </summary>
  public class ModPacifySilenceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      ++Owner.Pacified;
      Owner.IncMechanicCount(SpellMechanic.Silenced, false);
    }

    protected override void Remove(bool cancelled)
    {
      --Owner.Pacified;
      Owner.DecMechanicCount(SpellMechanic.Silenced, false);
    }
  }
}