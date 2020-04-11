/*************************************************************************
 *
 *   file		: AddCasterHitTrigger.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
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
            if (SpellEffect.Spell.RealId == 160)//splinter
            {
                switch (SpellEffect.Spell.Level)
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
                     m_aura.Owner.SplinterEffect = 0.50f;
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
            if (SpellEffect.Spell.RealId == 160)//splinter
            {
                m_aura.Owner.SplinterEffect = 0;
            }
            base.Remove(cancelled);
        }
        public override void OnAttack(WCell.RealmServer.Misc.DamageAction action)
        {
            if(action.Spell!=null)
                return;
            if (Owner.SplinterEffect > 0)
            {
                var targets = Owner.GetObjectsInRadius(2.5f, ObjectTypes.Attackable, false);
                foreach (var worldObject in targets)
                {
                    if (!Owner.IsHostileWith(worldObject) || ReferenceEquals(worldObject, Owner))
                        continue;
                    if (Utility.Random(0, 100000) > Owner.SplinterEffectChange)
                        continue;
                    var targetChr = worldObject as Character;
                    var targetNpc = worldObject as NPC;
                    var a = Owner.GetUnusedAction();
                    a.Damage = (int)(Owner.RandomDamage * Owner.SplinterEffect);
                    a.Attacker = worldObject as Unit;
                    a.Victim = worldObject as Unit;
                    a.DoAttack();
                    if(Owner is Character)
                        Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(null, targetChr, targetNpc, a.ActualDamage);
                    action.OnFinished();
                }

            }
            base.OnAttack(action);
        }
	}
};