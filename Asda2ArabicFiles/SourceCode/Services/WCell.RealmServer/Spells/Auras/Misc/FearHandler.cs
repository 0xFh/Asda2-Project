using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>
    /// Forces target to run away in fear.
    /// </summary>
    public class FearHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if(Owner.IsInfluenced)
                return;
            if (Owner is Character)
            {
                var chr = (Character)Owner;
                chr.SetMover(Owner, false);
                chr.SpeedFactor = chr.SpeedFactor*0.5f;

                WCell.RealmServer.Handlers.Asda2MovmentHandler.OnMoveRequest(chr.Client, chr.Asda2Y + Util.Utility.Random(-10, 10), chr.Asda2X + Util.Utility.Random(-10, 10));
                
            }
            else if (Owner is NPC)
            {
                m_aura.Auras.Owner.Brain.State = BrainState.Fear;
                //m_aura.Auras.Owner.IncMechanicCount(Constants.Spells.SpellMechanic.Stunned);
            }

            // TODO: Make unit run away instead of being stuck.
            Owner.IsInfluenced = true;
            //
        }

        protected override void Remove(bool cancelled)
        {
            if (Owner is Character)
            {
                var chr = Owner as Character;
                chr.UpdateSpeedFactor();
                chr.SetMover(Owner, true);
            }
            else if (Owner is NPC)
            {
                m_aura.Auras.Owner.Brain.State = BrainState.Combat;
                //m_aura.Auras.Owner.DecMechanicCount(Constants.Spells.SpellMechanic.Stunned);
            }

            Owner.IsInfluenced = false;
            //
        }
    }
}
