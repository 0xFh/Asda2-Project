using WCell.Constants.Items;

namespace WCell.RealmServer.Spells.Effects
{
    public class EnchantItemTemporaryEffectHandler : EnchantItemEffectHandler
    {
        public EnchantItemTemporaryEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override EnchantSlot EnchantSlot
        {
            get { return EnchantSlot.Temporary; }
        }
    }
}