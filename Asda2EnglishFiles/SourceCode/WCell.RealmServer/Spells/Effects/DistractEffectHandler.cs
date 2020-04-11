using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>
    /// TODO: Stop movement for a short time or until someting happened to the NPC
    /// </summary>
    public class DistractEffectHandler : SpellEffectHandler
    {
        public DistractEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if (!(target is NPC))
                return;
            NPC npc = (NPC) target;
            this.CalcEffectValue();
            npc.Face(this.m_cast.TargetLoc);
            npc.Movement.Stop();
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}