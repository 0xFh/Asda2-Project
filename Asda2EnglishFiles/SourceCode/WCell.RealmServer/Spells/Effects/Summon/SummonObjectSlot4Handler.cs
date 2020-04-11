namespace WCell.RealmServer.Spells.Effects
{
    public class SummonObjectSlot4Handler : SummonObjectSlot1Handler
    {
        public SummonObjectSlot4Handler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override uint Slot
        {
            get { return 4; }
        }
    }
}