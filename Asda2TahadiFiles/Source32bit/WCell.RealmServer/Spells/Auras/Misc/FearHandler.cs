using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>Forces target to run away in fear.</summary>
  public class FearHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(Owner.IsInfluenced)
        return;
      if(Owner is Character)
      {
        Character owner = (Character) Owner;
        owner.SetMover(Owner, false);
        owner.SpeedFactor *= 0.5f;
        Asda2MovmentHandler.OnMoveRequest(owner.Client, owner.Asda2Y + Utility.Random(-10, 10),
          owner.Asda2X + Utility.Random(-10, 10));
      }
      else if(Owner is NPC)
        m_aura.Auras.Owner.Brain.State = BrainState.Fear;

      Owner.IsInfluenced = true;
    }

    protected override void Remove(bool cancelled)
    {
      if(Owner is Character)
      {
        Character owner = Owner as Character;
        owner.UpdateSpeedFactor();
        owner.SetMover(Owner, true);
      }
      else if(Owner is NPC)
        m_aura.Auras.Owner.Brain.State = BrainState.Combat;

      Owner.IsInfluenced = false;
    }
  }
}