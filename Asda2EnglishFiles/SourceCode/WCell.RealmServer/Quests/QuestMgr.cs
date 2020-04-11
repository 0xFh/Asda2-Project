using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Quests;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Quests
{
    /// <summary>
    /// Implementation of quest manager which is handlint most of the quest actions at server.
    /// TODO: Faction-restrictions
    /// </summary>
    [GlobalMgr]
    public static class QuestMgr
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Amount of levels above the Character level at which a quest becomes obsolete to the player
        /// </summary>
        public static int LevelObsoleteOffset = 7;

        /// <summary>
        /// The Xp Reward Level that is used to determine the Xp reward upon completion.
        /// </summary>
        public static readonly QuestXPInfo[] QuestXpInfos = new QuestXPInfo[200];

        /// <summary>
        /// The Reputation level that is used to deteminate the reputation reward upon quest completion.
        /// </summary>
        public static readonly QuestRewRepInfo[] QuestRewRepInfos = new QuestRewRepInfo[2];

        /// <summary>
        /// The Honor Reward Level that is used to determine the honor reward upon completion.
        /// </summary>
        public static readonly QuestHonorInfo[] QuestHonorInfos = new QuestHonorInfo[2000];

        /// <summary>
        /// Amount of levels above the Character level at which a player is not allowed to do the Quest
        /// </summary>
        public static int LevelRequirementOffset = 1;

        [NotVariable] public static QuestTemplate[] Templates = new QuestTemplate[30000];
        public static Dictionary<uint, List<QuestPOI>> POIs = new Dictionary<uint, List<QuestPOI>>();
        internal static uint _questCount;
        internal static int _questFinisherCount;
        internal static int _questStarterCount;
        private static bool loaded;

        public static uint QuestCount
        {
            get { return QuestMgr._questCount; }
        }

        public static int QuestFinisherCount
        {
            get { return QuestMgr._questFinisherCount; }
        }

        public static int QuestStarterCount
        {
            get { return QuestMgr._questStarterCount; }
        }

        public static bool Loaded
        {
            get { return QuestMgr.loaded; }
            private set
            {
                if (!(QuestMgr.loaded = value))
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(QuestMgr));
            }
        }

        public static QuestTemplate GetTemplate(uint id)
        {
            if ((long) id >= (long) QuestMgr.Templates.Length)
                return (QuestTemplate) null;
            return QuestMgr.Templates[id];
        }

        public static void Initialize()
        {
            QuestMgr.LoadAll();
        }

        /// <summary>Loads the quest templates.</summary>
        /// <returns></returns>
        public static bool LoadAll()
        {
            if (!QuestMgr.Loaded)
            {
                QuestMgr.Templates = new QuestTemplate[30000];
                ContentMgr.Load<QuestTemplate>();
                ContentMgr.Load<QuestPOI>();
                ContentMgr.Load<QuestPOIPoints>();
                QuestMgr.CreateQuestRelationGraph();
                QuestMgr.EnsureCharacterQuestsLoaded();
                QuestMgr.AddSpellCastObjectives();
                if (ItemMgr.Loaded)
                    ItemMgr.EnsureItemQuestRelations();
                foreach (QuestTemplate template1 in QuestMgr.Templates)
                {
                    if (template1 != null)
                    {
                        ItemTemplate template2 = ItemMgr.GetTemplate(template1.SrcItemId);
                        if (template2 != null && template1.SrcItemId != (Asda2ItemId) 0 &&
                            !template1.Starters.Contains((IQuestHolderEntry) template2))
                            template1.ProvidedItems.Add(new Asda2ItemStackDescription(template1.SrcItemId, 1));
                    }
                }

                QuestMgr.Loaded = true;
                QuestMgr.log.Debug("{0} Quests loaded.", QuestMgr._questCount);
            }

            return true;
        }

        private static void AddSpellCastObjectives()
        {
        }

        /// <summary>Creates the graph of all quests and their relations</summary>
        private static void CreateQuestRelationGraph()
        {
            Dictionary<int, List<uint>> map = new Dictionary<int, List<uint>>();
            foreach (QuestTemplate template1 in QuestMgr.Templates)
            {
                if (template1 != null)
                {
                    if (template1.Id == 10068U)
                        template1.ToString();
                    if (template1.ExclusiveGroup != 0)
                        map.GetOrCreate<int, uint>(template1.ExclusiveGroup).AddUnique<uint>(template1.Id);
                    else if (template1.NextQuestId != 0)
                    {
                        QuestTemplate template2 = QuestMgr.GetTemplate((uint) Math.Abs(template1.NextQuestId));
                        if (template2 == null)
                            ContentMgr.OnInvalidDBData("NextQuestId {0} is invalid in: {1}",
                                (object) template1.NextQuestId, (object) template1);
                        else if (template1.NextQuestId > 0)
                            template2.ReqAllFinishedQuests.AddUnique<uint>(template1.Id);
                        else
                            template2.ReqAllActiveQuests.AddUnique<uint>(template1.Id);
                    }

                    if (template1.PreviousQuestId != 0)
                    {
                        if (template1.PreviousQuestId > 0)
                            template1.ReqAllFinishedQuests.AddUnique<uint>((uint) template1.PreviousQuestId);
                        else
                            template1.ReqAllActiveQuests.AddUnique<uint>((uint) -template1.PreviousQuestId);
                    }

                    if (template1.FollowupQuestId != 0U)
                    {
                        QuestTemplate template2 = QuestMgr.GetTemplate(template1.FollowupQuestId);
                        if (template2 != null)
                            template2.ReqAllFinishedQuests.AddUnique<uint>(template1.Id);
                    }
                }
            }

            foreach (KeyValuePair<int, List<uint>> keyValuePair in map)
            {
                foreach (uint id in keyValuePair.Value)
                {
                    QuestTemplate template1 = QuestMgr.GetTemplate(id);
                    foreach (uint num in keyValuePair.Value)
                    {
                        if ((int) num != (int) id && keyValuePair.Key > 0)
                            template1.ReqUndoneQuests.AddUnique<uint>(num);
                    }

                    if (template1.NextQuestId != 0)
                    {
                        QuestTemplate template2 = QuestMgr.GetTemplate((uint) Math.Abs(template1.NextQuestId));
                        if (template2 == null)
                            ContentMgr.OnInvalidDBData("NextQuestId {0} is invalid in: {1}",
                                (object) template1.NextQuestId, (object) template1);
                        else if (keyValuePair.Key > 0)
                            template2.ReqAllFinishedQuests.AddUnique<uint>(template1.Id);
                        else
                            template2.ReqAnyFinishedQuests.AddUnique<uint>(template1.Id);
                    }
                }
            }
        }

        private static void EnsureCharacterQuestsLoaded()
        {
            List<Character> allCharacters = World.GetAllCharacters();
            for (int index = 0; index < allCharacters.Count; ++index)
            {
                Character chr = allCharacters[index];
                chr.AddMessage((Action) (() =>
                {
                    if (!chr.IsInWorld)
                        return;
                    chr.LoadQuests();
                }));
            }
        }

        public static bool UnloadAll()
        {
            if (!QuestMgr.Loaded)
                return false;
            QuestMgr.Loaded = false;
            QuestMgr.Templates = new QuestTemplate[30000];
            return true;
        }

        public static bool Reload()
        {
            if (QuestMgr.UnloadAll())
                return QuestMgr.LoadAll();
            return false;
        }

        [WCell.Core.Initialization.Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        [DependentInitialization(typeof(QuestMgr))]
        public static void EnsureNPCRelationsLoaded()
        {
            ContentMgr.Load<NPCQuestGiverRelation>();
        }

        /// <summary>Loads GO - questgiver relations.</summary>
        [DependentInitialization(typeof(QuestMgr))]
        [WCell.Core.Initialization.Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void EnsureGOQuestRelationsLoaded()
        {
            ContentMgr.Load<GOQuestGiverRelation>();
        }

        /// <summary>Loads Item - questgiver relations.</summary>
        [DependentInitialization(typeof(ItemMgr))]
        [DependentInitialization(typeof(QuestMgr))]
        [WCell.Core.Initialization.Initialization]
        public static void SetItemQuestRelations()
        {
            foreach (ItemTemplate template1 in ItemMgr.Templates)
            {
                if (template1 != null && template1.QuestId != 0U)
                {
                    QuestTemplate template2 = QuestMgr.GetTemplate(template1.QuestId);
                    if (template2 != null)
                    {
                        template1.QuestHolderInfo = new QuestHolderInfo();
                        template1.QuestHolderInfo.QuestStarts.Add(template2);
                    }
                }
            }
        }

        public static void AddQuest(QuestTemplate template)
        {
            if (QuestMgr.Templates.Get<QuestTemplate>(template.Id) == null)
                ++QuestMgr._questCount;
            ArrayUtil.Set<QuestTemplate>(ref QuestMgr.Templates, template.Id, template);
        }

        public static void StartQuestDialog(this IQuestHolder qHolder, Character chr)
        {
            chr.OnInteract(qHolder as WorldObject);
            List<QuestTemplate> availableQuests = qHolder.QuestHolderInfo.GetAvailableQuests(chr);
            if (availableQuests.Count <= 0)
                return;
            if (availableQuests.Count == 1 && !chr.QuestLog.HasActiveQuest(availableQuests[0].Id))
            {
                bool flag = availableQuests[0].Flags.HasFlag((Enum) QuestFlags.AutoAccept);
                QuestHandler.SendDetails((IEntity) qHolder, availableQuests[0], chr, !flag);
                if (!flag)
                    return;
                chr.QuestLog.TryAddQuest(availableQuests[0], qHolder);
            }
            else
                QuestHandler.SendQuestList(qHolder, availableQuests, chr);
        }
    }
}