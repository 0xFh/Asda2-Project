using NLog;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Spells.Effects
{
  public class EnchantItemEffectHandler : SpellEffectHandler
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private ItemEnchantmentEntry enchantEntry;

    public EnchantItemEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      if(m_cast.TargetItem == null)
        return SpellFailedReason.ItemGone;
      if(m_cast.TargetItem.Template.Level < Effect.Spell.BaseLevel)
        return SpellFailedReason.TargetLowlevel;
      enchantEntry = EnchantMgr.GetEnchantmentEntry((uint) Effect.MiscValue);
      if(enchantEntry == null)
      {
        log.Error("Spell {0} refers to invalid EnchantmentEntry {1}",
          Effect.Spell, Effect.MiscValue);
        return SpellFailedReason.Error;
      }

      return !enchantEntry.CheckRequirements(m_cast.CasterUnit)
        ? SpellFailedReason.MinSkill
        : SpellFailedReason.Ok;
    }

    public virtual EnchantSlot EnchantSlot
    {
      get { return EnchantSlot.Permanent; }
    }

    public override void Apply()
    {
      Item targetItem = m_cast.TargetItem;
      int duration = CalcEffectValue();
      if(duration < 0)
        duration = 0;
      targetItem.ApplyEnchant(enchantEntry, EnchantSlot, duration, 0, true);
    }

    public override bool HasOwnTargets
    {
      get { return false; }
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}