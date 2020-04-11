using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class EnableCriticalHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            this.m_spellEffect.CopyAffectMaskTo(owner.PlayerAuras.CriticalStrikeEnabledMask);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            this.m_spellEffect.RemoveAffectMaskFrom(owner.PlayerAuras.CriticalStrikeEnabledMask);
        }
    }
}