namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Allows to hover</summary>
  public class HoverHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      ++m_aura.Auras.Owner.Hovering;
    }

    protected override void Remove(bool cancelled)
    {
      --m_aura.Auras.Owner.Hovering;
    }
  }
}