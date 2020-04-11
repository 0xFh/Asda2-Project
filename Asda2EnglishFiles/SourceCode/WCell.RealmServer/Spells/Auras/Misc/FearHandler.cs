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
            if (this.Owner.IsInfluenced)
                return;
            if (this.Owner is Character)
            {
                Character owner = (Character) this.Owner;
                owner.SetMover((WorldObject) this.Owner, false);
                owner.SpeedFactor *= 0.5f;
                Asda2MovmentHandler.OnMoveRequest(owner.Client, owner.Asda2Y + (float) Utility.Random(-10, 10),
                    owner.Asda2X + (float) Utility.Random(-10, 10));
            }
            else if (this.Owner is NPC)
                this.m_aura.Auras.Owner.Brain.State = BrainState.Fear;

            this.Owner.IsInfluenced = true;
        }

        protected override void Remove(bool cancelled)
        {
            if (this.Owner is Character)
            {
                Character owner = this.Owner as Character;
                owner.UpdateSpeedFactor();
                owner.SetMover((WorldObject) this.Owner, true);
            }
            else if (this.Owner is NPC)
                this.m_aura.Auras.Owner.Brain.State = BrainState.Combat;

            this.Owner.IsInfluenced = false;
        }
    }
}