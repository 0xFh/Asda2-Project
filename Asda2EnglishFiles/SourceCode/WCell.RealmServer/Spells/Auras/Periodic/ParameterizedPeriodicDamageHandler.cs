namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ParameterizedPeriodicDamageHandler : PeriodicDamageHandler
    {
        public int TotalDamage { get; set; }

        public ParameterizedPeriodicDamageHandler()
            : this(0)
        {
        }

        public ParameterizedPeriodicDamageHandler(int totalDmg)
        {
            this.TotalDamage = totalDmg;
        }

        protected override void Apply()
        {
            this.BaseEffectValue = this.TotalDamage / (this.m_aura.TicksLeft + 1);
            this.TotalDamage -= this.BaseEffectValue;
        }
    }
}