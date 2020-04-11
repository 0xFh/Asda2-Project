using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SpellVisualCommand : RealmServerCommand
    {
        protected SpellVisualCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("SpellVisual", "PlaySpellVisual", "SpellAnim");
            this.ParamInfo = new TranslatableItem(RealmLangKey.CmdSpellVisualParamInfo, new object[0]);
            this.Description = new TranslatableItem(RealmLangKey.CmdSpellVisualDescription, new object[0]);
        }

        public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
        {
            SpellId spellId = trigger.Text.NextEnum<SpellId>(SpellId.None);
            Spell spell = SpellHandler.Get(spellId);
            if (spell == null)
            {
                trigger.Reply(RealmLangKey.CmdSpellVisualError, (object) spellId);
            }
            else
            {
                uint visual = spell.Visual;
                SpellHandler.SendVisual((WorldObject) trigger.Args.Target, visual);
            }
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.All; }
        }
    }
}