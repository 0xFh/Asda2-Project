using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Passive
{
    public class ForceAutoRunForwardHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.UnitFlags2 |= UnitFlags2.ForceAutoRunForward;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.UnitFlags2 &= ~UnitFlags2.ForceAutoRunForward;
        }
    }
}