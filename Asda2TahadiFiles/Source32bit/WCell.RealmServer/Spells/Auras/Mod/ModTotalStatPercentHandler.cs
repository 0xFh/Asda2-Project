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
      Character owner = Owner as Character;
      if(owner == null)
        return;
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
      Character owner = Owner as Character;
      if(owner == null)
        return;
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
}