using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class ChannelCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Channel");
            this.EnglishParamInfo = "<spellid>";
            this.EnglishDescription = "Channels the given Spell on the current Target.";
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            Unit target = trigger.Args.Target;
            if (target == null)
            {
                trigger.Reply("No valid caster");
            }
            else
            {
                Unit unit = target.Target;
                if (unit == null)
                {
                    unit = (Unit) trigger.Args.Character;
                    if (unit == null)
                    {
                        trigger.Reply("No valid target");
                        return;
                    }
                }

                target.CancelSpellCast();
                SpellId spellId = trigger.Text.NextEnum<SpellId>(SpellId.None);
                if (spellId == SpellId.None)
                {
                    if (target.ChannelSpell != SpellId.None)
                    {
                        target.ChannelSpell = spellId;
                        target.ChannelObject = (WorldObject) null;
                    }
                    else
                        trigger.Reply("Invalid SpellId.");
                }
                else
                {
                    target.ChannelSpell = spellId;
                    target.ChannelObject = (WorldObject) unit;
                }
            }
        }

        public override bool RequiresCharacter
        {
            get { return true; }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}