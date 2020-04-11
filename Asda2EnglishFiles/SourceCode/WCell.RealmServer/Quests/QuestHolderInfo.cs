using System;
using System.Collections.Generic;
using WCell.Constants.Quests;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.Util;

namespace WCell.RealmServer.Quests
{
    /// <summary>
    /// TODO: Add methods related to query QuestGiver-specific information etc
    /// 
    /// Represents all information that a QuestGiver has
    /// </summary>
    [Serializable]
    public class QuestHolderInfo : IQuestHolderInfo
    {
        /// <summary>Set of Quests that may start at this QuestHolder</summary>
        public List<QuestTemplate> QuestStarts;

        /// <summary>
        /// Set of Quests that may be turned in at this QuestHolder
        /// </summary>
        public List<QuestTemplate> QuestEnds;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WCell.RealmServer.Quests.QuestHolderInfo" /> class.
        /// </summary>
        public QuestHolderInfo()
        {
            this.QuestStarts = new List<QuestTemplate>(1);
            this.QuestEnds = new List<QuestTemplate>(1);
        }

        /// <summary>
        /// Gets the Highest Status of any quest newly available or continuable for the given Character
        /// </summary>
        /// <param name="chr">The character which is status calculated with.</param>
        /// <returns></returns>
        public QuestStatus GetHighestQuestGiverStatus(Character chr)
        {
            QuestStatus questStatus = QuestStatus.NotAvailable;
            if (this.QuestStarts != null)
            {
                foreach (QuestTemplate questStart in this.QuestStarts)
                {
                    QuestStatus startStatus = questStart.GetStartStatus(this, chr);
                    if (startStatus > questStatus)
                        questStatus = startStatus;
                }
            }

            if (this.QuestEnds != null)
            {
                foreach (QuestTemplate questEnd in this.QuestEnds)
                {
                    QuestStatus endStatus = questEnd.GetEndStatus(chr);
                    if (endStatus > questStatus)
                        questStatus = endStatus;
                }
            }

            return questStatus;
        }

        /// <summary>
        /// Gets list of quests, which are activatable by this Character (not low leveled nor unavailable).
        /// </summary>
        /// <param name="chr">The client.</param>
        /// <returns>List of the active quests.</returns>
        public List<QuestTemplate> GetAvailableQuests(Character chr)
        {
            List<QuestTemplate> questTemplateList = new List<QuestTemplate>();
            foreach (QuestTemplate questEnd in this.QuestEnds)
            {
                if (chr.QuestLog.GetQuestById(questEnd.Id) != null)
                    questTemplateList.Add(questEnd);
            }

            foreach (QuestTemplate questStart in this.QuestStarts)
            {
                if (questStart.GetStartStatus(this, chr).IsAvailable())
                    questTemplateList.Add(questStart);
            }

            return questTemplateList;
        }

        /// <summary>
        /// Gets the QuestMenuItems for a <see href="GossiGossipMenu">GossipMenu</see>
        /// </summary>
        /// <param name="chr">The client.</param>
        /// <returns></returns>
        public List<QuestMenuItem> GetQuestMenuItems(Character chr)
        {
            List<QuestMenuItem> questMenuItemList = new List<QuestMenuItem>();
            foreach (QuestTemplate questEnd in this.QuestEnds)
            {
                if (chr.QuestLog.GetQuestById(questEnd.Id) != null)
                    questMenuItemList.Add(new QuestMenuItem(questEnd.Id, 4U, questEnd.Level,
                        questEnd.Titles.Localize(chr.Locale)));
            }

            foreach (QuestTemplate questStart in this.QuestStarts)
            {
                QuestStatus startStatus = questStart.GetStartStatus(this, chr);
                if (startStatus.IsAvailable())
                    questMenuItemList.Add(new QuestMenuItem(questStart.Id,
                        startStatus == QuestStatus.Available ? 2U : 4U, questStart.Level,
                        questStart.Titles.Localize(chr.Locale)));
            }

            return questMenuItemList;
        }

        public override string ToString()
        {
            return string.Format("QuestHolder - Starts: {0}; Ends: {1}",
                (object) Utility.GetStringRepresentation((object) this.QuestStarts),
                (object) Utility.GetStringRepresentation((object) this.QuestEnds));
        }
    }
}