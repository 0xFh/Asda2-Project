using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>Forces target to wander around.</summary>
  public class ModConfuseHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(Owner is Character)
        ((Character) Owner).SetMover(Owner, false);
      else if(Owner is NPC)
        Owner.Brain.State = BrainState.Idle;
      Owner.IsInfluenced = true;
    }

    protected override void Remove(bool cancelled)
    {
      if(Owner is Character)
        (Owner as Character).SetMover(Owner, true);
      else if(Owner is NPC)
        (Owner as NPC).Brain.EnterDefaultState();
      Owner.IsInfluenced = false;
    }
  }
}