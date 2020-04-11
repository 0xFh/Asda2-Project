using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Gives a chance of $s1% of all melee and ranged attacks to land on the Caster instead of the Aura-owner
  /// </summary>
  public class AddCasterHitTriggerHandler : AttackEventEffectHandler
  {
    protected override void Apply()
    {
      if(SpellEffect.Spell.RealId == 160)
      {
        switch(SpellEffect.Spell.Level)
        {
          case 1:
            m_aura.Owner.SplinterEffect = 0.1f;
            m_aura.Owner.SplinterEffectChange = 15000;
            break;
          case 2:
            m_aura.Owner.SplinterEffect = 0.15f;
            m_aura.Owner.SplinterEffectChange = 20000;
            break;
          case 3:
            m_aura.Owner.SplinterEffect = 0.21f;
            m_aura.Owner.SplinterEffectChange = 25000;
            break;
          case 4:
            m_aura.Owner.SplinterEffect = 0.28f;
            m_aura.Owner.SplinterEffectChange = 30000;
            break;
          case 5:
            m_aura.Owner.SplinterEffect = 0.38f;
            m_aura.Owner.SplinterEffectChange = 35000;
            break;
          case 6:
            m_aura.Owner.SplinterEffect = 0.5f;
            m_aura.Owner.SplinterEffectChange = 40000;
            break;
          case 7:
            m_aura.Owner.SplinterEffect = 0.65f;
            m_aura.Owner.SplinterEffectChange = 45000;
            break;
        }
      }

      base.Apply();
    }

    protected override void Remove(bool cancelled)
    {
      if(SpellEffect.Spell.RealId == 160)
        m_aura.Owner.SplinterEffect = 0.0f;
      base.Remove(cancelled);
    }

    public override void OnAttack(DamageAction action)
    {
      if(action.Spell != null)
        return;
      if(Owner.SplinterEffect > 0.0)
      {
        foreach(WorldObject objectsInRadiu in Owner.GetObjectsInRadius(
          2.5f, ObjectTypes.Attackable, false, int.MaxValue))
        {
          if(Owner.IsHostileWith(objectsInRadiu) &&
             !ReferenceEquals(objectsInRadiu, Owner) &&
             Utility.Random(0, 100000) <= Owner.SplinterEffectChange)
          {
            Character targetChr = objectsInRadiu as Character;
            NPC targetNpc = objectsInRadiu as NPC;
            DamageAction unusedAction = Owner.GetUnusedAction();
            unusedAction.Damage =
              (int) (Owner.RandomDamage * (double) Owner.SplinterEffect);
            unusedAction.Attacker = objectsInRadiu as Unit;
            unusedAction.Victim = objectsInRadiu as Unit;
            int num = (int) unusedAction.DoAttack();
            if(Owner is Character)
              Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(null, targetChr,
                targetNpc, unusedAction.ActualDamage);
            action.OnFinished();
          }
        }
      }

      base.OnAttack(action);
    }
  }
}