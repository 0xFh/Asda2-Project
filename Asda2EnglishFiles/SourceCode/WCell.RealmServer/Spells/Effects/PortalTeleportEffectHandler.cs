using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells.Effects
{
    public class PortalTeleportEffectHandler : SpellEffectHandler
    {
        public PortalTeleportEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            return SpellFailedReason.Ok;
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            if (!(target is Character))
                return SpellFailedReason.TargetNotPlayer;
            return !((Unit) target).MayTeleport ? SpellFailedReason.TargetAurastate : SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            Character chr = (Character) target;
            if (chr == null)
                return;
            chr.IsMoving = false;
            WorldLocation newPos = new WorldLocation((MapId) this.Effect.MiscValue,
                new Vector3((float) this.Effect.MiscValueB, (float) this.Effect.MiscValueC), 1U);
            target.AddMessage((Action) (() => chr.TeleportTo((IWorldLocation) newPos)));
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Player; }
        }
    }
}