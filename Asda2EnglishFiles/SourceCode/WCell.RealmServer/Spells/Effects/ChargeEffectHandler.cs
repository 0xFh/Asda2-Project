using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells.Effects
{
    public class ChargeEffectHandler : SpellEffectHandler
    {
        public ChargeEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            return SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if (this.Effect.Spell.Id != 207U && this.Effect.Spell.Id != 97U)
                return;
            float num = (float) ((double) this.m_cast.CasterObject.Position.GetDistance(target.Position) -
                                 (double) ((Unit) target).BoundingRadius - 0.699999988079071);
            Vector3 vector3 = target.Position - this.m_cast.CasterObject.Position;
            vector3.Normalize();
            this.Cast.CasterChar.Position = this.Cast.CasterChar.Position + vector3 * num;
            if (!(target is NPC) && this.Effect.Spell.Id != 207U)
                return;
            Asda2MovmentHandler.SendStartMoveCommonToAreaResponse(this.Cast.CasterChar, true, true);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}