namespace WCell.RealmServer.Spells.Effects
{
    public class SummonObjectSlot2Handler : SummonObjectSlot1Handler
    {
        public SummonObjectSlot2Handler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override uint Slot
        {
            get { return 2; }
        }
    }
}