using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>Energizes EffectValue on proc</summary>
    public class ProcEnergizeHandler : AuraEffectHandler
    {
        public override void OnProc(Unit triggerer, IUnitAction action)
        {
            this.Owner.Energize(this.EffectValue, this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}