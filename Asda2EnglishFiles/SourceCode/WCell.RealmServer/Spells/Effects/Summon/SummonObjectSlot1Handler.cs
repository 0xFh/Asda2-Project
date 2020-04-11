using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class SummonObjectSlot1Handler : SummonObjectEffectHandler
    {
        public SummonObjectSlot1Handler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            Character casterUnit = this.m_cast.CasterUnit as Character;
            if (casterUnit != null)
            {
                GameObject ownedGo = casterUnit.GetOwnedGO(this.Slot);
                if (ownedGo != null)
                    ownedGo.Delete();
                base.Apply();
                this.GO.Entry.SummonSlotId = this.Slot;
                casterUnit.AddOwnedGO(this.GO);
            }
            else
                base.Apply();
        }

        public virtual uint Slot
        {
            get { return 1; }
        }
    }
}