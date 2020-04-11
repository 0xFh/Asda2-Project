using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class ProcTriggerSpellOnAutoAttackHandler : ProcTriggerSpellHandler
    {
        public override bool CanProcBeTriggeredBy(IUnitAction action)
        {
            if (action.Spell != null)
                return action.Spell.IsAutoRepeating;
            return true;
        }
    }
}