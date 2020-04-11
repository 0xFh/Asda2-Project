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
      Character casterUnit = m_cast.CasterUnit as Character;
      if(casterUnit != null)
      {
        GameObject ownedGo = casterUnit.GetOwnedGO(Slot);
        if(ownedGo != null)
          ownedGo.Delete();
        base.Apply();
        GO.Entry.SummonSlotId = Slot;
        casterUnit.AddOwnedGO(GO);
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