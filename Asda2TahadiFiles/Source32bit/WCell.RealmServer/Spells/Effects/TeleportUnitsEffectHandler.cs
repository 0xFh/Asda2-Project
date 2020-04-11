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
      if(Effect.MiscValue == 10)
        m_cast.CasterChar.Position = target.Position;
      else if(Effect.Spell.IsHearthStoneSpell && m_cast.CasterChar != null)
      {
        IWorldZoneLocation pos = m_cast.CasterChar.BindLocation;
        target.AddMessage(() => ((Unit) target).TeleportTo(pos));
      }
      else if(Effect.ImplicitTargetB == ImplicitSpellTargetType.BehindTargetLocation)
      {
        Unit unit = (Unit) target;
        if(unit == null)
          return;
        float orientation = unit.Orientation;
        m_cast.CasterChar.TeleportTo(
          new Vector3(unit.Position.X - (unit.BoundingRadius + 0.5f) * (float) Math.Cos(orientation),
            unit.Position.Y - (unit.BoundingRadius + 0.5f) * (float) Math.Sin(orientation),
            unit.Position.Z), orientation);
      }
      else
      {
        Map map = m_cast.TargetMap;
        Vector3 pos = m_cast.TargetLoc;
        float ori = m_cast.TargetOrientation;
        target.AddMessage(() => ((Unit) target).TeleportTo(map, pos, ori));
      }
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}