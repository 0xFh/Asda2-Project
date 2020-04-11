using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.DynamicAccess;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Instances
{
    public class InstanceMgr
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The max amount of different instances that a Character may enter within <see cref="F:WCell.RealmServer.Instances.InstanceMgr.InstanceCounterTime" />
        /// </summary>
        public static int MaxInstancesPerHour = 5;

        /// <summary>
        /// Amount of time until a normal empty Instance expires by default
        /// </summary>
        public static int DungeonExpiryMinutes = 10;

        /// <summary>
        /// Players may only enter MaxInstancesPerCharPerHour instances within this cooldown
        /// </summary>
        public static TimeSpan InstanceCounterTime = TimeSpan.FromHours(1.0);

        public static readonly Dictionary<uint, InstanceCollection> OfflineLogs =
            new Dictionary<uint, InstanceCollection>();

        public static readonly List<MapTemplate> InstanceInfos = new List<MapTemplate>();
        private static readonly ReaderWriterLockWrapper syncLock = new ReaderWriterLockWrapper();

        public static readonly WorldInstanceCollection<MapId, BaseInstance> Instances =
            new WorldInstanceCollection<MapId, BaseInstance>(MapId.End);

        public const int MaxInstanceDifficulties = 4;
        [NotVariable] public static GlobalInstanceTimer[] GlobalResetTimers;

        public static void SetCreator(MapId id, string typeName)
        {
            Type type = ServerApp<WCell.RealmServer.RealmServer>.GetType(typeName);
            if (type == (Type) null)
            {
                InstanceMgr.log.Warn("Invalid Creator for Instance \"" + (object) id + "\": " + typeName +
                                     "  - Please correct it in the Instance-config file: " +
                                     InstanceConfigBase<InstanceConfig, MapId>.Filename);
            }
            else
            {
                IProducer producer = AccessorMgr.GetOrCreateDefaultProducer(type);
                InstanceMgr.SetCreator(id, (InstanceCreator) (() => (BaseInstance) producer.Produce()));
            }
        }

        public static void SetCreator(MapId id, InstanceCreator creator)
        {
            MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(id);
            if (mapTemplate == null || mapTemplate.InstanceTemplate == null)
                throw new ArgumentException("Given Map is not an Instance:" + (object) id);
            mapTemplate.InstanceTemplate.Creator = creator;
        }

        /// <param name="creator">Can be null</param>
        public static I CreateInstance<I>(Character creator, InstanceTemplate template, uint difficultyIndex)
            where I : BaseInstance, new()
        {
            return (I) InstanceMgr.SetupInstance(creator, (BaseInstance) Activator.CreateInstance<I>(), template,
                difficultyIndex);
        }

        /// <param name="creator">Can be null</param>
        public static BaseInstance CreateInstance(Character creator, InstanceTemplate template, uint difficultyIndex)
        {
            BaseInstance instance = template.Create();
            return InstanceMgr.SetupInstance(creator, instance, template, difficultyIndex);
        }

        /// <summary>Convinience method for development</summary>
        /// <param name="creator">Can be null</param>
        public static BaseInstance CreateInstance(Character creator, MapId mapId)
        {
            MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(mapId);
            if (mapTemplate == null || !mapTemplate.IsInstance)
                return (BaseInstance) null;
            uint difficultyIndex;
            if (creator != null)
            {
                creator.EnsurePureStaffGroup();
                difficultyIndex = creator.GetInstanceDifficulty(mapTemplate.IsRaid);
            }
            else
                difficultyIndex = 0U;

            BaseInstance instance = mapTemplate.InstanceTemplate.Create();
            return InstanceMgr.SetupInstance(creator, instance, mapTemplate.InstanceTemplate, difficultyIndex);
        }

        /// <param name="creator">Can be null</param>
        private static BaseInstance SetupInstance(Character creator, BaseInstance instance, InstanceTemplate template,
            uint difficultyIndex = 0)
        {
            if (instance != null)
            {
                instance.difficulty = template.MapTemplate.GetDifficulty(difficultyIndex) ??
                                      template.MapTemplate.Difficulties[0];
                if (creator != null)
                {
                    instance.m_OwningFaction = creator.FactionGroup;
                    instance.Owner = (IInstanceHolderSet) creator.InstanceLeader;
                    instance.IsActive = true;
                }

                instance.InitMap(template.MapTemplate);
                InstanceMgr.Instances.AddInstance(instance.MapId, instance);
            }

            return instance;
        }

        /// <summary>
        /// This is called when an area trigger causes entering an instance
        /// </summary>
        public static bool EnterInstance(Character chr, MapTemplate mapTemplate, Vector3 targetPos)
        {
            if (!mapTemplate.IsInstance)
            {
                InstanceMgr.log.Error("Character {0} tried to enter \"{1}\" as Instance.", (object) chr,
                    (object) mapTemplate);
                return false;
            }

            bool isRaid = mapTemplate.Type == MapType.Raid;
            Group group = chr.Group;
            if (isRaid && !chr.Role.IsStaff && !group.Flags.HasFlag((Enum) GroupFlags.Raid))
            {
                InstanceHandler.SendRequiresRaid((IPacketReceiver) chr.Client, 0);
                return false;
            }

            if (!mapTemplate.MayEnter(chr))
                return false;
            chr.SendSystemMessage("Entering instance...");
            InstanceCollection instances = chr.Instances;
            BaseInstance instance = instances.GetActiveInstance(mapTemplate);
            if (instance == null)
            {
                if (mapTemplate.GetDifficulty(chr.GetInstanceDifficulty(isRaid)).BindingType == BindingType.Soft &&
                    !instances.HasFreeInstanceSlot && !chr.GodMode)
                {
                    MovementHandler.SendTransferFailure((IPacketReceiver) chr.Client, mapTemplate.Id,
                        MapTransferError.TRANSFER_ABORT_TOO_MANY_INSTANCES);
                    return false;
                }

                if (group != null)
                {
                    instance = group.GetActiveInstance(mapTemplate);
                    if (instance != null && !InstanceMgr.CheckFull(instance, chr))
                        return false;
                }

                if (instance == null)
                {
                    instance = InstanceMgr.CreateInstance(chr, mapTemplate.InstanceTemplate,
                        chr.GetInstanceDifficulty(isRaid));
                    if (instance == null)
                    {
                        InstanceMgr.log.Warn("Could not create Instance \"{0}\" for: {1}", (object) mapTemplate,
                            (object) chr);
                        return false;
                    }
                }
            }
            else if (!chr.GodMode)
            {
                if (!InstanceMgr.CheckFull(instance, chr))
                    return false;
                if (isRaid)
                {
                    if (group == null)
                    {
                        MovementHandler.SendTransferFailure((IPacketReceiver) chr.Client, instance.Id,
                            MapTransferError.TRANSFER_ABORT_NEED_GROUP);
                        return false;
                    }

                    InstanceBinding binding1 =
                        group.InstanceLeaderCollection.GetBinding(mapTemplate.Id, BindingType.Hard);
                    InstanceBinding binding2 = instances.GetBinding(mapTemplate.Id, BindingType.Hard);
                    if (binding2 != (InstanceBinding) null && binding1 != binding2)
                    {
                        MovementHandler.SendTransferFailure((IPacketReceiver) chr.Client, instance.Id,
                            MapTransferError.TRANSFER_ABORT_NOT_FOUND);
                        return false;
                    }
                }
            }

            instance.TeleportInside(chr, targetPos);
            return true;
        }

        private static bool CheckFull(BaseInstance instance, Character chr)
        {
            if (instance.MaxPlayerCount == 0 || instance.PlayerCount < instance.MaxPlayerCount || chr.GodMode)
                return true;
            MovementHandler.SendTransferFailure((IPacketReceiver) chr.Client, instance.Id,
                MapTransferError.TRANSFER_ABORT_MAX_PLAYERS);
            return false;
        }

        /// <summary>
        /// This is called when an area trigger causes leaving an instance
        /// TODO: Add associations to raid individuals
        /// TODO: Implement the 5 instances per hour limit, simple but needs the right spot
        /// </summary>
        public static void LeaveInstance(Character player, MapTemplate mapTemplate, Vector3 entryInfo)
        {
            Map nonInstancedMap = WCell.RealmServer.Global.World.GetNonInstancedMap(mapTemplate.Id);
            player.TeleportTo(nonInstancedMap, entryInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lowId"></param>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        public static InstanceCollection GetOfflineInstances(uint lowId, bool autoCreate)
        {
            return InstanceMgr.GetOfflineInstances(lowId, autoCreate, false);
        }

        public static void RemoveLog(uint lowId)
        {
            InstanceMgr.GetOfflineInstances(lowId, false, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lowId"></param>
        /// <param name="autoCreate"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public static InstanceCollection GetOfflineInstances(uint lowId, bool autoCreate, bool remove)
        {
            using (InstanceMgr.syncLock.EnterReadLock())
                return InstanceMgr.GetOfflineInstancesUnlocked(lowId, autoCreate, remove);
        }

        private static InstanceCollection GetOfflineInstancesUnlocked(uint lowId, bool autoCreate, bool remove)
        {
            InstanceCollection instanceCollection = (InstanceCollection) null;
            if (InstanceMgr.OfflineLogs.ContainsKey(lowId))
            {
                instanceCollection = InstanceMgr.OfflineLogs[lowId];
                if (remove)
                    InstanceMgr.OfflineLogs.Remove(lowId);
            }

            if (autoCreate)
            {
                instanceCollection = new InstanceCollection(lowId);
                InstanceMgr.OfflineLogs.Add(lowId, instanceCollection);
            }

            return instanceCollection;
        }

        /// <summary>
        /// Gets and removes the InstanceLog for the given Character
        /// </summary>
        /// <param name="character"></param>
        internal static void RetrieveInstances(Character character)
        {
            using (InstanceMgr.syncLock.EnterReadLock())
            {
                InstanceCollection instancesUnlocked =
                    InstanceMgr.GetOfflineInstancesUnlocked(character.EntityId.Low, false, true);
                if (instancesUnlocked == null)
                    return;
                instancesUnlocked.Character = character;
                character.Instances = instancesUnlocked;
            }
        }

        internal static void OnCharacterLogout(Character character)
        {
            if (!character.HasInstanceCollection)
                return;
            using (InstanceMgr.syncLock.EnterWriteLock())
            {
                character.Instances.Character = (Character) null;
                InstanceMgr.OfflineLogs[character.EntityId.Low] = character.Instances;
            }
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Initialize Instances")]
        public static void Initialize()
        {
            InstanceMgr.InstanceInfos.Sort();
            InstanceConfig.LoadSettings();
            try
            {
                InstanceMgr.GlobalResetTimers = GlobalInstanceTimer.LoadTimers();
            }
            catch (Exception ex)
            {
                RealmDBMgr.OnDBError(ex);
                InstanceMgr.GlobalResetTimers = GlobalInstanceTimer.LoadTimers();
            }
        }

        public static DateTime GetNextResetTime(MapId id, uint difficultyIndex)
        {
            MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(id);
            if (mapTemplate != null)
                return InstanceMgr.GetNextResetTime(mapTemplate.GetDifficulty(difficultyIndex));
            return new DateTime();
        }

        public static DateTime GetNextResetTime(MapDifficultyEntry difficulty)
        {
            GlobalInstanceTimer globalResetTimer = InstanceMgr.GlobalResetTimers[(int) difficulty.Map.Id];
            if (globalResetTimer != null)
                return globalResetTimer.LastResets.Get<DateTime>(difficulty.Index)
                    .AddSeconds((double) difficulty.ResetTime);
            return new DateTime();
        }
    }
}