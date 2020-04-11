using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
  public class SkillCommand : RealmServerCommand
  {
    protected SkillCommand()
    {
    }

    protected override void Initialize()
    {
      Init("Skill", "Skills", "Sk");
      Description = new TranslatableItem(RealmLangKey.CmdSkillDescription);
    }

    public override ObjectTypeCustom TargetTypes
    {
      get { return ObjectTypeCustom.Player; }
    }

    public class SetCommand : SubCommand
    {
      protected SetCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Set", "S");
        ParamInfo = new TranslatableItem(RealmLangKey.CmdSkillSetParamInfo);
        Description = new TranslatableItem(RealmLangKey.CmdSkillSetDescription);
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        SkillId id = trigger.Text.NextEnum(SkillId.None);
        SkillLine skillLine = SkillHandler.Get(id);
        if(skillLine != null)
        {
          Character target = (Character) trigger.Args.Target;
          int num = trigger.Text.NextInt(1);
          SkillTierId tierForLevel = skillLine.GetTierForLevel(num);
          target.Skills.GetOrCreate(id, true).CurrentValue = (ushort) num;
          Spell spellForTier = skillLine.GetSpellForTier(tierForLevel);
          if(spellForTier != null)
            target.Spells.AddSpell(spellForTier);
          trigger.Reply(RealmLangKey.CmdSkillSetResponse, (object) skillLine, (object) num,
            (object) tierForLevel);
        }
        else
          trigger.Reply(RealmLangKey.CmdSkillSetError, (object) id);
      }
    }

    public class TierCommand : SubCommand
    {
      protected TierCommand()
      {
      }

      protected override void Initialize()
      {
        Init("Tier", "SetTier", "ST");
        ParamInfo = new TranslatableItem(RealmLangKey.CmdSkillTierParamInfo);
        Description = new TranslatableItem(RealmLangKey.CmdSkillTierDescription);
      }

      public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
      {
        SkillId id = trigger.Text.NextEnum(SkillId.None);
        if(trigger.Text.HasNext)
        {
          SkillTierId tier = trigger.Text.NextEnum(SkillTierId.GrandMaster);
          if(SkillHandler.Get(id) != null)
          {
            Skill skill = ((Character) trigger.Args.Target).Skills.GetOrCreate(id, tier, true);
            trigger.Reply(RealmLangKey.CmdSkillTierResponse, (object) skill, (object) skill.CurrentValue,
              (object) skill.MaxValue);
          }
          else
            trigger.Reply(RealmLangKey.CmdSkillTierError1, (object) id);
        }
        else
          trigger.Reply(RealmLangKey.CmdSkillTierError2);
      }
    }
  }
}