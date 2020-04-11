using System;
using WCell.Constants.Factions;
using WCell.RealmServer.Database;
using WCell.Util;

namespace WCell.RealmServer.Factions
{
    public class Reputation
    {
        /// <summary>Discounts indexed by StandingLevel</summary>
        public static uint[] DiscountPercents = new uint[8]
        {
            0U,
            0U,
            0U,
            0U,
            5U,
            10U,
            15U,
            20U
        };

        public static readonly Standing[] Standings = (Standing[]) Enum.GetValues(typeof(Standing));
        public const int Max = 42999;
        public const int Min = -42000;
        private Standing m_standing;
        private readonly ReputationRecord m_record;
        public readonly Faction Faction;

        /// <summary>Loads an existing Reputation from the given Record.</summary>
        public Reputation(ReputationRecord record, Faction faction)
        {
            this.m_record = record;
            this.Faction = faction;
            this.m_standing = Reputation.GetStanding(record.Value);
        }

        public Reputation(ReputationRecord record, Faction faction, int defaultValue, ReputationFlags defaultFlags)
        {
            this.m_record = record;
            this.m_record.ReputationIndex = faction.ReputationIndex;
            this.m_record.Value = defaultValue;
            this.m_record.Flags = defaultFlags;
            this.Faction = faction;
            this.m_standing = Reputation.GetStanding(defaultValue);
            this.m_record.Save();
        }

        /// <summary>The reputation value</summary>
        public int Value
        {
            get { return this.m_record.Value; }
        }

        public Standing Standing
        {
            get { return this.m_standing; }
        }

        /// <summary>Exalted, Honored, Neutral, Hated</summary>
        public StandingLevel StandingLevel
        {
            get { return Reputation.GetStandingLevel(this.m_record.Value); }
        }

        public ReputationFlags Flags
        {
            get { return this.m_record.Flags; }
        }

        /// <summary>
        /// Whether racial and faction mounts/tabards etc can be purchased.
        /// </summary>
        public bool SpecialItems
        {
            get { return this.m_standing >= Standing.Exalted; }
        }

        /// <summary>
        /// Whether Heroic mode keys can be purchased for Outland dungeons.
        /// <see href="http://www.wowwiki.com/Heroic" />
        /// </summary>
        public bool HeroicModeAllowed
        {
            get { return this.m_standing >= Standing.Honored; }
        }

        /// <summary>
        /// Enough reputation to interact with NPCs of that Faction
        /// </summary>
        public bool CanInteract
        {
            get { return this.m_standing >= Standing.Neutral; }
        }

        /// <summary>
        /// Either very bad rep or the player declared war.
        /// Will cause mobs to attack on sight.
        /// </summary>
        public bool Hostile
        {
            get
            {
                if (!this.DeclaredWar)
                    return Reputation.IsHostileStanding(this.m_standing);
                return true;
            }
        }

        public bool IsVisible
        {
            get { return this.m_record.Flags.HasFlag((Enum) ReputationFlags.Visible); }
            internal set
            {
                if (this.IsForcedInvisible)
                    return;
                if (value)
                    this.m_record.Flags |= ReputationFlags.Visible;
                else
                    this.m_record.Flags &= ~ReputationFlags.Visible;
            }
        }

        /// <summary>whether the player actively declared war</summary>
        public bool DeclaredWar
        {
            get { return this.m_record.Flags.HasFlag((Enum) ReputationFlags.AtWar); }
            internal set
            {
                if (this.IsForcedAtPeace)
                    return;
                if (value)
                    this.m_record.Flags |= ReputationFlags.AtWar;
                else
                    this.m_record.Flags &= ~ReputationFlags.AtWar;
            }
        }

        public bool IsHidden
        {
            get { return this.m_record.Flags.HasFlag((Enum) ReputationFlags.Hidden); }
            set
            {
                if (value)
                    this.m_record.Flags |= ReputationFlags.Hidden;
                else
                    this.m_record.Flags &= ~ReputationFlags.Hidden;
            }
        }

        public bool IsForcedInvisible
        {
            get { return this.m_record.Flags.HasFlag((Enum) ReputationFlags.ForcedInvisible); }
            internal set
            {
                if (value)
                    this.m_record.Flags |= ReputationFlags.ForcedInvisible;
                else
                    this.m_record.Flags &= ~ReputationFlags.ForcedInvisible;
            }
        }

        public bool IsForcedAtPeace
        {
            get { return this.m_record.Flags.HasFlag((Enum) ReputationFlags.ForcedPeace); }
            internal set
            {
                if (value)
                    this.m_record.Flags |= ReputationFlags.ForcedPeace;
                else
                    this.m_record.Flags &= ~ReputationFlags.ForcedPeace;
            }
        }

        public bool IsInactive
        {
            get { return this.m_record.Flags.HasFlag((Enum) ReputationFlags.Inactive); }
            set
            {
                if (value)
                    this.m_record.Flags |= ReputationFlags.Inactive;
                else
                    this.m_record.Flags &= ~ReputationFlags.Inactive;
            }
        }

        /// <summary>
        /// Changes the reputation value with a specific Faction.
        /// Is called by ReputationCollect.SetValue
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Whether hostility changed due to the stending change</returns>
        internal bool SetValue(int value)
        {
            Standing standing = this.m_standing;
            bool hostile1 = this.Hostile;
            this.m_standing = Reputation.GetStanding(value);
            bool hostile2 = this.Hostile;
            this.m_record.Value = value;
            if (standing != this.m_standing)
                return hostile1 != hostile2;
            return false;
        }

        static Reputation()
        {
            Array.Sort<Standing>(Reputation.Standings);
        }

        public static Standing GetStanding(int repValue)
        {
            for (int index = Reputation.Standings.Length - 1; index >= 0; --index)
            {
                if ((Standing) repValue >= Reputation.Standings[index])
                    return Reputation.Standings[index];
            }

            return Standing.Hated;
        }

        public static StandingLevel GetStandingLevel(int repValue)
        {
            for (int index = 0; index < Reputation.Standings.Length; ++index)
            {
                if ((Standing) repValue >= Reputation.Standings[index])
                    return (StandingLevel) (Reputation.Standings.Length - index);
            }

            return StandingLevel.Hated;
        }

        public static bool IsHostileStanding(Standing standing)
        {
            return standing <= Standing.Hostile;
        }

        public static uint GetReputationDiscountPct(StandingLevel lvl)
        {
            return Reputation.DiscountPercents.Get<uint>((uint) lvl);
        }
    }
}