using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Spells.Effects
{
  public class DispelEffectHandler : SpellEffectHandler
  {
    public DispelEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      DispelType miscValue = (DispelType) Effect.MiscValue;
      if(miscValue == DispelType.None)
        throw new Exception("Invalid DispelType None in Spell: " + Effect.Spell);
      Unit casterUnit1 = Cast.CasterUnit;
      int num = CalcEffectValue();
      foreach(Aura aura in ((Unit) target).Auras)
      {
        if(aura.Spell.DispelType == miscValue)
        {
          Unit casterUnit2 = aura.CasterUnit;
          if(casterUnit1 != null && casterUnit2 != null &&
             (casterUnit1.MayAttack(casterUnit2) &&
              casterUnit2.Auras.GetModifiedInt(SpellModifierType.DispelResistance, aura.Spell, 1) >
              Utility.Random(100)))
          {
            if(--num == 0)
              break;
          }
          else
          {
            aura.Remove(true);
            if(--num == 0)
              break;
          }
        }
      }
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}