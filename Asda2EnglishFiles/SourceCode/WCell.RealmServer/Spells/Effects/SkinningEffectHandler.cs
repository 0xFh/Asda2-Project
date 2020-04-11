using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.Spells.Effects
{
    public class SkinningEffectHandler : SpellEffectHandler
    {
        public SkinningEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            Unit unit = (Unit) target;
            if (!this.Cast.CasterChar.Skills.CheckSkill(SkillHandler.GetSkill((SkinningType) this.Effect.MiscValue),
                unit.Level * 5))
                return SpellFailedReason.TargetUnskinnable;
            return unit.Loot != null && !unit.Loot.IsReleased
                ? SpellFailedReason.TargetNotLooted
                : SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            Character casterChar = this.m_cast.CasterChar;
            Unit unit = (Unit) target;
            casterChar.Emote(EmoteType.SimpleTalk);
            unit.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 |
                              UnitFlags.SelectableNotAttackable | UnitFlags.Influenced | UnitFlags.PlayerControlled |
                              UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob |
                              UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable | UnitFlags.Passive |
                              UnitFlags.Looting | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                              UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                              UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat | UnitFlags.TaxiFlight |
                              UnitFlags.Disarmed | UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed |
                              UnitFlags.NotSelectable | UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                              UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 |
                              UnitFlags.Flag_31_0x80000000;
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}