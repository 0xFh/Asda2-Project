using WCell.Constants.Quests;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    public abstract class QuestGiverRelation : IDataHolder
    {
        public QuestGiverRelationType RelationType;
        public uint QuestGiverId;
        public uint QuestId;

        public abstract ObjectTemplate ObjectTemplate { get; }

        public void FinalizeDataHolder()
        {
            QuestTemplate template = QuestMgr.GetTemplate(this.QuestId);
            if (template == null)
            {
                ContentMgr.OnInvalidDBData(
                    this.GetType().Name + " (QuestGiverId: {0}) referred to invalid QuestId: " + (object) this.QuestId,
                    (object) this.QuestGiverId);
            }
            else
            {
                ObjectTemplate objectTemplate = this.ObjectTemplate;
                if (objectTemplate == null)
                {
                    ContentMgr.OnInvalidDBData(
                        this.GetType().Name + " (QuestId: {0}) referred to invalid QuestGiverId: " +
                        (object) this.QuestGiverId, (object) this.QuestId);
                }
                else
                {
                    QuestHolderInfo questHolderInfo = objectTemplate.QuestHolderInfo;
                    bool flag = questHolderInfo == null;
                    if (flag)
                        objectTemplate.QuestHolderInfo = questHolderInfo = new QuestHolderInfo();
                    switch (this.RelationType)
                    {
                        case QuestGiverRelationType.Starter:
                            questHolderInfo.QuestStarts.Add(template);
                            template.Starters.Add((IQuestHolderEntry) objectTemplate);
                            if (!flag)
                                break;
                            ++QuestMgr._questStarterCount;
                            break;
                        case QuestGiverRelationType.Finisher:
                            questHolderInfo.QuestEnds.Add(template);
                            template.Finishers.Add((IQuestHolderEntry) objectTemplate);
                            if (!flag)
                                break;
                            ++QuestMgr._questFinisherCount;
                            break;
                        default:
                            ContentMgr.OnInvalidDBData(
                                this.GetType().Name +
                                " (Quest: {0}, QuestGiver: {1}) had invalid QuestGiverRelationType: " +
                                (object) this.RelationType, (object) this.QuestId, (object) this.QuestGiverId);
                            break;
                    }
                }
            }
        }
    }
}