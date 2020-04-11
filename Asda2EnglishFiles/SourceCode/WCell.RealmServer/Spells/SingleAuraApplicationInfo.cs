using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells
{
    public struct SingleAuraApplicationInfo
    {
        public readonly Unit Target;
        public readonly AuraEffectHandler Handler;

        public SingleAuraApplicationInfo(Unit target, AuraEffectHandler handler)
        {
            this.Target = target;
            this.Handler = handler;
        }
    }
}