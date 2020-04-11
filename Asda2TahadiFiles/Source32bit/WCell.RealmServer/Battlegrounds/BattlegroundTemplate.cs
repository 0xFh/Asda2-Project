using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>Battleground information</summary>
    [Serializable]
    public class BattlegroundTemplate : IDataHolder
    {
        public BattlegroundId Id;
        [NotPersistent] public MapId MapId;
        public int MinPlayersPerTeam;
        public int MaxPlayersPerTeam;
        public int MinLevel;
        public int MaxLevel;

        /// <summary>Load pos from DBCs</summary>
        public int AllianceStartPosIndex;

        /// <summary>Load pos from DBCs</summary>
        public int HordeStartPosIndex;

        public Vector3 AllianceStartPosition;
        public Vector3 HordeStartPosition;
        public float AllianceStartOrientation;
        public float HordeStartOrientation;
        [NotPersistent] public BattlegroundCreator Creator;
        [NotPersistent] public MapTemplate MapTemplate;
        [NotPersistent] public GlobalBattlegroundQueue[] Queues;
        [NotPersistent] public PvPDifficultyEntry[] Difficulties;

        public int MinPlayerCount
        {
            get { return this.MinPlayersPerTeam * 2; }
        }

        public uint GetId()
        {
            return (uint) this.Id;
        }

        public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
            this.MapId = BattlegroundMgr.BattlemasterListReader.Entries[(int) this.Id].MapId;
            this.MapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(this.MapId);
            if (this.MapTemplate == null)
                ContentMgr.OnInvalidDBData("BattlegroundTemplate had invalid MapId: {0} (#{1})", (object) this.MapId,
                    (object) this.MapId);
            else if (BattlegroundMgr.Templates.Length <= (int) this.Id)
            {
                ContentMgr.OnInvalidDBData("BattlegroundTemplate had invalid BG-Id: {0} (#{1})", (object) this.Id,
                    (object) (int) this.Id);
            }
            else
            {
                this.MapTemplate.BattlegroundTemplate = this;
                this.Difficulties =
                    new PvPDifficultyEntry[BattlegroundMgr.PVPDifficultyReader.Entries.Values.Count<PvPDifficultyEntry>(
                        (Func<PvPDifficultyEntry, bool>) (entry => entry.mapId == this.MapId))];
                foreach (PvPDifficultyEntry pdifficultyEntry in BattlegroundMgr.PVPDifficultyReader.Entries.Values
                    .Where<PvPDifficultyEntry>((Func<PvPDifficultyEntry, bool>) (entry => entry.mapId == this.MapId)))
                    this.Difficulties[pdifficultyEntry.bracketId] = pdifficultyEntry;
                this.MinLevel = this.MapTemplate.MinLevel = ((IEnumerable<PvPDifficultyEntry>) this.Difficulties)
                    .First<PvPDifficultyEntry>().minLevel;
                this.MaxLevel = this.MapTemplate.MaxLevel = ((IEnumerable<PvPDifficultyEntry>) this.Difficulties)
                    .Last<PvPDifficultyEntry>().maxLevel;
                BattlegroundMgr.Templates[(int) this.Id] = this;
                this.CreateQueues();
                this.SetStartPos();
            }
        }

        public int GetBracketIdForLevel(int level)
        {
            PvPDifficultyEntry pdifficultyEntry =
                ((IEnumerable<PvPDifficultyEntry>) this.Difficulties).FirstOrDefault<PvPDifficultyEntry>(
                    (Func<PvPDifficultyEntry, bool>) (entry =>
                    {
                        if (level >= entry.minLevel)
                            return level <= entry.maxLevel;
                        return false;
                    }));
            if (pdifficultyEntry != null)
                return pdifficultyEntry.bracketId;
            return -1;
        }

        private void CreateQueues()
        {
            this.Queues = new GlobalBattlegroundQueue[this.Difficulties.Length];
            foreach (PvPDifficultyEntry difficulty in this.Difficulties)
            {
                if (difficulty != null)
                    this.AddQueue(new GlobalBattlegroundQueue(this, difficulty.bracketId, difficulty.minLevel,
                        difficulty.maxLevel));
            }
        }

        private void AddQueue(GlobalBattlegroundQueue queue)
        {
            this.Queues[queue.BracketId] = queue;
        }

        /// <summary>Gets the appropriate queue for the given character.</summary>
        /// <param name="chr">the character</param>
        /// <returns>the appropriate queue for the given character</returns>
        public GlobalBattlegroundQueue GetQueue(Character character)
        {
            return this.GetQueue(character.Level);
        }

        /// <summary>
        /// Gets the appropriate queue for a character of the given level.
        /// </summary>
        /// <param name="level">the level of the character</param>
        /// <returns>the appropriate queue for the given character level</returns>
        public GlobalBattlegroundQueue GetQueue(int level)
        {
            return this.Queues.Get<GlobalBattlegroundQueue>((uint) this.GetBracketIdForLevel(level));
        }

        /// <summary>
        /// Enqueues a single Character to the global Queue of this Template
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public BattlegroundRelation EnqueueCharacter(Character chr, BattlegroundSide side)
        {
            return this.GetQueue(chr).Enqueue((ICharacterSet) chr, side);
        }

        /// <summary>
        /// Tries to enqueue the given Character or -if specified- his/her entire Group
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="asGroup"></param>
        public void TryEnqueue(Character chr, bool asGroup)
        {
            this.TryEnqueue(chr, asGroup, chr.FactionGroup.GetBattlegroundSide());
        }

        /// <summary>
        /// Tries to enqueue the given Character or -if specified- his/her entire Group
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="asGroup"></param>
        public void TryEnqueue(Character chr, bool asGroup, BattlegroundSide side)
        {
            this.TryEnqueue(chr, asGroup, 0U, side);
        }

        /// <summary>
        /// Tries to enqueue the given Character or -if specified- his/her entire Group
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="asGroup"></param>
        public void TryEnqueue(Character chr, bool asGroup, uint instanceId)
        {
            this.TryEnqueue(chr, asGroup, instanceId, chr.FactionGroup.GetBattlegroundSide());
        }

        /// <summary>
        /// Tries to enqueue the given character or his whole group for the given battleground.
        /// </summary>
        /// <param name="chr">the character who enqueued</param>
        /// <param name="asGroup">whether or not to enqueue the character or his/her group</param>
        public void TryEnqueue(Character chr, bool asGroup, uint instanceId, BattlegroundSide side)
        {
            if (instanceId != 0U)
            {
                GlobalBattlegroundQueue queue = this.GetQueue(chr);
                if (queue == null)
                    return;
                Battleground battleground = queue.GetBattleground(instanceId);
                if (battleground == null || !battleground.HasQueue)
                    return;
                battleground.TryJoin(chr, asGroup, side);
            }
            else
                this.GetQueue(chr).GetTeamQueue(side).Enqueue(chr, asGroup);
        }

        public override string ToString()
        {
            return this.GetType().Name + string.Format(" (Id: {0} (#{1}), Map: {2} (#{3})", (object) this.Id,
                       (object) (int) this.Id, (object) this.MapId, (object) this.MapId);
        }

        public void SetStartPos()
        {
            WorldSafeLocation worldSafeLocation1;
            BattlegroundMgr.WorldSafeLocs.TryGetValue(this.AllianceStartPosIndex, out worldSafeLocation1);
            if (worldSafeLocation1 != null)
            {
                Vector3 vector3 = new Vector3(worldSafeLocation1.X, worldSafeLocation1.Y, worldSafeLocation1.Z);
                if ((double) vector3.X != 0.0 && (double) vector3.Y != 0.0 && (double) vector3.Z != 0.0)
                    this.AllianceStartPosition = vector3;
            }

            WorldSafeLocation worldSafeLocation2;
            BattlegroundMgr.WorldSafeLocs.TryGetValue(this.HordeStartPosIndex, out worldSafeLocation2);
            if (worldSafeLocation2 == null)
                return;
            Vector3 vector3_1 = new Vector3(worldSafeLocation2.X, worldSafeLocation2.Y, worldSafeLocation2.Z);
            if ((double) vector3_1.X == 0.0 || (double) vector3_1.Y == 0.0 || (double) vector3_1.Z == 0.0)
                return;
            this.HordeStartPosition = vector3_1;
        }
    }
}