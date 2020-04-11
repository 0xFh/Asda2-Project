using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells.Effects
{
    public class InstantKillEffectHandler : SpellEffectHandler
    {
        public InstantKillEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            NPC npc = target as NPC;
            if (npc != null && npc.Entry.IsBoss)
                return;
            Character character = target as Character;
            if (character != null && character.Role.IsStaff || Utility.Random(0, 100) >= this.Effect.MiscValue)
                return;
            ((Unit) target).Kill(this.m_cast.CasterUnit);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}