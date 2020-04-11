using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public abstract class Asda2SoulmateSkill
    {
        public virtual bool TryCast(Character caster, Character friend)
        {
            if ((int) caster.SoulmateRecord.Level < (int) this.Level || DateTime.Now < this.ReadyTime)
                return false;
            this.Action(caster, friend);
            this.ReadyTime = DateTime.Now.AddSeconds((double) this.CooldownTimeSecs);
            return true;
        }

        protected int CooldownTimeSecs { get; set; }

        public DateTime ReadyTime { get; set; }

        public Asda2SoulmateSkillId Id { get; set; }

        public byte Level { get; set; }

        public virtual void Action(Character caster, Character friend)
        {
        }

        protected Asda2SoulmateSkill(Asda2SoulmateSkillId id, byte level, int cooldownTimeSecs)
        {
            this.Id = id;
            this.Level = level;
            this.CooldownTimeSecs = cooldownTimeSecs;
            this.ReadyTime = DateTime.MinValue;
        }
    }
}