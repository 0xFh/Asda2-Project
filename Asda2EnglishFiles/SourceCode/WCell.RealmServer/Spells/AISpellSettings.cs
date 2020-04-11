using System;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class AISpellSettings
    {
        public readonly CooldownRange Cooldown = new CooldownRange(-1, -1);

        /// <summary>Amount of time to idle after casting the spell</summary>
        public int IdleTimeAfterCastMillis = 1000;

        public static readonly CooldownRange[] DefaultCooldownsByCategory = new CooldownRange[4];

        public static CooldownRange GetDefaultCategoryCooldown(AISpellCooldownCategory cat)
        {
            return AISpellSettings.DefaultCooldownsByCategory[(int) cat];
        }

        public static void SetDefaultCategoryCooldown(AISpellCooldownCategory cat, int min, int max)
        {
            AISpellSettings.DefaultCooldownsByCategory[(int) cat] = new CooldownRange(min, max);
        }

        static AISpellSettings()
        {
            AISpellSettings.DefaultCooldownsByCategory[0] = new CooldownRange(30000, 60000);
            AISpellSettings.DefaultCooldownsByCategory[1] = new CooldownRange(30000, 60000);
            AISpellSettings.DefaultCooldownsByCategory[2] = new CooldownRange(30000, 60000);
            AISpellSettings.DefaultCooldownsByCategory[3] = new CooldownRange(5000, 10000);
        }

        public AISpellSettings(Spell spell)
        {
            this.Spell = spell;
        }

        public Spell Spell { get; private set; }

        public void SetCooldownRange(int cdMin, int cdMax)
        {
            this.SetCooldown(cdMin, cdMax);
        }

        public void SetCooldownRange(int cd)
        {
            this.SetCooldown(cd);
        }

        public void SetCooldown(int cd)
        {
            this.SetCooldown(cd, cd);
        }

        public void SetCooldown(int cdMin, int cdMax)
        {
            this.Cooldown.MinDelay = cdMin;
            this.Cooldown.MaxDelay = cdMax;
        }

        internal void InitializeAfterLoad()
        {
            CooldownRange categoryCooldown =
                AISpellSettings.GetDefaultCategoryCooldown(this.Spell.GetAISpellCooldownCategory());
            if (this.Cooldown.MinDelay < 0)
                this.Cooldown.MinDelay = categoryCooldown.MinDelay;
            if (this.Cooldown.MaxDelay >= 0)
                return;
            this.Cooldown.MaxDelay = categoryCooldown.MaxDelay;
        }
    }
}