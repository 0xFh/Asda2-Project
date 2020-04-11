/*************************************************************************
 *
 *   file		: ModIncreaseEnergyPercent.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 14:58:12 +0800 (Sat, 07 Mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Modifiers;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Same as ModStatPercent, but including item bonuses
	/// TODO: Include item bonuses
	/// </summary>
	public class ModTotalStatPercentHandler : ModStatPercentHandler
	{
        protected override void Apply()
        {
            var owner = Owner as Character;
            if (owner == null) return;
            owner.ApplyStatMod(ItemModType.AgilityPrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.LuckPrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.IntelectPrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.StaminaPrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.StrengthPrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.EnergyPrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.DamagePrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.MagicDamagePrc, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.Health, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.Power, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.Speed, SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.AtackTimePrc, -SpellEffect.MiscValue);
            Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
        }

        protected override void Remove(bool cancelled)
        {
            var owner = Owner as Character;
            if (owner == null) return;
            owner.RemoveStatMod(ItemModType.AgilityPrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.LuckPrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.IntelectPrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.StaminaPrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.StrengthPrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.EnergyPrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.DamagePrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.MagicDamagePrc, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.Health, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.Power, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.Speed, SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.AtackTimePrc, -SpellEffect.MiscValue);
            Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
        }
	}
};