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
      QuestTemplate template = QuestMgr.GetTemplate(QuestId);
      if(template == null)
      {
        ContentMgr.OnInvalidDBData(
          GetType().Name + " (QuestGiverId: {0}) referred to invalid QuestId: " + QuestId,
          (object) QuestGiverId);
      }
      else
      {
        ObjectTemplate objectTemplate = ObjectTemplate;
        if(objectTemplate == null)
        {
          ContentMgr.OnInvalidDBData(
            GetType().Name + " (QuestId: {0}) referred to invalid QuestGiverId: " +
            QuestGiverId, (object) QuestId);
        }
        else
        {
          QuestHolderInfo questHolderInfo = objectTemplate.QuestHolderInfo;
          bool flag = questHolderInfo == null;
          if(flag)
            objectTemplate.QuestHolderInfo = questHolderInfo = new QuestHolderInfo();
          switch(RelationType)
          {
            case QuestGiverRelationType.Starter:
              questHolderInfo.QuestStarts.Add(template);
              template.Starters.Add(objectTemplate);
              if(!flag)
                break;
              ++QuestMgr._questStarterCount;
              break;
            case QuestGiverRelationType.Finisher:
              questHolderInfo.QuestEnds.Add(template);
              template.Finishers.Add(objectTemplate);
              if(!flag)
                break;
              ++QuestMgr._questFinisherCount;
              break;
            default:
              ContentMgr.OnInvalidDBData(
                GetType().Name +
                " (Quest: {0}, QuestGiver: {1}) had invalid QuestGiverRelationType: " +
                RelationType, (object) QuestId, (object) QuestGiverId);
              break;
          }
        }
      }
    }
  }
}