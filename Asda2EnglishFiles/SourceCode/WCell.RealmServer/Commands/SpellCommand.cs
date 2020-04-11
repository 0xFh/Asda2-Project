using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class SpellCommand : RealmServerCommand
    {
        protected SpellCommand()
        {
        }

        protected override void Initialize()
        {
            this.Init("Spell", "Spells", "Sp");
            this.Description = new TranslatableItem(RealmLangKey.CmdSpellDescription, new object[0]);
        }

        public class SpellAddCommand : RealmServerCommand.SubCommand
        {
            public static SpellCommand.SpellAddCommand Instance { get; private set; }

            protected SpellAddCommand()
            {
                SpellCommand.SpellAddCommand.Instance = this;
            }

            protected override void Initialize()
            {
                this.Init("Add", "A");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdSpellAddParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdSpellAddDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string str = trigger.Text.NextModifiers();
                Unit target = trigger.Args.Target;
                if (target == null)
                    return;
                if (str.Length > 0 && (str.Length != 1 || !str.Contains("r")))
                {
                    if (str.Contains("c"))
                    {
                        ClassId clss;
                        if (trigger.Text.HasNext)
                        {
                            clss = trigger.Text.NextEnum<ClassId>(ClassId.End);
                            if (clss == ClassId.End)
                            {
                                trigger.Reply(RealmLangKey.InvalidClass);
                                return;
                            }
                        }
                        else
                            clss = target.Class;

                        int num = 0;
                        foreach (SpellLine line in SpellLines.GetLines(clss))
                        {
                            if (line.HighestRank.Talent == null)
                            {
                                SpellCommand.SpellAddCommand.AddSpell(target, line.HighestRank, str.Contains("r"));
                                ++num;
                            }
                        }

                        if (num > 0)
                            trigger.Reply(RealmLangKey.CmdSpellAddResponseSpells, (object) num);
                    }

                    if (!str.Contains("t"))
                        return;
                    int num1 = 0;
                    foreach (SpellLine line in SpellLines.GetLines(target.Class))
                    {
                        if (line.HighestRank.Talent != null)
                        {
                            SpellCommand.SpellAddCommand.AddSpell(target, line.HighestRank, str.Contains("r"));
                            ++num1;
                        }
                    }

                    trigger.Reply(RealmLangKey.CmdSpellAddResponseTalents, (object) num1);
                }
                else
                {
                    Spell[] spellArray = SpellGetCommand.RetrieveSpells(trigger);
                    if (spellArray.Length == 0)
                    {
                        trigger.Reply(RealmLangKey.CmdSpellNotExists);
                    }
                    else
                    {
                        foreach (Spell spell in spellArray)
                        {
                            SpellCommand.SpellAddCommand.AddSpell(target, spell, str.Contains("r"));
                            trigger.Reply(RealmLangKey.CmdSpellAddResponseSpell, (object) spell);
                        }
                    }
                }
            }

            private static void AddSpell(Unit target, Spell spell, bool addRequired)
            {
                Character character = target as Character;
                if (addRequired && character != null)
                    character.PlayerSpells.AddSpellRequirements(spell);
                if (spell.Talent != null && character != null)
                    character.Talents.Set(spell.Talent, spell.Line.SpellCount - 1);
                else
                    target.Spells.AddSpell(spell);
            }
        }

        public class RemoveSpellCommand : RealmServerCommand.SubCommand
        {
            protected RemoveSpellCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Remove", "R");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdSpellRemoveParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdSpellRemoveDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Spell[] spellArray = SpellGetCommand.RetrieveSpells(trigger);
                Unit target = trigger.Args.Target;
                if (target == null)
                    return;
                if (spellArray.Length > 0)
                {
                    foreach (Spell spell in spellArray)
                    {
                        if (trigger.Args.Target.HasSpells)
                        {
                            if (spell.Talent != null)
                                target.Talents.Remove(spell.Talent.Id);
                            else
                                target.Spells.Remove(spell);
                            trigger.Reply(RealmLangKey.CmdSpellRemoveResponse, (object) spell);
                        }
                    }
                }
                else
                    trigger.Reply(RealmLangKey.CmdSpellRemoveError);
            }
        }

        public class PurgeSpellsCommand : RealmServerCommand.SubCommand
        {
            protected PurgeSpellsCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Clear", "Purge");
                this.Description = new TranslatableItem(RealmLangKey.CmdSpellPurgeDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (target == null)
                    return;
                if (target.HasSpells)
                {
                    target.Spells.Clear();
                    target.Spells.AddDefaultSpells();
                    trigger.Reply(RealmLangKey.CmdSpellPurgeResponse);
                }
                else
                    trigger.Reply(RealmLangKey.CmdSpellPurgeError);
            }
        }

        public class SpellTriggerCommand : RealmServerCommand.SubCommand
        {
            protected SpellTriggerCommand()
            {
            }

            protected override void Initialize()
            {
                this.Init("Trigger", "T");
                this.ParamInfo = new TranslatableItem(RealmLangKey.CmdSpellTriggerParamInfo, new object[0]);
                this.Description = new TranslatableItem(RealmLangKey.CmdSpellTriggerDescription, new object[0]);
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Spell[] spellArray = SpellGetCommand.RetrieveSpells(trigger);
                Unit target = trigger.Args.Target;
                if (target == null)
                    return;
                if (spellArray.Length > 0)
                {
                    foreach (Spell spell in spellArray)
                    {
                        target.SpellCast.TriggerSelf(spell);
                        trigger.Reply(RealmLangKey.CmdSpellTriggerResponse, (object) spell);
                    }
                }
                else
                    trigger.Reply(RealmLangKey.CmdSpellTriggerError);
            }
        }
    }
}