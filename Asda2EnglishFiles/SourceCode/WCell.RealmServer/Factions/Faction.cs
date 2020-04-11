using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Factions
{
    /// <summary>
    /// TODO: Load faction info completely at startup to avoid synchronization issues
    /// </summary>
    [Serializable]
    public class Faction
    {
        public static readonly HashSet<Faction> EmptySet = new HashSet<Faction>();
        public static readonly Faction NullFaction = new Faction(Faction.EmptySet, Faction.EmptySet, Faction.EmptySet);
        public readonly HashSet<Faction> Enemies = new HashSet<Faction>();
        public readonly HashSet<Faction> Friends = new HashSet<Faction>();
        public readonly HashSet<Faction> Neutrals = new HashSet<Faction>();
        public List<Faction> Children = new List<Faction>();
        public FactionGroup Group;
        public FactionEntry Entry;
        public readonly FactionTemplateEntry Template;
        public FactionId Id;
        public FactionReputationIndex ReputationIndex;

        /// <summary>whether this is a Player faction</summary>
        public bool IsPlayer;

        /// <summary>whether this is the Alliance or an Alliance faction</summary>
        public bool IsAlliance;

        /// <summary>whether this is the Horde or a Horde faction</summary>
        public bool IsHorde;

        /// <summary>
        /// whether this is a neutral faction (always stays neutral).
        /// </summary>
        public bool IsNeutral;

        /// <summary>
        /// Default ctor can be used for customizing your own Faction
        /// </summary>
        public Faction()
        {
            this.Entry = new FactionEntry()
            {
                Name = "Null Faction"
            };
            this.Template = new FactionTemplateEntry()
            {
                EnemyFactions = new FactionId[0],
                FriendlyFactions = new FactionId[0]
            };
        }

        private Faction(HashSet<Faction> enemies, HashSet<Faction> friends, HashSet<Faction> neutrals)
        {
            this.Enemies = enemies;
            this.Friends = friends;
            this.Neutrals = neutrals;
            this.Entry = new FactionEntry()
            {
                Name = "Null Faction"
            };
            this.Template = new FactionTemplateEntry()
            {
                EnemyFactions = new FactionId[0],
                FriendlyFactions = new FactionId[0]
            };
        }

        public Faction(FactionEntry entry, FactionTemplateEntry template)
        {
            this.Entry = entry;
            this.Template = template;
            this.Id = entry.Id;
            this.ReputationIndex = entry.FactionIndex;
            this.IsAlliance = template.FactionGroup.HasFlag((Enum) FactionGroupMask.Alliance);
            this.IsHorde = template.FactionGroup.HasFlag((Enum) FactionGroupMask.Horde);
        }

        internal void Init()
        {
            if (this.Id == FactionId.Alliance || this.Entry.ParentId == FactionId.Alliance)
            {
                this.IsAlliance = true;
                this.Group = FactionGroup.Alliance;
            }
            else if (this.Id == FactionId.Horde || this.Entry.ParentId == FactionId.Horde)
            {
                this.IsHorde = true;
                this.Group = FactionGroup.Horde;
            }

            foreach (Faction faction in ((IEnumerable<Faction>) FactionMgr.ByTemplateId).Where<Faction>(
                (Func<Faction, bool>) (faction => faction != null)))
            {
                if (this.IsPlayer && faction.Template.FriendGroup.HasAnyFlag(FactionGroupMask.Player))
                {
                    this.Friends.Add(faction);
                    faction.Friends.Add(this);
                }

                if (this.Template.FriendGroup.HasAnyFlag(faction.Template.FactionGroup))
                    this.Friends.Add(faction);
            }

            foreach (FactionId friendlyFaction in this.Template.FriendlyFactions)
            {
                Faction faction = FactionMgr.Get(friendlyFaction);
                if (faction != null)
                {
                    this.Friends.Add(faction);
                    faction.Friends.Add(this);
                }
            }

            this.Friends.Add(this);
            foreach (Faction faction in ((IEnumerable<Faction>) FactionMgr.ByTemplateId).Where<Faction>(
                (Func<Faction, bool>) (faction => faction != null)))
            {
                if (this.IsPlayer && faction.Template.EnemyGroup.HasAnyFlag(FactionGroupMask.Player))
                {
                    this.Enemies.Add(faction);
                    faction.Enemies.Add(this);
                }

                if (this.Template.EnemyGroup.HasAnyFlag(faction.Template.FactionGroup))
                    this.Enemies.Add(faction);
            }

            foreach (FactionId enemyFaction in this.Template.EnemyFactions)
            {
                Faction faction = FactionMgr.Get(enemyFaction);
                if (faction != null)
                {
                    if (!this.Template.Flags.HasAnyFlag(FactionTemplateFlags.Flagx400))
                        this.Enemies.Add(faction);
                    faction.Enemies.Add(this);
                }
            }

            foreach (Faction faction in ((IEnumerable<Faction>) FactionMgr.ByTemplateId).Where<Faction>(
                (Func<Faction, bool>) (faction => faction != null)))
            {
                if (!this.Friends.Contains(faction) && !this.Enemies.Contains(faction))
                    this.Neutrals.Add(faction);
            }

            if (this.Id == FactionId.Prey)
                this.Enemies.Clear();
            this.IsNeutral = this.Enemies.Count == 0 && this.Friends.Count == 0;
        }

        /// <summary>Make this an alliance player faction</summary>
        internal void SetAlliancePlayer()
        {
            this.IsPlayer = true;
            this.Entry.ParentId = FactionId.Alliance;
            FactionMgr.AlliancePlayerFactions.Add(this);
        }

        /// <summary>Make this a horde player faction</summary>
        internal void SetHordePlayer()
        {
            this.IsPlayer = true;
            this.Entry.ParentId = FactionId.Horde;
            FactionMgr.HordePlayerFactions.Add(this);
        }

        public bool IsHostileTowards(Faction otherFaction)
        {
            if (this.Enemies.Contains(otherFaction))
                return true;
            if (this.Template.EnemyFactions.Length > 0)
            {
                for (int index = 0; index < this.Template.EnemyFactions.Length; ++index)
                {
                    if (this.Template.EnemyFactions[index] == otherFaction.Template.FactionId)
                        return true;
                    if (this.Template.FriendlyFactions[index] == otherFaction.Template.FactionId)
                        return false;
                }
            }

            return this.Template.EnemyGroup.HasAnyFlag(otherFaction.Template.FactionGroup);
        }

        public bool IsNeutralWith(Faction otherFaction)
        {
            return this.Neutrals.Contains(otherFaction);
        }

        public bool IsFriendlyTowards(Faction otherFaction)
        {
            if (this.Friends.Contains(otherFaction))
                return true;
            if (this.Template.EnemyFactions.Length > 0)
            {
                for (int index = 0; index < this.Template.FriendlyFactions.Length; ++index)
                {
                    if (this.Template.FriendlyFactions[index] == otherFaction.Template.FactionId)
                        return true;
                    if (this.Template.EnemyFactions[index] == otherFaction.Template.FactionId)
                        return false;
                }
            }

            return this.Template.FriendGroup.HasAnyFlag(otherFaction.Template.FactionGroup);
        }

        public override int GetHashCode()
        {
            return (int) this.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is Faction)
                return this.Id == ((Faction) obj).Id;
            return false;
        }

        public override string ToString()
        {
            return this.Entry.Name + " (" + (object) (int) this.Id + ")";
        }
    }
}