using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>
  /// The caster requests a duel with the given target.
  /// Only used in: Duel (Id: 7266) (non-displayed Skill)
  /// </summary>
  public class DuelEffectHandler : SpellEffectHandler
  {
    public DuelEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      Character selectedTarget = m_cast.SelectedTarget as Character;
      if(selectedTarget != null)
        return Duel.CheckRequirements(m_cast.CasterChar, selectedTarget);
      return SpellFailedReason.Ok;
    }

    public override bool HasOwnTargets
    {
      get { return false; }
    }

    public override void Apply()
    {
      Duel.InitializeDuel(m_cast.CasterChar, m_cast.SelectedTarget as Character);
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Player; }
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Player; }
    }
  }
}