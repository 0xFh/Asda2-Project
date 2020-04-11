namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ParameterizedPeriodicHealHandler : PeriodicHealHandler
    {
        public int TotalHeal { get; set; }

        public ParameterizedPeriodicHealHandler(int totalDmg = 0)
        {
            this.TotalHeal = totalDmg;
        }

        protected override void Apply()
        {
            this.BaseEffectValue = this.TotalHeal / (this.m_aura.TicksLeft + 1);
            this.TotalHeal -= this.BaseEffectValue;
        }
    }
}