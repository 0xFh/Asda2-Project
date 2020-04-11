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
            if(!(target is Character))
                return SpellFailedReason.TargetNotPlayer;
            if (!((Unit)target).MayTeleport)
            {
                return SpellFailedReason.TargetAurastate;
            }
            return SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            var chr = (Character) target;
            if(chr==null)return;
            chr.IsMoving = false;
            var newPos = new WorldLocation((MapId) Effect.MiscValue, new Vector3(Effect.MiscValueB, Effect.MiscValueC));
            target.AddMessage(()=>chr.TeleportTo(newPos));
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Player; }
        }
    }
}