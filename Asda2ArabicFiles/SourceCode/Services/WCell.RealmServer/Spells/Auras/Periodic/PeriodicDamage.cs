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

using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Periodically damages the holder
    /// </summary>
    public class PeriodicDamageHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var holder = Owner;
            if (holder.IsAlive)
            {
                var dmg = 1f;
                if (m_aura.CasterUnit == null)
                    dmg = 666;
                else
                {
                    if (m_aura.CasterUnit.Class == ClassId.AtackMage || m_aura.CasterUnit.Class == ClassId.SupportMage || m_aura.CasterUnit.Class == ClassId.HealMage)
                        dmg = m_aura.CasterUnit.RandomMagicDamage;
                    else
                        dmg = m_aura.CasterUnit.RandomDamage;
                }
                var value = SpellEffect.MiscValue * dmg / 550 * 3;
                if (m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
                {
                    var bonus = m_aura.Auras.GetBleedBonusPercent();
                    value += ((value * bonus) + 1) / 1;
                    m_aura.Owner.IncMechanicCount(SpellMechanic.Bleeding);
                }

                var da = holder.DealSpellDamage(m_aura.CasterUnit, m_spellEffect, (int)value, clearAction: false);
                if (da != null)
                {
                    WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character, Owner as Character, Owner as NPC, da.ActualDamage);
                    da.OnFinished();
                }
            }
        }
        protected override void Remove(bool cancelled)
        {
            if (m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
                m_aura.Owner.DecMechanicCount(SpellMechanic.Bleeding);
        }
    }

    public class ParameterizedPeriodicDamageHandler : PeriodicDamageHandler
    {
        public int TotalDamage { get; set; }

        public ParameterizedPeriodicDamageHandler()
            : this(0)
        {
        }

        public ParameterizedPeriodicDamageHandler(int totalDmg)
        {
            TotalDamage = totalDmg;
        }

        protected override void Apply()
        {
            BaseEffectValue = TotalDamage / (m_aura.TicksLeft + 1);
            TotalDamage -= BaseEffectValue;
        }
    }
}