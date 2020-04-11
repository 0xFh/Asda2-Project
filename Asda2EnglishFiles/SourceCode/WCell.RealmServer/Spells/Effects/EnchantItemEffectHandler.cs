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
            if (this.m_cast.TargetItem == null)
                return SpellFailedReason.ItemGone;
            if ((long) this.m_cast.TargetItem.Template.Level < (long) this.Effect.Spell.BaseLevel)
                return SpellFailedReason.TargetLowlevel;
            this.enchantEntry = EnchantMgr.GetEnchantmentEntry((uint) this.Effect.MiscValue);
            if (this.enchantEntry == null)
            {
                EnchantItemEffectHandler.log.Error("Spell {0} refers to invalid EnchantmentEntry {1}",
                    (object) this.Effect.Spell, (object) this.Effect.MiscValue);
                return SpellFailedReason.Error;
            }

            return !this.enchantEntry.CheckRequirements(this.m_cast.CasterUnit)
                ? SpellFailedReason.MinSkill
                : SpellFailedReason.Ok;
        }

        public virtual EnchantSlot EnchantSlot
        {
            get { return EnchantSlot.Permanent; }
        }

        public override void Apply()
        {
            Item targetItem = this.m_cast.TargetItem;
            int duration = this.CalcEffectValue();
            if (duration < 0)
                duration = 0;
            targetItem.ApplyEnchant(this.enchantEntry, this.EnchantSlot, duration, 0, true);
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