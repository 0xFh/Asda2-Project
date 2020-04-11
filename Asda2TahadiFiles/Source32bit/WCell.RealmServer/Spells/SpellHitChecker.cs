using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells
{
  internal sealed class SpellHitChecker
  {
    private Spell spell;
    private WorldObject caster;
    private Unit target;

    public void Initialize(Spell spell, WorldObject caster)
    {
      this.spell = spell;
      this.caster = caster;
    }

    public CastMissReason CheckHitAgainstTarget(Unit target)
    {
      this.target = target;
      return CheckHitAgainstTarget();
    }

    private CastMissReason CheckHitAgainstTarget()
    {
      if(target.IsEvading)
        return CastMissReason.Evade;
      if(spell.IsAffectedByInvulnerability ||
         target is Character && ((Character) target).Role.IsStaff)
      {
        if(target.IsInvulnerable)
          return CastMissReason.Immune_2;
        if(spell.IsAffectedByInvulnerability &&
           spell.Schools.All(
             target.IsImmune))
          return CastMissReason.Immune;
      }

      return CheckMiss() ? CastMissReason.Miss : CastMissReason.None;
    }

    private bool CheckMiss()
    {
      return CalculateHitChanceAgainstTargetInPercentage() < (double) Utility.Random(0, 101);
    }

    private float CalculateHitChanceAgainstTargetInPercentage()
    {
      float min = 0.0f;
      float targetInPercentage = CalculateBaseHitChanceAgainstTargetInPercentage();
      if(caster is Unit)
      {
        targetInPercentage += (caster as Unit).GetHighestSpellHitChanceMod(spell.Schools);
        if(caster is Character)
        {
          min = 1f;
          targetInPercentage += (caster as Character).SpellHitChanceFromHitRating;
        }
      }

      return MathUtil.ClampMinMax(targetInPercentage, min, 100f);
    }

    private int CalculateBaseHitChanceAgainstTargetInPercentage()
    {
      int num1 = target.Level - caster.SharedReference.Level;
      if(num1 < 3)
      {
        int num2 = 96 - num1;
        if(num2 <= 100)
          return num2;
        return 100;
      }

      int num3 = !(target is Character) ? 11 : 7;
      return 94 - (num1 - 2) * num3;
    }
  }
}