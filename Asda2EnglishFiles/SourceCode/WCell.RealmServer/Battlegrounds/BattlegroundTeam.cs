using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>Represents a team in a Battleground</summary>
    public class BattlegroundTeam : ICharacterSet, IPacketReceiver
    {
        private List<Character> _members = new List<Character>();
        public readonly BattlegroundTeamQueue Queue;
        public readonly BattlegroundSide Side;
        private Battleground _battleground;
        private int _count;
        private Vector3 _startPosition;
        private float _startOrientation;
        private int _reservedSlots;

        public BattlegroundTeam(BattlegroundTeamQueue queue, BattlegroundSide side, Battleground battleground)
        {
            this.Queue = queue;
            this.Side = side;
            this._battleground = battleground;
            BattlegroundTemplate template = battleground.Template;
            if (side == BattlegroundSide.Alliance)
            {
                this._startPosition = template.AllianceStartPosition;
                this._startOrientation = template.AllianceStartOrientation;
            }
            else
            {
                this._startPosition = template.HordeStartPosition;
                this._startOrientation = template.HordeStartOrientation;
            }
        }

        /// <summary>
        /// Amount of reserved slots.
        /// This is used to hold slots open for invited Players.
        /// </summary>
        public int ReservedSlots
        {
            get { return this._reservedSlots; }
            set { this._reservedSlots = value; }
        }

        /// <summary>
        /// The <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundTeam" /> which currently fights against this one
        /// in a <see cref="P:WCell.RealmServer.Battlegrounds.BattlegroundTeam.Battleground" />
        /// </summary>
        public BattlegroundTeam OpposingTeam
        {
            get { return this._battleground.GetTeam(this.Side.GetOppositeSide()); }
        }

        public Vector3 StartPosition
        {
            get { return this._startPosition; }
        }

        public float StartOrientation
        {
            get { return this._startOrientation; }
        }

        public Battleground Battleground
        {
            get { return this._battleground; }
        }

        /// <summary>
        /// A full team has reached its max player count: <see cref="F:WCell.RealmServer.Battlegrounds.BattlegroundTemplate.MaxPlayersPerTeam" />
        /// </summary>
        public bool IsFull
        {
            get { return this.CharacterCount + this._reservedSlots >= this._battleground.Template.MaxPlayersPerTeam; }
        }

        /// <summary>
        /// The amount of all Players in this team, including offline ones
        /// </summary>
        public int TotalCount
        {
            get { return this._members.Count; }
        }

        /// <summary>Amount of available slots (can be negative)</summary>
        public int OpenPlayerSlotCount
        {
            get
            {
                if (this._battleground.AddPlayersToBiggerTeam)
                    return this._battleground.Template.MaxPlayersPerTeam - this.CharacterCount - this._reservedSlots;
                return this.OpposingTeam.CharacterCount - this.CharacterCount - this._reservedSlots;
            }
        }

        /// <summary>The amount of online Characters</summary>
        public int CharacterCount
        {
            get { return this._count; }
        }

        public FactionGroup FactionGroup
        {
            get { return this.Side.GetFactionGroup(); }
        }

        /// <summary>Iterates over all online Characters in this team</summary>
        /// <param name="callback"></param>
        public void ForeachCharacter(Action<Character> callback)
        {
            this._battleground.EnsureContext();
            foreach (Character member in this._members)
                callback(member);
        }

        public Character[] GetAllCharacters()
        {
            this._battleground.EnsureContext();
            return this._members.ToArray();
        }

        public void AddMember(Character chr)
        {
            BattlegroundHandler.SendPlayerJoined((IPacketReceiver) this, chr);
            chr.Battlegrounds.Team = this;
            this._members.Add(chr);
            ++this._count;
        }

        public void RemoveMember(Character chr)
        {
            BattlegroundHandler.SendPlayerLeft((IPacketReceiver) this, chr);
            chr.Battlegrounds.Team = (BattlegroundTeam) null;
            this._members.Remove(chr);
            --this._count;
        }

        /// <summary>
        /// Distributes honor to Teammates within 40 yards of honorable kill.
        /// </summary>
        /// <param name="earner">Character that made the honorable kill.</param>
        /// <param name="honorPoints">Honor earned by the earner.</param>
        public void DistributeSharedHonor(Character earner, Character victim, uint honorPoints)
        {
            if (this.TotalCount < 1)
                return;
            uint bonus = honorPoints / (uint) this.TotalCount;
            this.ForeachCharacter((Action<Character>) (chr =>
            {
                if (!chr.IsInRange(new SimpleRange(0.0f, 40f), (WorldObject) earner))
                    return;
                chr.GiveHonorPoints(bonus);
                ++chr.KillsToday;
                ++chr.LifetimeHonorableKills;
                HonorHandler.SendPVPCredit((IPacketReceiver) chr, bonus * 10U, victim);
            }));
        }

        /// <summary>
        /// Make sure that Battleground.HasQueue is true before calling this method.
        /// Adds the given set of Characters to this team's queue.
        /// Will invite immediately if there are enough open slots and
        /// <see cref="P:WCell.RealmServer.Battlegrounds.Battleground.IsAddingPlayers" /> is true.
        /// </summary>
        /// <param name="chrs"></param>
        public BattlegroundRelation Enqueue(ICharacterSet chrs)
        {
            this._battleground.EnsureContext();
            BattlegroundRelation relation = new BattlegroundRelation(this.Queue, chrs);
            bool shouldInvite = this._battleground.IsAddingPlayers && chrs.CharacterCount <= this.OpenPlayerSlotCount;
            if (!shouldInvite)
            {
                this.Queue.Enqueue(relation);
            }
            else
            {
                this.ReservedSlots += chrs.CharacterCount;
                relation.IsEnqueued = false;
            }

            chrs.ForeachCharacter((Action<Character>) (chr => chr.ExecuteInContext((Action) (() =>
            {
                int queueIndex = chr.Battlegrounds.AddRelation(relation);
                if (!shouldInvite)
                    return;
                chr.Battlegrounds.InviteTo(this, queueIndex, relation);
            }))));
            return relation;
        }

        public int Invite(ICharacterSet chrs)
        {
            int added = 0;
            this.ReservedSlots += chrs.CharacterCount;
            chrs.ForeachCharacter((Action<Character>) (chr =>
            {
                if (!chr.IsInWorld)
                    return;
                chr.ExecuteInContext((Action) (() => chr.Battlegrounds.InviteTo(this)));
                ++added;
            }));
            return added;
        }

        public bool IsRussianClient { get; set; }

        public Locale Locale { get; set; }

        public void Send(RealmPacketOut packet, bool addEnd = false)
        {
            this.ForeachCharacter((Action<Character>) (chr => chr.Send(packet, addEnd)));
        }

        public override string ToString()
        {
            return ((int) this.Side).ToString() + " (" + (object) (this._count + this._reservedSlots) + "/" +
                   (object) this._battleground.Template.MaxPlayersPerTeam + ")";
        }

        public void Dispose()
        {
            this._members = (List<Character>) null;
            this._battleground = (Battleground) null;
        }
    }
}