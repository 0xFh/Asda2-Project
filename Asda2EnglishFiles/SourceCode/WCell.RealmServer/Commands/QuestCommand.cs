using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
    public class QuestCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Quest");
            this.EnglishParamInfo = "";
            this.EnglishDescription = "Provides a set of commands to dynamically change status of Quests and more.";
        }

        public class ResetQuestCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Reset", "Start");
                this.EnglishParamInfo = "<questid>";
                this.EnglishDescription = "Removes all progress of the given Quest (if present) and starts it (again).";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (!(target is Character))
                {
                    trigger.Reply("Invalid target: {0} - Character-target required.", (object) target);
                }
                else
                {
                    Character character = (Character) target;
                    uint id = trigger.Text.NextUInt(0U);
                    QuestTemplate qt = (QuestTemplate) null;
                    if (id > 0U)
                        qt = QuestMgr.GetTemplate(id);
                    if (qt == null)
                    {
                        trigger.Reply("Invalid QuestId: {0}", (object) id);
                    }
                    else
                    {
                        if (!character.QuestLog.RemoveFinishedQuest(id))
                            character.QuestLog.Cancel(id);
                        if (character.QuestLog.AddQuest(qt) == null)
                            trigger.Reply("Could not add Quest: " + (object) qt);
                        else
                            trigger.Reply("Quest added: " + (object) qt);
                    }
                }
            }
        }

        public class CancelQuestCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("Remove", "Cancel");
                this.EnglishParamInfo = "<questid>";
                this.EnglishDescription = "Removes the given finished or active Quest.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (!(target is Character))
                {
                    trigger.Reply("Invalid target: {0} - Character-target required.", (object) target);
                }
                else
                {
                    Character character = (Character) target;
                    uint id = trigger.Text.NextUInt(0U);
                    QuestTemplate questTemplate = (QuestTemplate) null;
                    if (id > 0U)
                        questTemplate = QuestMgr.GetTemplate(id);
                    if (questTemplate == null)
                        trigger.Reply("Invalid QuestId: {0}", (object) id);
                    else if (!character.QuestLog.RemoveFinishedQuest(id))
                    {
                        character.QuestLog.Cancel(id);
                        trigger.Reply("Removed active quest: {0}", (object) questTemplate);
                    }
                    else
                        trigger.Reply("Removed finished quest: {0}", (object) questTemplate);
                }
            }
        }

        public class GiveQuestRewardCommand : RealmServerCommand.SubCommand
        {
            protected override void Initialize()
            {
                this.Init("GiveReward", "Reward");
                this.EnglishParamInfo = "<questid> [<choiceSlot>]";
                this.EnglishDescription =
                    "Gives the reward of the given quest to the Character. The optional choiceSlot determines the choosable item (if any)";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (!(target is Character))
                {
                    trigger.Reply("Invalid target: {0} - Character-target required.", (object) target);
                }
                else
                {
                    Character receiver = (Character) target;
                    uint id = trigger.Text.NextUInt(0U);
                    uint rewardSlot = trigger.Text.NextUInt(0U);
                    QuestTemplate questTemplate = (QuestTemplate) null;
                    if (id > 0U)
                        questTemplate = QuestMgr.GetTemplate(id);
                    if (questTemplate == null)
                    {
                        trigger.Reply("Invalid QuestId: {0}", (object) id);
                    }
                    else
                    {
                        questTemplate.GiveRewards(receiver, rewardSlot);
                        trigger.Reply("Done.");
                    }
                }
            }
        }

        public abstract class QuestSubCmd : RealmServerCommand.SubCommand
        {
            public QuestTemplate GetTemplate(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint questId = this.GetQuestId(trigger);
                if (questId == 0U)
                    return (QuestTemplate) null;
                QuestTemplate template = QuestMgr.GetTemplate(questId);
                if (template == null)
                    trigger.Reply("Invalid Id - Use: 'Quest Lookup <search term>' to find quest-ids.");
                return template;
            }

            public uint GetQuestId(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                uint num = trigger.Text.NextUInt(0U);
                if (num != 0U)
                    return num;
                trigger.Reply("Invalid Id - Use: 'Quest Lookup <search term>' to find quest-ids.");
                return 0;
            }
        }

        public class QuestGotoCommand : QuestCommand.QuestSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Goto");
                this.EnglishParamInfo = "<id>[ <starter index>[ <template index>]";
                this.EnglishDescription =
                    "Teleports the target to the first starter of the given quest or the one at the given index.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                Unit target = trigger.Args.Target;
                if (target == null)
                {
                    trigger.Reply("No target given.");
                }
                else
                {
                    QuestTemplate template = this.GetTemplate(trigger);
                    if (template == null)
                        return;
                    List<IQuestHolderEntry> starters = template.Starters;
                    if (starters.Count == 0)
                    {
                        trigger.Reply("Quest {0} has no Starters.", (object) template);
                    }
                    else
                    {
                        trigger.Reply("Found {0} Starters: " + starters.ToString<IQuestHolderEntry>(", "),
                            (object) starters.Count);
                        int index1;
                        if (trigger.Text.HasNext)
                        {
                            index1 = trigger.Text.NextInt(-1);
                            if (index1 < 0 || index1 >= starters.Count)
                            {
                                trigger.Reply("Invalid starter-index.");
                                return;
                            }
                        }
                        else
                            index1 = 0;

                        IQuestHolderEntry questHolderEntry = starters[index1];
                        IWorldLocation[] inWorldTemplates = questHolderEntry.GetInWorldTemplates();
                        if (inWorldTemplates == null)
                        {
                            trigger.Reply("Quest starters are not accessible.");
                        }
                        else
                        {
                            trigger.Reply(
                                "Found {0} templates: " + ((IEnumerable<IWorldLocation>) inWorldTemplates)
                                .ToString<IWorldLocation>(", "), (object) inWorldTemplates.Length);
                            int index2;
                            if (trigger.Text.HasNext)
                            {
                                index2 = trigger.Text.NextInt(-1);
                                if (index2 < 0 || index2 >= inWorldTemplates.Length)
                                {
                                    trigger.Reply("Invalid template-index.");
                                    return;
                                }
                            }
                            else
                                index2 = 0;

                            IWorldLocation location = inWorldTemplates[index2];
                            if (target.TeleportTo(location))
                            {
                                if (inWorldTemplates.Length <= 1 && starters.Count <= 1)
                                    return;
                                trigger.Reply("Going to {0} ({1})", (object) questHolderEntry, (object) location);
                            }
                            else
                                trigger.Reply("Template is located in {0} ({1}) and not accessible.",
                                    (object) location.MapId, (object) location.Position);
                        }
                    }
                }
            }
        }

        public class QuestLookupCommand : QuestCommand.QuestSubCmd
        {
            protected override void Initialize()
            {
                this.Init("Lookup", "Find");
                this.EnglishParamInfo = "<search terms>";
                this.EnglishDescription = "Lists all quests matching the given search term.";
            }

            public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
            {
                string[] searchTerms = trigger.Text.Remainder.Split(new char[1]
                {
                    ' '
                }, StringSplitOptions.RemoveEmptyEntries);
                IEnumerable<QuestTemplate> source =
                    ((IEnumerable<QuestTemplate>) QuestMgr.Templates).Where<QuestTemplate>(
                        (Func<QuestTemplate, bool>) (templ =>
                        {
                            if (templ != null)
                                return ((IEnumerable<string>) searchTerms).Iterate<string>(
                                    (Func<string, bool>) (term => !templ.DefaultTitle.ContainsIgnoreCase(term)));
                            return false;
                        }));
                int num1 = source.Count<QuestTemplate>();
                trigger.Reply("Found {0} matching Quests.", (object) num1);
                int num2 = 100;
                if (num1 > num2)
                {
                    trigger.Reply("Cannot display more than " + (object) num2 + " matches at a time.");
                }
                else
                {
                    foreach (QuestTemplate questTemplate in source)
                        trigger.Reply(questTemplate.ToString());
                }
            }
        }
    }
}