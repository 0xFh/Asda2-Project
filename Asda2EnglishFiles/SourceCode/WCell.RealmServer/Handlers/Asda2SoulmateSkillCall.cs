using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2SoulmateSkillCall : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillCall()
            : base(Asda2SoulmateSkillId.Call, (byte) 3, 1200)
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
            Asda2SoulmateHandler.SendSoulmateSummoningYouResponse(caster, friend);
            base.Action(caster, friend);
        }
    }
}