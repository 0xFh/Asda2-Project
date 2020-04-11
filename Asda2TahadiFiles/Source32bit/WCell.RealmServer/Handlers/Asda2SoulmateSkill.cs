using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
  public abstract class Asda2SoulmateSkill
  {
    public virtual bool TryCast(Character caster, Character friend)
    {
      if(caster.SoulmateRecord.Level < Level || DateTime.Now < ReadyTime)
        return false;
      Action(caster, friend);
      ReadyTime = DateTime.Now.AddSeconds(CooldownTimeSecs);
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
      Id = id;
      Level = level;
      CooldownTimeSecs = cooldownTimeSecs;
      ReadyTime = DateTime.MinValue;
    }
  }
}