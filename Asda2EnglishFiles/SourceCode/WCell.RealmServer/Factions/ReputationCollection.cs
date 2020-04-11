using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Factions
{
    /// <summary>
    /// Represents the Reputation between a Player and all his known factions
    /// </summary>
    public class ReputationCollection
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Character m_owner;
        private Dictionary<FactionReputationIndex, Reputation> m_byIndex;

        public ReputationCollection(Character chr)
        {
            this.m_byIndex = new Dictionary<FactionReputationIndex, Reputation>();
            this.m_owner = chr;
        }

        public int Count
        {
            get { return this.m_byIndex.Count; }
        }

        public Character Owner
        {
            get { return this.m_owner; }
            set { this.m_owner = value; }
        }

        /// <summary>
        /// Initializes initial Factions of the owner (used when new Character is created)
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Loads all Factions that this Character already knows from the DB
        /// </summary>
        public void Load()
        {
            foreach (ReputationRecord record in ReputationRecord.Load(this.m_owner.Record.Guid))
            {
                Faction faction = FactionMgr.Get(record.ReputationIndex);
                if (faction != null)
                {
                    if (this.m_byIndex.ContainsKey(record.ReputationIndex))
                    {
                        ReputationCollection.log.Warn("Character {0} had Reputation with Faction {1} more than once.",
                            (object) this.m_owner, (object) record.ReputationIndex);
                    }
                    else
                    {
                        Reputation reputation = new Reputation(record, faction);
                        this.m_byIndex.Add(record.ReputationIndex, reputation);
                    }
                }
                else
                    ReputationCollection.log.Warn("Character {0} has saved Reputation with invalid Faction: {1}",
                        (object) this.m_owner, (object) record.ReputationIndex);
            }
        }

        /// <summary>Sends all existing factions to the Client</summary>
        public void ResendAllFactions()
        {
            foreach (Reputation rep in this.m_byIndex.Values)
                FactionHandler.SendReputationStandingUpdate((IPacketReceiver) this.m_owner.Client, rep);
        }

        public bool IsHostile(Faction faction)
        {
            Reputation reputation = this.GetOrCreate(faction.ReputationIndex);
            if (reputation != null)
                return reputation.Hostile;
            return false;
        }

        public bool CanAttack(Faction faction)
        {
            Reputation reputation = this.GetOrCreate(faction.ReputationIndex);
            if (reputation != null)
                return reputation.Hostile;
            return true;
        }

        public Reputation this[FactionReputationIndex key]
        {
            get
            {
                Reputation reputation;
                this.m_byIndex.TryGetValue(key, out reputation);
                return reputation;
            }
            set
            {
                throw new Exception(
                    "To modify the reputation with a specific faction, just modify the values of an already existing Reputation object.");
            }
        }

        /// <summary>
        /// Returns the corresponding Reputation object. Creates a new one
        /// if the player didn't meet this faction yet.
        /// </summary>
        /// <param name="reputationIndex">The repListId of the faction</param>
        internal Reputation GetOrCreate(FactionReputationIndex reputationIndex)
        {
            Reputation reputation;
            if (!this.m_byIndex.TryGetValue(reputationIndex, out reputation))
                reputation = this.Create(reputationIndex);
            return reputation;
        }

        /// <summary>
        /// Creates a Reputation object that represents the relation to the given faction, or null
        /// </summary>
        /// <param name="factionIndex">The repListId of the faction</param>
        private Reputation Create(FactionReputationIndex factionIndex)
        {
            Faction faction = FactionMgr.Get(factionIndex);
            if (faction != null)
                return this.Create(faction);
            return (Reputation) null;
        }

        /// <summary>
        /// Creates a Reputation object that represents the relation to the given faction, or null
        /// </summary>
        /// <param name="faction">The Faction which the Reputation should be with</param>
        private Reputation Create(Faction faction)
        {
            int defaultReputationValue = this.GetDefaultReputationValue(faction);
            ReputationFlags defaultReputationFlags = this.GetDefaultReputationFlags(faction);
            Reputation rep = new Reputation(this.m_owner.Record.CreateReputationRecord(), faction,
                defaultReputationValue, defaultReputationFlags);
            this.m_byIndex.Add(faction.ReputationIndex, rep);
            FactionHandler.SendReputationStandingUpdate((IPacketReceiver) this.m_owner.Client, rep);
            return rep;
        }

        private ReputationFlags GetDefaultReputationFlags(Faction faction)
        {
            FactionEntry entry = faction.Entry;
            for (int index = 0; index < 4; ++index)
            {
                if ((entry.ClassMask[index] == ClassMask.None ||
                     entry.ClassMask[index].HasAnyFlag(this.Owner.ClassMask)) &&
                    (entry.RaceMask[index] == ~RaceMask.AllRaces1 ||
                     entry.RaceMask[index].HasAnyFlag(this.Owner.RaceMask)))
                    return (ReputationFlags) entry.BaseFlags[index];
            }

            return ReputationFlags.None;
        }

        private int GetDefaultReputationValue(Faction faction)
        {
            FactionEntry entry = faction.Entry;
            for (int index = 0; index < 4; ++index)
            {
                if ((entry.ClassMask[index] == ClassMask.None ||
                     entry.ClassMask[index].HasAnyFlag(this.Owner.ClassMask)) &&
                    (entry.RaceMask[index] == ~RaceMask.AllRaces1 ||
                     entry.RaceMask[index].HasAnyFlag(this.Owner.RaceMask)))
                    return entry.BaseRepValue[index];
            }

            return 0;
        }

        public int GetValue(FactionReputationIndex reputationIndex)
        {
            Reputation reputation;
            if (this.m_byIndex.TryGetValue(reputationIndex, out reputation))
                return reputation.Value;
            return 0;
        }

        public Reputation SetValue(FactionReputationIndex reputationIndex, int value)
        {
            Reputation rep = this.GetOrCreate(reputationIndex);
            if (rep != null)
                this.SetValue(rep, value);
            return rep;
        }

        public void SetValue(Reputation rep, int value)
        {
            rep.SetValue(value);
            FactionHandler.SendReputationStandingUpdate((IPacketReceiver) this.m_owner.Client, rep);
        }

        public Reputation ModValue(FactionId factionId, int value)
        {
            return this.ModValue(FactionMgr.Get(factionId).ReputationIndex, value);
        }

        public Reputation ModValue(FactionReputationIndex reputationIndex, int value)
        {
            Reputation rep = this.GetOrCreate(reputationIndex);
            if (rep != null)
                this.ModValue(rep, value);
            return rep;
        }

        public void ModValue(Reputation rep, int value)
        {
            if (rep.SetValue(rep.Value + value))
            {
                if (rep.StandingLevel >= StandingLevel.Honored)
                    this.Owner.Achievements.CheckPossibleAchievementUpdates(
                        AchievementCriteriaType.GainHonoredReputation, 0U, 0U, (Unit) null);
                if (rep.StandingLevel >= StandingLevel.Revered)
                    this.Owner.Achievements.CheckPossibleAchievementUpdates(
                        AchievementCriteriaType.GainReveredReputation, 0U, 0U, (Unit) null);
                if (rep.StandingLevel >= StandingLevel.Exalted)
                    this.Owner.Achievements.CheckPossibleAchievementUpdates(
                        AchievementCriteriaType.GainExaltedReputation, 0U, 0U, (Unit) null);
                if (value > 0)
                    this.Owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.GainReputation, 0U,
                        0U, (Unit) null);
            }

            FactionHandler.SendReputationStandingUpdate((IPacketReceiver) this.m_owner.Client, rep);
        }

        public StandingLevel GetStandingLevel(FactionReputationIndex reputationIndex)
        {
            Reputation reputation;
            if (this.m_byIndex.TryGetValue(reputationIndex, out reputation))
                return reputation.StandingLevel;
            return StandingLevel.Hated;
        }

        /// <summary>Only called if the player declared war</summary>
        public void DeclareWar(FactionReputationIndex reputationIndex, bool hostile, bool sendUpdate)
        {
            Reputation rep = this.GetOrCreate(reputationIndex);
            if (rep.IsForcedAtPeace || rep.Faction.Group == this.m_owner.Faction.Group || rep.DeclaredWar == hostile)
                return;
            rep.DeclaredWar = hostile;
            if (!sendUpdate || !rep.DeclaredWar)
                return;
            FactionHandler.SendSetAtWar((IPacketReceiver) this.m_owner.Client, rep);
        }

        public void SetInactive(FactionReputationIndex reputationIndex, bool inactive)
        {
            Reputation reputation = this.GetOrCreate(reputationIndex);
            if (reputation == null)
                return;
            reputation.IsInactive = true;
        }

        /// <summary>
        /// For GMs/Testers: Introduces the char to all Factions and sets
        /// Reputation to max.
        /// </summary>
        public void LoveAll()
        {
            foreach (Faction faction in FactionMgr.ByReputationIndex)
            {
                if (faction != null)
                    this.SetValue(faction.ReputationIndex, 42999);
            }
        }

        /// <summary>
        /// For GMs/Testers: Introduces the char to all Factions and sets
        /// Reputation to min (oh boy are they gonna hate you).
        /// </summary>
        public void HateAll()
        {
            foreach (Faction faction in FactionMgr.ByReputationIndex)
            {
                if (faction != null)
                    this.SetValue(faction.ReputationIndex, -42000);
            }
        }

        /// <summary>
        /// Returns the cost of this item after the reputation discount has been applied.
        /// </summary>
        public uint GetDiscountedCost(FactionReputationIndex reputationIndex, uint cost)
        {
            StandingLevel standingLevel = this.GetStandingLevel(reputationIndex);
            return cost * (100U - Reputation.GetReputationDiscountPct(standingLevel)) / 100U;
        }

        /// <summary>Called when interacting with an NPC.</summary>
        public void OnTalkWith(NPC npc)
        {
            FactionReputationIndex reputationIndex = npc.Faction.ReputationIndex;
            if (reputationIndex < FactionReputationIndex.None || reputationIndex >= FactionReputationIndex.End)
                return;
            Reputation reputation = this.GetOrCreate(reputationIndex);
            if (reputation.IsForcedInvisible)
                return;
            reputation.IsVisible = true;
            this.Owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KnownFactions, 0U, 0U,
                (Unit) null);
            FactionHandler.SendVisible((IPacketReceiver) this.m_owner.Client, reputationIndex);
        }

        /// <summary>
        /// Increases or Decreases reputation with the given faction.
        /// </summary>
        /// <param name="factionId">Faction Id.</param>
        /// <param name="value">Amount to add or decrease</param>
        /// <returns></returns>
        public Reputation GainReputation(FactionId factionId, int value)
        {
            value += (int) Math.Round((double) (value * this.m_owner.ReputationGainModifierPercent) / 100.0);
            return this.ModValue(factionId, value);
        }

        public uint GetVisibleReputations()
        {
            uint num = 0;
            foreach (Reputation reputation in this.m_byIndex.Values)
            {
                if (reputation.IsVisible)
                    ++num;
            }

            return num;
        }

        public uint GetHonoredReputations()
        {
            uint num = 0;
            foreach (Reputation reputation in this.m_byIndex.Values)
            {
                if (reputation.StandingLevel >= StandingLevel.Honored)
                    ++num;
            }

            return num;
        }

        public uint GetReveredReputations()
        {
            uint num = 0;
            foreach (Reputation reputation in this.m_byIndex.Values)
            {
                if (reputation.StandingLevel >= StandingLevel.Revered)
                    ++num;
            }

            return num;
        }

        public uint GetExaltedReputations()
        {
            uint num = 0;
            foreach (Reputation reputation in this.m_byIndex.Values)
            {
                if (reputation.StandingLevel >= StandingLevel.Exalted)
                    ++num;
            }

            return num;
        }
    }
}