using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.Constants.Skills;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
    /// <summary>
    /// Represents Lock information that are applied to lockable GOs and Items
    /// TODO: Check whether we need to send a animations when interaction with certain LockTypes
    /// </summary>
    public class LockEntry
    {
        internal static int LockInteractionLength = (int) (Utility.GetMaxEnum<LockInteractionType>() + 1);
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static readonly SkillId[] InteractionSkills = new SkillId[LockEntry.LockInteractionLength];

        public static readonly LockEntry.InteractionHandler[] InteractionHandlers =
            new LockEntry.InteractionHandler[LockEntry.LockInteractionLength];

        /// <summary>All Lock-entries, indexed by their id</summary>
        public static readonly LockEntry[] Entries = new LockEntry[2000];

        public readonly uint Id;

        /// <summary>
        /// 0 or more professions that can be used to open the Lock
        /// </summary>
        public LockOpeningMethod[] OpeningMethods;

        /// <summary>
        /// 0 or more key-entries that can be used to open the Lock
        /// </summary>
        public LockKeyEntry[] Keys;

        /// <summary>Whether the lock requires kneeling when being used</summary>
        public bool RequiresKneeling;

        /// <summary>
        /// Whether the user swings at the object when using its lock
        /// </summary>
        public bool RequiresAttack;

        /// <summary>
        /// 
        /// </summary>
        public bool RequiresVehicle;

        /// <summary>
        /// Whether the lock does not require any skill or key to be opened.
        /// </summary>
        public bool IsUnlocked;

        /// <summary>Whether the lock can be closed</summary>
        public bool CanBeClosed;

        public static void Handle(Character chr, ILockable lockable, LockInteractionType type)
        {
            LockEntry.InteractionHandler interactionHandler =
                LockEntry.InteractionHandlers.Get<LockEntry.InteractionHandler>((uint) type);
            if (interactionHandler != null)
                interactionHandler(lockable, chr);
            else
                LockEntry.log.Error(
                    "{0} trying to interact with lockable \"{1}\", but the used InteractionType \"{2}\" is not handled.",
                    (object) chr, (object) lockable, (object) type);
        }

        public LockEntry(uint id)
        {
            this.Id = id;
        }

        public override string ToString()
        {
            List<string> collection = new List<string>();
            if (this.OpeningMethods.Length > 0)
                collection.Add("Opening Methods: " +
                               ((IEnumerable<LockOpeningMethod>) this.OpeningMethods)
                               .ToString<LockOpeningMethod>(", "));
            if (this.Keys.Length > 0)
                collection.Add("Keys: " + ((IEnumerable<LockKeyEntry>) this.Keys).ToString<LockKeyEntry>(", "));
            else if (this.IsUnlocked)
                collection.Add("Unlocked");
            if (this.CanBeClosed)
                collection.Add("Closable");
            return collection.ToString<string>("; ");
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.First, null)]
        public static void Initialize()
        {
            if (((IEnumerable<LockEntry.InteractionHandler>) LockEntry.InteractionHandlers)
                .First<LockEntry.InteractionHandler>() != null)
                return;
            LockEntry.InitTypes();
        }

        private static void InitTypes()
        {
            LockEntry.InteractionSkills[16] = SkillId.Engineering;
            LockEntry.InteractionSkills[19] = SkillId.Fishing;
            LockEntry.InteractionSkills[2] = SkillId.Herbalism;
            LockEntry.InteractionSkills[3] = SkillId.Mining;
            LockEntry.InteractionSkills[1] = SkillId.Lockpicking;
            LockEntry.InteractionSkills[20] = SkillId.Inscription;
            LockEntry.InteractionHandlers[16] = new LockEntry.InteractionHandler(LockEntry.Loot);
            LockEntry.InteractionHandlers[8] = new LockEntry.InteractionHandler(LockEntry.Close);
            LockEntry.InteractionHandlers[4] = new LockEntry.InteractionHandler(LockEntry.DisarmTrap);
            LockEntry.InteractionHandlers[19] = new LockEntry.InteractionHandler(LockEntry.Loot);
            LockEntry.InteractionHandlers[2] = new LockEntry.InteractionHandler(LockEntry.Loot);
            LockEntry.InteractionHandlers[3] = new LockEntry.InteractionHandler(LockEntry.Loot);
            LockEntry.InteractionHandlers[0] = new LockEntry.InteractionHandler(LockEntry.Open);
            LockEntry.InteractionHandlers[5] = new LockEntry.InteractionHandler(LockEntry.Open);
            LockEntry.InteractionHandlers[14] = new LockEntry.InteractionHandler(LockEntry.Open);
            LockEntry.InteractionHandlers[13] = new LockEntry.InteractionHandler(LockEntry.Open);
            LockEntry.InteractionHandlers[12] = new LockEntry.InteractionHandler(LockEntry.Open);
            LockEntry.InteractionHandlers[1] = new LockEntry.InteractionHandler(LockEntry.Loot);
            LockEntry.InteractionHandlers[11] = new LockEntry.InteractionHandler(LockEntry.Close);
            LockEntry.InteractionHandlers[10] = new LockEntry.InteractionHandler(LockEntry.Open);
            LockEntry.InteractionHandlers[18] = new LockEntry.InteractionHandler(LockEntry.Close);
        }

        private static void LoadLocks()
        {
            MappedDBCReader<LockEntry, LockEntry.LockConverter> mappedDbcReader =
                new MappedDBCReader<LockEntry, LockEntry.LockConverter>(
                    RealmServerConfiguration.GetDBCFile("Lock.dbc"));
        }

        /// <summary>
        /// Whether this lock can be interacted with, using the given type
        /// </summary>
        public bool Supports(LockInteractionType type)
        {
            foreach (LockOpeningMethod openingMethod in this.OpeningMethods)
            {
                if (openingMethod.InteractionType == type)
                    return true;
            }

            return false;
        }

        private static void BreakOpen(ILockable lockable, Character user)
        {
        }

        /// <summary>Close an object</summary>
        public static void Close(ILockable lockable, Character user)
        {
            if (!(lockable is GameObject))
                return;
            GameObject go = lockable as GameObject;
            go.State = GameObjectState.Disabled;
            if (!(go.Map is Battleground))
                return;
            (go.Map as Battleground).OnPlayerClickedOnflag(go, user);
        }

        /// <summary>Disarm a trap</summary>
        public static void DisarmTrap(ILockable trap, Character user)
        {
        }

        /// <summary>Loot a container's contents</summary>
        public static void Loot(ILockable lockable, Character user)
        {
            if (lockable is Item)
                LootMgr.CreateAndSendObjectLoot((ILootable) lockable, user, LootEntryType.Item, user.Map.IsHeroic);
            else if (lockable is GameObject)
                ((GameObject) lockable).Handler.Use(user);
            else
                LockEntry.log.Error("{0} tried to loot invalid object: " + (object) lockable, (object) user);
        }

        /// <summary>Open a GameObject</summary>
        public static void Open(ILockable lockable, Character chr)
        {
            if (!(lockable is GameObject))
                return;
            GameObject go = lockable as GameObject;
            go.Use(chr);
            if (!(go.Map is Battleground))
                return;
            (go.Map as Battleground).OnPlayerClickedOnflag(go, chr);
        }

        public delegate void InteractionHandler(ILockable lockable, Character user);

        private class LockConverter : AdvancedDBCRecordConverter<LockEntry>
        {
            public override LockEntry ConvertTo(byte[] rawData, ref int id)
            {
                LockEntry lockEntry = new LockEntry((uint) (id = rawData.GetInt32(0U)));
                List<LockOpeningMethod> lockOpeningMethodList = new List<LockOpeningMethod>(5);
                List<LockKeyEntry> lockKeyEntryList = new List<LockKeyEntry>(5);
                uint num = 1;
                uint field1 = 9;
                uint field2 = 17;
                for (uint index = 0; index < 5U; ++index)
                {
                    switch ((LockInteractionGroup) rawData.GetUInt32(num++))
                    {
                        case LockInteractionGroup.Key:
                            uint uint32_1 = rawData.GetUInt32(field1);
                            lockKeyEntryList.Add(new LockKeyEntry(index, uint32_1));
                            goto default;
                        case LockInteractionGroup.Profession:
                            LockInteractionType uint32_2 = (LockInteractionType) rawData.GetUInt32(field1);
                            switch (uint32_2)
                            {
                                case LockInteractionType.None:
                                    continue;
                                case LockInteractionType.Close:
                                case LockInteractionType.QuickClose:
                                case LockInteractionType.PvPClose:
                                    lockEntry.CanBeClosed = true;
                                    goto label_9;
                                case LockInteractionType.OpenKneeling:
                                    lockEntry.RequiresKneeling = true;
                                    goto label_9;
                                case LockInteractionType.OpenAttacking:
                                    lockEntry.RequiresAttack = true;
                                    goto label_9;
                                default:
                                    SkillId interactionSkill = LockEntry.InteractionSkills[(uint) uint32_2];
                                    if (interactionSkill != SkillId.None)
                                    {
                                        lockOpeningMethodList.Add(new LockOpeningMethod(index)
                                        {
                                            InteractionType = uint32_2,
                                            RequiredSkill = interactionSkill,
                                            RequiredSkillValue = rawData.GetUInt32(field2)
                                        });
                                        goto label_9;
                                    }
                                    else
                                        goto label_9;
                            }
                        default:
                            label_9:
                            ++field1;
                            ++field2;
                            break;
                    }
                }

                lockEntry.IsUnlocked = lockOpeningMethodList.Count == 0 && lockKeyEntryList.Count == 0;
                lockEntry.Keys = lockKeyEntryList.ToArray();
                lockEntry.OpeningMethods = lockOpeningMethodList.ToArray();
                LockEntry.Entries[lockEntry.Id] = lockEntry;
                return lockEntry;
            }
        }
    }
}