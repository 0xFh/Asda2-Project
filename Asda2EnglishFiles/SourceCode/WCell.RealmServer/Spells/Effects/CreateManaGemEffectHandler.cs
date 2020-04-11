using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class CreateManaGemEffectHandler : CreateItemEffectHandler
    {
        public CreateManaGemEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (this.Effect.BasePoints < 0)
                this.Effect.BasePoints = 0;
            return base.Initialize();
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            return SpellFailedReason.Ok;
        }
    }
}