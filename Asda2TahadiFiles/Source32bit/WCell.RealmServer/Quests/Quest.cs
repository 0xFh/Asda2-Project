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
    [NotPersistent]public readonly List<QuestTemplate> RequiredQuests = new List<QuestTemplate>(3);
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
      get { return m_Log.Owner; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:WCell.RealmServer.Quests.Quest" /> class.
    /// Which represents one item in character's <seealso cref="T:WCell.RealmServer.Quests.QuestLog" />
    /// </summary>
    /// <param name="template">The Quest Template.</param>
    internal Quest(QuestLog log, QuestTemplate template, int slot)
      : this(log, template, new QuestRecord(template.Id, log.Owner.EntityId.Low))
    {
      if(template.HasObjectOrSpellInteractions)
        Interactions = new uint[template.ObjectOrSpellInteractions.Length];
      if(template.AreaTriggerObjectives.Length > 0)
        VisitedATs = new bool[template.AreaTriggerObjectives.Length];
      Slot = slot;
    }

    /// <summary>Load Quest progress</summary>
    internal Quest(QuestLog log, QuestRecord record, QuestTemplate template)
      : this(log, template, record)
    {
      m_saved = true;
      if(template.HasObjectOrSpellInteractions)
      {
        if(Interactions == null ||
           template.ObjectOrSpellInteractions
             .Count() != Interactions.Count())
          Interactions = new uint[template.ObjectOrSpellInteractions.Length];
        for(int index = 0; index < Template.ObjectOrSpellInteractions.Length; ++index)
        {
          QuestInteractionTemplate spellInteraction = Template.ObjectOrSpellInteractions[index];
          if(spellInteraction != null && spellInteraction.IsValid)
            log.Owner.SetQuestCount(Slot, spellInteraction.Index,
              (byte) Interactions[index]);
        }
      }

      UpdateStatus();
    }

    private Quest(QuestLog log, QuestTemplate template, QuestRecord record)
    {
      m_record = record;
      if(template.CollectableItems.Length > 0)
        CollectedItems = new int[template.CollectableItems.Length];
      if(template.CollectableSourceItems.Length > 0)
        CollectedSourceItems = new int[template.CollectableSourceItems.Length];
      m_Log = log;
      Template = template;
    }

    /// <summary>Amounts interactions</summary>
    public uint[] Interactions
    {
      get { return m_record.Interactions; }
      private set { m_record.Interactions = value; }
    }

    /// <summary>Visited AreaTriggers</summary>
    public bool[] VisitedATs
    {
      get { return m_record.VisitedATs; }
      private set { m_record.VisitedATs = value; }
    }

    public int Slot
    {
      get { return m_record.Slot; }
      set { m_record.Slot = value; }
    }

    public uint TemplateId
    {
      get { return m_record.QuestTemplateId; }
      set { m_record.QuestTemplateId = value; }
    }

    /// <summary>Current status of quest in QuestLog</summary>
    public QuestStatus Status
    {
      get
      {
        if(Template.IsTooHighLevel(m_Log.Owner))
          return QuestStatus.TooHighLevel;
        if(CompleteStatus != QuestCompleteStatus.Completed)
          return QuestStatus.NotCompleted;
        return !Template.Repeatable ? QuestStatus.Completable : QuestStatus.RepeateableCompletable;
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
      get { return m_Log.Owner.GetQuestState(Slot); }
      set { m_Log.Owner.SetQuestState(Slot, value); }
    }

    public bool IsSaved
    {
      get { return m_saved; }
    }

    public bool CheckCompletedStatus()
    {
      if(Template.CompleteHandler != null && Template.CompleteHandler(this))
        return true;
      for(int index = 0; index < Template.CollectableItems.Length; ++index)
      {
        if(CollectedItems[index] < Template.CollectableItems[index].Amount)
          return false;
      }

      if(Template.HasObjectOrSpellInteractions)
      {
        for(int index = 0; index < Template.ObjectOrSpellInteractions.Length; ++index)
        {
          QuestInteractionTemplate spellInteraction = Template.ObjectOrSpellInteractions[index];
          if(spellInteraction != null && spellInteraction.IsValid)
          {
            uint interaction = Interactions[index];
            bool flag = spellInteraction.ObjectType != ObjectTypeId.None || spellInteraction.Amount == 0;
            if(!flag && interaction == 0U || flag && interaction <
               Template.ObjectOrSpellInteractions[index].Amount)
              return false;
          }
        }
      }

      return true;
    }

    public void SignalATVisited(uint id)
    {
      if(VisitedATs == null)
        return;
      for(int index = 0; index < Template.AreaTriggerObjectives.Length; ++index)
      {
        if((int) Template.AreaTriggerObjectives[index] == (int) id)
        {
          VisitedATs[index] = true;
          UpdateStatus();
          break;
        }
      }
    }

    public void OfferQuestReward(IQuestHolder qHolder)
    {
      QuestHandler.SendQuestGiverOfferReward(qHolder, Template, m_Log.Owner);
    }

    /// <summary>
    /// Tries to hand out the rewards, archives this quest and sends details about the next quest in the chain (if any).
    /// </summary>
    /// <param name="qHolder"></param>
    /// <param name="rewardSlot"></param>
    public bool TryFinish(IQuestHolder qHolder, uint rewardSlot)
    {
      Character owner = m_Log.Owner;
      owner.OnInteract(qHolder as WorldObject);
      if(qHolder is WorldObject &&
         !owner.IsInRadius((WorldObject) qHolder, NPCMgr.DefaultInteractionDistance))
      {
        NPCHandler.SendNPCError(owner, qHolder,
          VendorInventoryError.TooFarAway);
        return false;
      }

      if(!Template.TryGiveRewards(m_Log.Owner, qHolder, rewardSlot))
        return false;
      ArchiveQuest();
      QuestHandler.SendComplete(Template, owner);
      if(Template.FollowupQuestId != 0U)
      {
        QuestTemplate template = QuestMgr.GetTemplate(Template.FollowupQuestId);
        if(template != null && qHolder.QuestHolderInfo.QuestStarts.Contains(template))
        {
          QuestHandler.SendDetails(qHolder, template, owner, true);
          if(template.Flags.HasFlag(QuestFlags.AutoAccept))
            owner.QuestLog.TryAddQuest(template, qHolder);
        }
      }

      if(!Template.Repeatable)
      {
        owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteQuestCount, 1U, 0U,
          null);
        owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteQuest,
          Entry, 0U, null);
        if(Template.ZoneTemplate != null)
          owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteQuestsInZone,
            (uint) Template.ZoneTemplate.Id, 0U, null);
      }

      if(Template.IsDaily)
        owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteDailyQuest, 1U, 0U,
          null);
      return true;
    }

    /// <summary>
    /// Removes Quest from Active log and adds it to the finished Quests
    /// </summary>
    public void ArchiveQuest()
    {
      if(!m_Log.m_FinishedQuests.Contains(Template.Id))
      {
        m_Log.m_FinishedQuests.Add(Template.Id);
        Template.NotifyFinished(this);
        int num = Template.IsDaily ? 1 : 0;
      }

      m_Log.RemoveQuest(this);
    }

    /// <summary>Cancels the quest and removes it from the QuestLog</summary>
    public void Cancel(bool failed)
    {
      Template.NotifyCancelled(this, failed);
      m_Log.RemoveQuest(this);
    }

    public void UpdateStatus()
    {
      if(CheckCompletedStatus())
      {
        m_Log.Owner.SetQuestState(Slot, QuestCompleteStatus.Completed);
        QuestHandler.SendQuestUpdateComplete(m_Log.Owner, Template.Id);
      }
      else
        m_Log.Owner.SetQuestState(Slot, QuestCompleteStatus.NotCompleted);
    }

    public void Save()
    {
      if(m_saved)
      {
        m_record.Update();
      }
      else
      {
        m_record.Create();
        m_saved = true;
      }
    }

    public void Delete()
    {
      if(!m_saved)
        return;
      m_record.Delete();
      m_saved = false;
    }

    public override string ToString()
    {
      return Template.ToString();
    }
  }
}