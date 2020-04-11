namespace WCell.RealmServer.Spells.Effects
{
    public class SummonObjectSlot3Handler : SummonObjectSlot1Handler
    {
        public SummonObjectSlot3Handler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override uint Slot
        {
            get { return 3; }
        }
    }
}