/*************************************************************************
 *
 *   file		: PeriodicDamage.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-10 13:00:10 +0100 (s? 10 jan 2010) $

 *   revision		: $Rev: 1185 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Periodic
{
    /// <summary>
    /// Periodically damages the holder
    /// </summary>
    public class WhirlwindEffectHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var targets = Owner.GetObjectsInRadius(6, ObjectTypes.Unit, false);
            if (targets == null)
                return;
            foreach (var worldObject in targets)
            {
                var unit = worldObject as Unit;
                if (unit == null) continue;
                if (Owner == null || m_aura == null || !unit.IsHostileWith(Owner))
                    continue;
                var action = unit.DealSpellDamage(Owner, SpellEffect,
                                                   (int)(Owner.RandomDamage * SpellEffect.MiscValue / 100), true, true, false, false);
                if (action == null)
                    continue;
                WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character, unit as Character, unit as NPC, action.ActualDamage);

                action.OnFinished();
            }
        }
        protected override void Remove(bool cancelled)
        {
        }
    }
    public class TimeBombEffectHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
        }
        protected override void Remove(bool cancelled)
        {
            var cst = m_aura.CasterUnit;
            if (cst == null)
                return;
            var targets = Owner.GetObjectsInRadius(6, ObjectTypes.Unit, false);
            foreach (var worldObject in targets)
            {
                if (!cst.IsHostileWith(worldObject))
                    continue;
                var unit = worldObject as Unit;
                if (unit == null) continue;
                var action = unit.DealSpellDamage(cst, SpellEffect, (int)(cst.RandomDamage * SpellEffect.MiscValue / 200), true, true, false, false);
                if (action == null)
                    continue;
                WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character, Owner as Character, unit as NPC, action.ActualDamage);
                action.OnFinished();
            }
        }
    }
    public class ThunderBoltEffectHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var dmg = Owner.Health * SpellEffect.MiscValue / 100;
            var npc = Owner as NPC;
            if (npc != null)
            {
                if (npc.Entry.IsBoss) return;
            }
            var action = Owner.DealSpellDamage(Owner, SpellEffect, dmg, true, true, false, false);
            if (action != null && m_aura != null)
            {
                WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character, Owner as Character, Owner as NPC, action.ActualDamage);
                action.OnFinished();
                Aura.Cancel();
            }
        }
        protected override void Remove(bool cancelled)
        {
        }
    }
    public class ResurectOnDeathPlaceEffectHandler : AuraEffectHandler
    {

    }
    public class TrapEffectHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var targets = Owner.GetObjectsInRadius(3, ObjectTypes.Unit, false);
            var boom = false;
            foreach (var worldObject in targets)
            {
                var unit = worldObject as Unit;
                if (unit == null) continue;
                if (!unit.IsHostileWith(Owner))
                    continue;
                boom = true;
                break;
            }
            if (!boom) return;
            targets = Owner.GetObjectsInRadius(12, ObjectTypes.Unit, false);
            foreach (var worldObject in targets)
            {
                var unit = worldObject as Unit;
                if (unit == null) continue;
                if (!unit.IsHostileWith(Owner))
                    continue;
                if (SpellEffect.MiscValueB == 1) //freez trap
                {
                    var spell = SpellHandler.Get(775);//frost 10 sec
                    unit.Auras.CreateAndStartAura(Owner.SharedReference, spell, false);
                }
                else if (SpellEffect.MiscValueB == 0) //damage trap
                {
                    var range = unit.GetDist(Owner);
                    var mult = 1f;
                    if (range >= 3)
                        mult = (float)(mult / System.Math.Pow(range, 0.6f));
                    var action = unit.DealSpellDamage(Owner, SpellEffect,
                                                      (int)(Owner.RandomDamage * SpellEffect.MiscValue / 100 * mult), true,
                                                      true, false, false);
                    if (action == null) continue;
                    if (m_aura != null)
                        WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(
                            m_aura.CasterUnit as Character, worldObject as Character, worldObject as NPC,
                            action.ActualDamage);

                    action.OnFinished();
                }
            }
            Aura.Cancel();
        }

        protected override void Remove(bool cancelled)
        {
        }
    }
}