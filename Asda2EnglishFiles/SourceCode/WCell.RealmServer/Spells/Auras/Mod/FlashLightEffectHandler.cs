using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class FlashLightEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == null || Utility.Random(0, 100000) >= 2000)
                return;
            Spell spell = SpellHandler.Get(SpellId.Silence10Rank7FromWindSlasher);
            action.Victim.Auras.CreateAndStartAura(this.Owner.SharedReference, spell, false, (Item) null);
        }
    }
}