using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2SoulmateSkillTeleport : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillTeleport()
            : base(Asda2SoulmateSkillId.Call, (byte) 20, 1200)
        {
        }

        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.IsAsda2BattlegroundInProgress || friend.IsAsda2BattlegroundInProgress)
                return false;
            return base.TryCast(caster, friend);
        }

        public override void Action(Character caster, Character friend)
        {
            caster.TeleportTo((IWorldLocation) friend);
            base.Action(caster, friend);
        }
    }
}