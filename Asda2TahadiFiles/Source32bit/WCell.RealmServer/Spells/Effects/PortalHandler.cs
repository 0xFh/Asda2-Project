using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;

namespace WCell.RealmServer.Spells.Effects
{
  public class PortalHandler : SpellEffectHandler
  {
    public PortalHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      return (double) m_cast.TargetLoc.X == 0.0 || (double) m_cast.TargetLoc.Y == 0.0
        ? SpellFailedReason.BadTargets
        : SpellFailedReason.Ok;
    }

    public override void Apply()
    {
      Portal portal =
        Portal.Create(
          new WorldLocation(m_cast.TargetMap, m_cast.CasterObject.Position, 1U),
          new WorldLocation(m_cast.TargetMap, m_cast.TargetLoc, 1U));
      portal.State = GameObjectState.Enabled;
      portal.Flags = GameObjectFlags.None;
      portal.Orientation = m_cast.TargetOrientation;
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }

    public override bool HasOwnTargets
    {
      get { return false; }
    }
  }
}