using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    internal class CastAnotherSpellHandler : SpellEffectHandler
    {
        public CastAnotherSpellHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if (this.Effect.MiscValue != 0)
                this.RunSpell((Unit) target, (uint) this.Effect.MiscValue);
            if (this.Effect.MiscValueB != 0)
                this.RunSpell((Unit) target, (uint) this.Effect.MiscValueB);
            if (this.Effect.MiscValueC == 0)
                return;
            this.RunSpell((Unit) target, (uint) this.Effect.MiscValueC);
        }

        private void RunSpell(Unit target, uint spellId)
        {
            if ((long) Utility.Random(0, 101) > (long) this.Cast.Spell.ProcChance)
                return;
            Spell spell = SpellHandler.Get(spellId);
            if (spell == null)
                return;
            Vector3 position = target.Position;
            SpellCast.Trigger((WorldObject) this.m_cast.CasterUnit, spell, ref position, (WorldObject) target);
        }
    }
}