using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Modifiers;

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
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.ApplyStatMod(ItemModType.AgilityPrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.LuckPrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.IntelectPrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.StaminaPrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.StrengthPrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.EnergyPrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.DamagePrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.MagicDamagePrc, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.Health, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.Power, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.Speed, this.SpellEffect.MiscValue);
            owner.ApplyStatMod(ItemModType.AtackTimePrc, -this.SpellEffect.MiscValue);
            Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.RemoveStatMod(ItemModType.AgilityPrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.LuckPrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.IntelectPrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.StaminaPrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.StrengthPrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.EnergyPrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.DamagePrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.MagicDamagePrc, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.Health, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.Power, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.Speed, this.SpellEffect.MiscValue);
            owner.RemoveStatMod(ItemModType.AtackTimePrc, -this.SpellEffect.MiscValue);
            Asda2CharacterHandler.SendUpdateStatsOneResponse(owner.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(owner.Client);
        }
    }
}