using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>Summons the current pet or a custom one</summary>
  public class SummonPetEffectHandler : SummonEffectHandler
  {
    private bool _ownedPet;

    public SummonPetEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      _ownedPet = Effect.MiscValue == 0 && m_cast.CasterObject is Character;
      if(_ownedPet && ((Character) m_cast.CasterObject).ActivePet == null)
        return SpellFailedReason.NoPet;
      return base.Initialize();
    }

    public override SummonType SummonType
    {
      get { return SummonType.SummonPet; }
    }

    public override void Apply()
    {
      if(_ownedPet)
        ((Character) m_cast.CasterObject).IsPetActive = true;
      else
        Summon(SpellHandler.GetSummonEntry(SummonType));
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Unit; }
    }
  }
}