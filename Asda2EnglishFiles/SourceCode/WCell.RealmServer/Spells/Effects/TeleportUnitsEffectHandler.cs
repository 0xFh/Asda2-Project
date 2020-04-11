using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells.Effects
{
    public class TeleportUnitsEffectHandler : SpellEffectHandler
    {
        public TeleportUnitsEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            return SpellFailedReason.Ok;
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            return SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            if (this.Effect.MiscValue == 10)
                this.m_cast.CasterChar.Position = target.Position;
            else if (this.Effect.Spell.IsHearthStoneSpell && this.m_cast.CasterChar != null)
            {
                IWorldZoneLocation pos = this.m_cast.CasterChar.BindLocation;
                target.AddMessage((Action) (() => ((Unit) target).TeleportTo((IWorldLocation) pos)));
            }
            else if (this.Effect.ImplicitTargetB == ImplicitSpellTargetType.BehindTargetLocation)
            {
                Unit unit = (Unit) target;
                if (unit == null)
                    return;
                float orientation = unit.Orientation;
                this.m_cast.CasterChar.TeleportTo(
                    new Vector3(unit.Position.X - (unit.BoundingRadius + 0.5f) * (float) Math.Cos((double) orientation),
                        unit.Position.Y - (unit.BoundingRadius + 0.5f) * (float) Math.Sin((double) orientation),
                        unit.Position.Z), new float?(orientation));
            }
            else
            {
                Map map = this.m_cast.TargetMap;
                Vector3 pos = this.m_cast.TargetLoc;
                float ori = this.m_cast.TargetOrientation;
                target.AddMessage((Action) (() => ((Unit) target).TeleportTo(map, pos, new float?(ori))));
            }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}