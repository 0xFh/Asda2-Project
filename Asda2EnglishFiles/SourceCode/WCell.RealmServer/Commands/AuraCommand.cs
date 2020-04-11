using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class AuraCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Aura", "Auras");
            this.EnglishDescription = "Provides commands to manage Auras (ie Buffs, Debuffs, passive effects).";
        }

        public override ObjectTypeCustom TargetTypes
        {
            get { return ObjectTypeCustom.Unit; }
        }

        public class ClearAurasCommand : RealmServerCommand.SubCommand
        {
            protected ClearAurasCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Clear");
                this.EnglishDescription = "Removes all visible Auras";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                trigger.Args.Target.Auras.ClearVisibleAuras();
                trigger.Reply("All visible Auras removed.");
            }
        }

        public class AddAuraCommand : RealmServerCommand.SubCommand
        {
            protected AddAuraCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Add");
                this.EnglishParamInfo = "<spell1>[, <spell2> ... ]";
                this.EnglishDescription = "Adds the given spells as Aura on the target";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Spell[] spellArray = SpellGetCommand.RetrieveSpells(trigger);
                if (spellArray.Length == 0)
                {
                    trigger.Reply(RealmLangKey.CmdSpellNotExists);
                }
                else
                {
                    foreach (Spell spell in spellArray)
                        trigger.Args.Target.Auras.CreateSelf(spell, false);
                    trigger.Reply("Added {0} Auras.", (object) spellArray.Length);
                }
            }
        }

        public class DumpAurasCommand : RealmServerCommand.SubCommand
        {
            protected DumpAurasCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Dump");
                this.EnglishParamInfo = "[<alsoPassive>]";
                this.EnglishDescription =
                    "Dumps all currently active Auras, also shows passive effects if second param is specified";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                AuraCollection auras = trigger.Args.Target.Auras;
                if (auras != null && auras.Count > 0)
                {
                    bool flag = trigger.Text.NextBool();
                    trigger.Reply("{0}'s Auras:", (object) auras.Owner.Name);
                    foreach (Aura aura in auras)
                    {
                        if (flag || !aura.Spell.IsPassive)
                            trigger.Reply("\t{0}{1}", (object) aura.Spell,
                                aura.HasTimeout
                                    ? (object) (" [" + TimeSpan.FromMilliseconds((double) aura.TimeLeft).Format() + "]")
                                    : (object) "");
                    }
                }
                else
                    trigger.Reply("{0} has no active Auras.", (object) trigger.Args.Target.Name);
            }
        }
    }
}