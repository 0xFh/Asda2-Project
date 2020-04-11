using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells
{
    public struct AuraApplicationInfo
    {
        public readonly Unit Target;
        public readonly List<AuraEffectHandler> Handlers;

        public AuraApplicationInfo(Unit target)
        {
            this.Target = target;
            this.Handlers = new List<AuraEffectHandler>(3);
        }

        public AuraApplicationInfo(Unit target, AuraEffectHandler firstHandler)
        {
            this.Target = target;
            this.Handlers = new List<AuraEffectHandler>(3)
            {
                firstHandler
            };
        }
    }
}