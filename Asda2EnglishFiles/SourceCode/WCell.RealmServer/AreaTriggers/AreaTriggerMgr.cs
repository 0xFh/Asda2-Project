using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.AreaTriggers;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.AreaTriggers
{
    [GlobalMgr]
    public static class AreaTriggerMgr
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        [NotVariable] public static AreaTrigger[] AreaTriggers = new AreaTrigger[3000];
        public static readonly AreaTriggerHandler[] Handlers = new AreaTriggerHandler[7];
        private static bool _loaded;

        static AreaTriggerMgr()
        {
            AreaTriggerMgr.Handlers[0] = new AreaTriggerHandler(AreaTriggerMgr.NoAction);
            AreaTriggerMgr.Handlers[2] = new AreaTriggerHandler(AreaTriggerMgr.HandleQuestTrigger);
            AreaTriggerMgr.Handlers[3] = new AreaTriggerHandler(AreaTriggerMgr.HandleRest);
            AreaTriggerMgr.Handlers[4] = new AreaTriggerHandler(AreaTriggerMgr.HandleTeleport);
            AreaTriggerMgr.Handlers[5] = new AreaTriggerHandler(AreaTriggerMgr.HandleSpell);
        }

        /// <summary>Do nothing</summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static bool NoAction(Character arg1, AreaTrigger arg2)
        {
            return false;
        }

        public static void SetHandler(AreaTriggerType type, AreaTriggerHandler handler)
        {
            AreaTriggerMgr.Handlers[(int) type] = handler;
        }

        /// <summary>Teleports into an instance</summary>
        /// <param name="chr"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static bool HandleTeleport(Character chr, AreaTrigger trigger)
        {
            MapTemplate mapTemplate = World.GetMapTemplate(trigger.Template.TargetMap);
            if (mapTemplate.IsInstance)
            {
                if (mapTemplate.Type != MapType.Normal)
                    return InstanceMgr.EnterInstance(chr, mapTemplate, trigger.Template.TargetPos);
                InstanceMgr.LeaveInstance(chr, mapTemplate, trigger.Template.TargetPos);
                return true;
            }

            if (mapTemplate.BattlegroundTemplate == null)
            {
                Map nonInstancedMap = World.GetNonInstancedMap(mapTemplate.Id);
                if (nonInstancedMap != null)
                {
                    chr.TeleportTo(nonInstancedMap, trigger.Template.TargetPos,
                        new float?(trigger.Template.TargetOrientation));
                    return true;
                }

                ContentMgr.OnInvalidDBData("Invalid Map: " + (object) nonInstancedMap);
            }
            else
                chr.AddMessage((Action) (() => chr.TeleportTo(trigger.Template.TargetMap, trigger.Template.TargetPos)));

            return true;
        }

        /// <summary>Triggers Quest</summary>
        /// <param name="chr"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static bool HandleQuestTrigger(Character chr, AreaTrigger trigger)
        {
            Quest activeQuest = chr.QuestLog.GetActiveQuest(trigger.Template.TriggerQuestId);
            if (activeQuest == null)
                return false;
            activeQuest.SignalATVisited(trigger.Id);
            return true;
        }

        /// <summary>Start resting</summary>
        /// <param name="chr"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static bool HandleRest(Character chr, AreaTrigger trigger)
        {
            chr.RestTrigger = trigger;
            return true;
        }

        /// <summary>
        /// Cast a spell on the Character.
        /// [NYI]
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static bool HandleSpell(Character chr, AreaTrigger trigger)
        {
            return false;
        }

        public static AreaTriggerHandler GetHandler(AreaTriggerType type)
        {
            return AreaTriggerMgr.Handlers[(int) type] ?? new AreaTriggerHandler(AreaTriggerMgr.NoAction);
        }

        /// <summary>Depends on Table-Creation (Third)</summary>
        public static void Initialize()
        {
            foreach (KeyValuePair<int, AreaTrigger> entry in new MappedDBCReader<AreaTrigger, ATConverter>(
                RealmServerConfiguration.GetDBCFile("AreaTrigger.dbc")).Entries)
                ArrayUtil.Set<AreaTrigger>(ref AreaTriggerMgr.AreaTriggers, (uint) entry.Key, entry.Value);
            ContentMgr.Load<ATTemplate>();
            if (ServerApp<WCell.RealmServer.RealmServer>.InitMgr == null)
                return;
            ServerApp<WCell.RealmServer.RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(AreaTriggerMgr));
        }

        /// <summary>Loaded flag</summary>
        public static bool Loaded
        {
            get { return AreaTriggerMgr._loaded; }
            private set
            {
                if (!(AreaTriggerMgr._loaded = value) || ServerApp<WCell.RealmServer.RealmServer>.InitMgr == null)
                    return;
                ServerApp<WCell.RealmServer.RealmServer>.InitMgr.SignalGlobalMgrReady(typeof(AreaTriggerMgr));
            }
        }

        [DependentInitialization(typeof(AreaTriggerMgr))]
        [WCell.Core.Initialization.Initialization]
        [DependentInitialization(typeof(QuestMgr))]
        public static void InitializeQuestTriggers()
        {
            foreach (AreaTrigger areaTrigger in AreaTriggerMgr.AreaTriggers)
            {
                if (areaTrigger != null)
                {
                    ATTemplate template1 = areaTrigger.Template;
                    if (template1 != null && template1.TriggerQuestId != 0U)
                    {
                        template1.Type = AreaTriggerType.QuestTrigger;
                        QuestTemplate template2 = QuestMgr.GetTemplate(template1.TriggerQuestId);
                        if (template2 != null)
                        {
                            template1.Type = AreaTriggerType.QuestTrigger;
                            template2.AddAreaTriggerObjective(areaTrigger.Id);
                        }
                    }
                }
            }

            if (AreaTriggerMgr.AreaTriggers.Length <= 0)
                return;
            AreaTriggerMgr.Loaded = true;
        }

        public static AreaTrigger GetTrigger(uint id)
        {
            return AreaTriggerMgr.AreaTriggers.Get<AreaTrigger>(id);
        }

        public static AreaTrigger GetTrigger(AreaTriggerId id)
        {
            return AreaTriggerMgr.AreaTriggers.Get<AreaTrigger>((uint) id);
        }
    }
}