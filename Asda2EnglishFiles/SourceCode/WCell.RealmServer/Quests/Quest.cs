using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Achievements;
using WCell.Constants.NPCs;
using WCell.Constants.Quests;
using WCell.Constants.Updates;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    public class Quest
    {
        [NotPersistent] public readonly List<QuestTemplate> RequiredQuests = new List<QuestTemplate>(3);
        public byte Entry;
        private readonly QuestLog m_Log;
        private bool m_saved;
        private readonly QuestRecord m_record;

        /// <summary>
        /// Template on which is this quest based, we might actually somehow cache only used templates
        /// in case someone would requested uncached template, we'd load it from DB/XML
        /// </summary>
        public readonly QuestTemplate Template;

        /// <summary>
        /// The time at which this Quest will expire for timed quests.
        /// </summary>
        public DateTime Until;

        /// <summary>Amounts of picked up Items</summary>
        public readonly int[] CollectedItems;

        /// <summary>Amounts of picked up Items</summary>
        public readonly int[] CollectedSourceItems;

        public Character Owner
        {
            get { return this.m_Log.Owner; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WCell.RealmServer.Quests.Quest" /> class.
        /// Which represents one item in character's <seealso cref="T:WCell.RealmServer.Quests.QuestLog" />
        /// </summary>
        /// <param name="template">The Quest Template.</param>
        internal Quest(QuestLog log, QuestTemplate template, int slot)
            : this(log, template, new QuestRecord(template.Id, log.Owner.EntityId.Low))
        {
            if (template.HasObjectOrSpellInteractions)
                this.Interactions = new uint[template.ObjectOrSpellInteractions.Length];
            if (template.AreaTriggerObjectives.Length > 0)
                this.VisitedATs = new bool[template.AreaTriggerObjectives.Length];
            this.Slot = slot;
        }

        /// <summary>Load Quest progress</summary>
        internal Quest(QuestLog log, QuestRecord record, QuestTemplate template)
            : this(log, template, record)
        {
            this.m_saved = true;
            if (template.HasObjectOrSpellInteractions)
            {
                if (this.Interactions == null ||
                    ((IEnumerable<QuestInteractionTemplate>) template.ObjectOrSpellInteractions)
                    .Count<QuestInteractionTemplate>() != ((IEnumerable<uint>) this.Interactions).Count<uint>())
                    this.Interactions = new uint[template.ObjectOrSpellInteractions.Length];
                for (int index = 0; index < this.Template.ObjectOrSpellInteractions.Length; ++index)
                {
                    QuestInteractionTemplate spellInteraction = this.Template.ObjectOrSpellInteractions[index];
                    if (spellInteraction != null && spellInteraction.IsValid)
                        log.Owner.SetQuestCount(this.Slot, spellInteraction.Index,
                            (ushort) (byte) this.Interactions[index]);
                }
            }

            this.UpdateStatus();
        }

        private Quest(QuestLog log, QuestTemplate template, QuestRecord record)
        {
            this.m_record = record;
            if (template.CollectableItems.Length > 0)
                this.CollectedItems = new int[template.CollectableItems.Length];
            if (template.CollectableSourceItems.Length > 0)
                this.CollectedSourceItems = new int[template.CollectableSourceItems.Length];
            this.m_Log = log;
            this.Template = template;
        }

        /// <summary>Amounts interactions</summary>
        public uint[] Interactions
        {
            get { return this.m_record.Interactions; }
            private set { this.m_record.Interactions = value; }
        }

        /// <summary>Visited AreaTriggers</summary>
        public bool[] VisitedATs
        {
            get { return this.m_record.VisitedATs; }
            private set { this.m_record.VisitedATs = value; }
        }

        public int Slot
        {
            get { return this.m_record.Slot; }
            set { this.m_record.Slot = value; }
        }

        public uint TemplateId
        {
            get { return this.m_record.QuestTemplateId; }
            set { this.m_record.QuestTemplateId = value; }
        }

        /// <summary>Current status of quest in QuestLog</summary>
        public QuestStatus Status
        {
            get
            {
                if (this.Template.IsTooHighLevel(this.m_Log.Owner))
                    return QuestStatus.TooHighLevel;
                if (this.CompleteStatus != QuestCompleteStatus.Completed)
                    return QuestStatus.NotCompleted;
                return !this.Template.Repeatable ? QuestStatus.Completable : QuestStatus.RepeateableCompletable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is quest completed] [the specified qt]; otherwise, <c>false</c>.
        /// </returns>
        public QuestCompleteStatus CompleteStatus
        {
            get { return this.m_Log.Owner.GetQuestState(this.Slot); }
            set { this.m_Log.Owner.SetQuestState(this.Slot, value); }
        }

        public bool IsSaved
        {
            get { return this.m_saved; }
        }

        public bool CheckCompletedStatus()
        {
            if (this.Template.CompleteHandler != null && this.Template.CompleteHandler(this))
                return true;
            for (int index = 0; index < this.Template.CollectableItems.Length; ++index)
            {
                if (this.CollectedItems[index] < this.Template.CollectableItems[index].Amount)
                    return false;
            }

            if (this.Template.HasObjectOrSpellInteractions)
            {
                for (int index = 0; index < this.Template.ObjectOrSpellInteractions.Length; ++index)
                {
                    QuestInteractionTemplate spellInteraction = this.Template.ObjectOrSpellInteractions[index];
                    if (spellInteraction != null && spellInteraction.IsValid)
                    {
                        uint interaction = this.Interactions[index];
                        bool flag = spellInteraction.ObjectType != ObjectTypeId.None || spellInteraction.Amount == 0;
                        if (!flag && interaction == 0U || flag && (long) interaction <
                            (long) this.Template.ObjectOrSpellInteractions[index].Amount)
                            return false;
                    }
                }
            }

            return true;
        }

        public void SignalATVisited(uint id)
        {
            if (this.VisitedATs == null)
                return;
            for (int index = 0; index < this.Template.AreaTriggerObjectives.Length; ++index)
            {
                if ((int) this.Template.AreaTriggerObjectives[index] == (int) id)
                {
                    this.VisitedATs[index] = true;
                    this.UpdateStatus();
                    break;
                }
            }
        }

        public void OfferQuestReward(IQuestHolder qHolder)
        {
            QuestHandler.SendQuestGiverOfferReward((IEntity) qHolder, this.Template, this.m_Log.Owner);
        }

        /// <summary>
        /// Tries to hand out the rewards, archives this quest and sends details about the next quest in the chain (if any).
        /// </summary>
        /// <param name="qHolder"></param>
        /// <param name="rewardSlot"></param>
        public bool TryFinish(IQuestHolder qHolder, uint rewardSlot)
        {
            Character owner = this.m_Log.Owner;
            owner.OnInteract(qHolder as WorldObject);
            if (qHolder is WorldObject &&
                !owner.IsInRadius((WorldObject) qHolder, (float) NPCMgr.DefaultInteractionDistance))
            {
                WCell.RealmServer.Handlers.NPCHandler.SendNPCError((IPacketReceiver) owner, (IEntity) qHolder,
                    VendorInventoryError.TooFarAway);
                return false;
            }

            if (!this.Template.TryGiveRewards(this.m_Log.Owner, qHolder, rewardSlot))
                return false;
            this.ArchiveQuest();
            QuestHandler.SendComplete(this.Template, owner);
            if (this.Template.FollowupQuestId != 0U)
            {
                QuestTemplate template = QuestMgr.GetTemplate(this.Template.FollowupQuestId);
                if (template != null && qHolder.QuestHolderInfo.QuestStarts.Contains(template))
                {
                    QuestHandler.SendDetails((IEntity) qHolder, template, owner, true);
                    if (template.Flags.HasFlag((Enum) QuestFlags.AutoAccept))
                        owner.QuestLog.TryAddQuest(template, qHolder);
                }
            }

            if (!this.Template.Repeatable)
            {
                owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteQuestCount, 1U, 0U,
                    (Unit) null);
                owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteQuest,
                    (uint) this.Entry, 0U, (Unit) null);
                if (this.Template.ZoneTemplate != null)
                    owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteQuestsInZone,
                        (uint) this.Template.ZoneTemplate.Id, 0U, (Unit) null);
            }

            if (this.Template.IsDaily)
                owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteDailyQuest, 1U, 0U,
                    (Unit) null);
            return true;
        }

        /// <summary>
        /// Removes Quest from Active log and adds it to the finished Quests
        /// </summary>
        public void ArchiveQuest()
        {
            if (!this.m_Log.m_FinishedQuests.Contains(this.Template.Id))
            {
                this.m_Log.m_FinishedQuests.Add(this.Template.Id);
                this.Template.NotifyFinished(this);
                int num = this.Template.IsDaily ? 1 : 0;
            }

            this.m_Log.RemoveQuest(this);
        }

        /// <summary>Cancels the quest and removes it from the QuestLog</summary>
        public void Cancel(bool failed)
        {
            this.Template.NotifyCancelled(this, failed);
            this.m_Log.RemoveQuest(this);
        }

        public void UpdateStatus()
        {
            if (this.CheckCompletedStatus())
            {
                this.m_Log.Owner.SetQuestState(this.Slot, QuestCompleteStatus.Completed);
                QuestHandler.SendQuestUpdateComplete(this.m_Log.Owner, this.Template.Id);
            }
            else
                this.m_Log.Owner.SetQuestState(this.Slot, QuestCompleteStatus.NotCompleted);
        }

        public void Save()
        {
            if (this.m_saved)
            {
                this.m_record.Update();
            }
            else
            {
                this.m_record.Create();
                this.m_saved = true;
            }
        }

        public void Delete()
        {
            if (!this.m_saved)
                return;
            this.m_record.Delete();
            this.m_saved = false;
        }

        public override string ToString()
        {
            return this.Template.ToString();
        }
    }
}