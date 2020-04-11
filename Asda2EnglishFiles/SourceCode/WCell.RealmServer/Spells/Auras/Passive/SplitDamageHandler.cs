namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Allows to split all received damage with the caster
    /// Usually comes with a Dummy Aura.
    /// Description often points at at effect-values of: Soul Link (Id: 25228)
    /// </summary>
    public class SplitDamageHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
        }
    }
}