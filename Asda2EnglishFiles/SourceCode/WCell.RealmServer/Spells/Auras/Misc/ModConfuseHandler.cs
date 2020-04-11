using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>Forces target to wander around.</summary>
    public class ModConfuseHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.Owner is Character)
                ((Character) this.Owner).SetMover((WorldObject) this.Owner, false);
            else if (this.Owner is NPC)
                this.Owner.Brain.State = BrainState.Idle;
            this.Owner.IsInfluenced = true;
        }

        protected override void Remove(bool cancelled)
        {
            if (this.Owner is Character)
                (this.Owner as Character).SetMover((WorldObject) this.Owner, true);
            else if (this.Owner is NPC)
                (this.Owner as NPC).Brain.EnterDefaultState();
            this.Owner.IsInfluenced = false;
        }
    }
}