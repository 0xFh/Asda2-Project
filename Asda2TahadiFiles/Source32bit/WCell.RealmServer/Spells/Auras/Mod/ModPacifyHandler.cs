namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>
  /// Prevents carrier from attacking or using "physical abilities"
  /// </summary>
  public class ModPacifyHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      ++Owner.Pacified;
    }

    protected override void Remove(bool cancelled)
    {
      --Owner.Pacified;
    }
  }
}