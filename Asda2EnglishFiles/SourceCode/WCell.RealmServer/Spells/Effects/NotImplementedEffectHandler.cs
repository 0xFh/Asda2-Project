using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class NotImplementedEffectHandler : SpellEffectHandler
    {
        public NotImplementedEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
            if (!(cast.CasterObject is Character))
                return;
            (cast.CasterObject as Character).SendSystemMessage(
                "Spell {0} ({1}) has not implemented Effect {2}. Please report this to the developers",
                (object) cast.Spell.Name, (object) cast.Spell.Id, (object) effect.EffectType);
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public override void Apply()
        {
        }
    }
}