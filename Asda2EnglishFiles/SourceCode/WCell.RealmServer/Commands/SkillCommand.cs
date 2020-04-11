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
            this.Init("Skill", "Skills", "Sk");
            this.Description = new TranslatableItem(RealmLangKey.CmdSkillDescription, new object[0]);
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Player; }
        }

        public class SetCommand : RealmServerCommand.SubCommand
        {
            protected SetCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Set", "S");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdSkillSetParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdSkillSetDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                SkillId id = trigger.Text.NextEnum<SkillId>(SkillId.None);
                SkillLine skillLine = SkillHandler.Get(id);
                if (skillLine != null)
                {
                    Character target = (Character) trigger.Args.Target;
                    int num = trigger.Text.NextInt(1);
                    SkillTierId tierForLevel = skillLine.GetTierForLevel(num);
                    target.Skills.GetOrCreate(id, true).CurrentValue = (ushort) num;
                    Spell spellForTier = skillLine.GetSpellForTier(tierForLevel);
                    if (spellForTier != null)
                        target.Spells.AddSpell(spellForTier);
                    trigger.Reply(RealmLangKey.CmdSkillSetResponse, (object) skillLine, (object) num,
                        (object) tierForLevel);
                }
                else
                    trigger.Reply(RealmLangKey.CmdSkillSetError, (object) id);
            }
        }

        public class TierCommand : RealmServerCommand.SubCommand
        {
            protected TierCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Tier", "SetTier", "ST");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdSkillTierParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdSkillTierDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                SkillId id = trigger.Text.NextEnum<SkillId>(SkillId.None);
                if (trigger.Text.HasNext)
                {
                    SkillTierId tier = trigger.Text.NextEnum<SkillTierId>(SkillTierId.GrandMaster);
                    if (SkillHandler.Get(id) != null)
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