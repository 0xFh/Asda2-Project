using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Tames the target</summary>
    public class TameCreatureEffectHandler : SpellEffectHandler
    {
        public TameCreatureEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            if (!(target is NPC))
                return SpellFailedReason.BadTargets;
            return SpellCast.CheckTame(this.m_cast.CasterObject as Character, (NPC) target) != TameFailReason.Ok
                ? SpellFailedReason.DontReport
                : SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            Unit casterObject = (Unit) this.m_cast.CasterObject;
            if (casterObject is Character)
            {
                ((Character) casterObject).MakePet((NPC) target);
            }
            else
            {
                casterObject.Enslave((NPC) target);
                ((Unit) target).Summoner = casterObject;
                ((Unit) target).Creator = casterObject.EntityId;
            }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}