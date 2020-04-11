namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class NoPvPCreditHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.YieldsXpOrHonor = false;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.YieldsXpOrHonor = true;
        }
    }
}