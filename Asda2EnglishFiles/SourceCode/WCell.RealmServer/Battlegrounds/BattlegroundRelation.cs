using System;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>
    /// Represents the relation between one or multiple Character and a Battleground.
    /// This is enqueued in the <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundQueue" />s once a Character
    /// requests to join one.
    /// Each Character maintains the relation during the entire stay in a <see cref="T:WCell.RealmServer.Battlegrounds.Battleground" />.
    /// </summary>
    public class BattlegroundRelation : IBattlegroundRelation
    {
        private readonly DateTime _created;
        private readonly ICharacterSet _participants;
        private readonly BattlegroundTeamQueue _queue;

        public BattlegroundRelation(BattlegroundTeamQueue queue, ICharacterSet participants)
            : this(queue, participants, true)
        {
        }

        public BattlegroundRelation(BattlegroundTeamQueue queue, ICharacterSet participants, bool isEnqueued)
        {
            this._queue = queue;
            this._participants = participants;
            this.IsEnqueued = isEnqueued;
            this._created = DateTime.Now;
        }

        public DateTime Created
        {
            get { return this._created; }
        }

        public BattlegroundId BattlegroundId
        {
            get
            {
                BattlegroundQueue parentQueueBase = this._queue.ParentQueueBase;
                if (parentQueueBase != null)
                    return parentQueueBase.Template.Id;
                return BattlegroundId.End;
            }
        }

        public TimeSpan QueueTime
        {
            get { return DateTime.Now - this._created; }
        }

        /// <summary>Whether this is still enqueued</summary>
        public bool IsEnqueued { get; internal set; }

        public int Count
        {
            get { return this.Characters.CharacterCount; }
        }

        public BattlegroundTeamQueue Queue
        {
            get { return this._queue; }
        }

        public ICharacterSet Characters
        {
            get { return this._participants; }
        }

        internal void Cancel()
        {
            this._participants.ForeachCharacter((Action<Character>) (chr =>
                chr.Battlegrounds.CancelIfEnqueued(this.BattlegroundId)));
        }
    }
}