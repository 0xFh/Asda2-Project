using System;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>
    /// Contains all the neccessary battleground information about a player.
    /// </summary>
    public class BattlegroundInfo
    {
        private Character _chr;
        private BattlegroundRelation[] _relations;
        private BattlegroundInvitation _invitation;
        private int _relationCount;
        private Map m_EntryMap;
        private Vector3 _entryPosition;
        private float _entryOrientation;
        private bool _isDeserter;
        private BattlegroundTeam _team;
        private BattlegroundStats _stats;

        public BattlegroundInfo(Character chr)
        {
            this._chr = chr;
            this._relations = new BattlegroundRelation[BattlegroundMgr.MaxQueuesPerChar];
        }

        /// <summary>The character this information is associated with.</summary>
        public Character Character
        {
            get { return this._chr; }
            internal set { this._chr = value; }
        }

        /// <summary>
        /// The battlegrounds team this character is associated with.
        /// </summary>
        public BattlegroundTeam Team
        {
            get { return this._team; }
            internal set
            {
                this._team = value;
                this._chr.Record.BattlegroundTeam = value == null ? BattlegroundSide.End : value.Side;
            }
        }

        /// <summary>Stats of current or last Battleground (or null)</summary>
        public BattlegroundStats Stats
        {
            get { return this._stats; }
            internal set { this._stats = value; }
        }

        /// <summary>
        /// Holds the outstanding, if any, invitation to a battleground team.
        /// </summary>
        public BattlegroundInvitation Invitation
        {
            get { return this._invitation; }
            internal set
            {
                if (value == this._invitation)
                    return;
                if (value == null)
                    this._chr.RemoveUpdateAction((ObjectUpdateTimer) this._invitation.CancelTimer);
                this._invitation = value;
            }
        }

        /// <summary>
        /// The map that this character was originally in before going to the battlegrounds.
        /// </summary>
        public Map EntryMap
        {
            get { return this.m_EntryMap; }
            set { this.m_EntryMap = value; }
        }

        /// <summary>
        /// The position that this character was originally at before going to the battlegrounds.
        /// </summary>
        public Vector3 EntryPosition
        {
            get { return this._entryPosition; }
            set { this._entryPosition = value; }
        }

        /// <summary>
        /// The orientation that this character was originally in before going to the battlegrounds.
        /// </summary>
        public float EntryOrientation
        {
            get { return this._entryOrientation; }
            set { this._entryOrientation = value; }
        }

        /// <summary>
        /// Whether or not this character is considered a deserter. (Deserters cannot join battlegrounds)
        /// </summary>
        public bool IsDeserter
        {
            get { return this._isDeserter; }
            set
            {
                this._isDeserter = value;
                if (!this._isDeserter)
                    return;
                this.CancelAllRelations();
            }
        }

        /// <summary>
        /// Whether or not this character is enqueued for any battlegrounds.
        /// </summary>
        public bool IsEnqueuedForBattleground
        {
            get { return this._relationCount > 0; }
        }

        /// <summary>
        /// The battlegrounds relations for this character, if any.
        /// </summary>
        public BattlegroundRelation[] Relations
        {
            get { return this._relations; }
        }

        /// <summary>The number of current battlegrounds relations.</summary>
        public int RelationCount
        {
            get { return this._relationCount; }
        }

        /// <summary>
        /// Whether or not this character can queue for any more battlegrounds.
        /// </summary>
        public bool HasAvailableQueueSlots
        {
            get { return this._relationCount < BattlegroundMgr.MaxQueuesPerChar; }
        }

        /// <summary>
        /// Returns the character to their original location prior to entering the Battleground.
        /// </summary>
        public void TeleportBack()
        {
            if (this.m_EntryMap == null || this.m_EntryMap.IsDisposed || (double) this._entryPosition.X == 0.0)
                this._chr.TeleportToBindLocation();
            else
                this._chr.TeleportTo(this.m_EntryMap, ref this._entryPosition, new float?(this._entryOrientation));
            this.m_EntryMap = (Map) null;
        }

        /// <summary>Sets the entry position of the character.</summary>
        public void SetCharacterEntry(Map map, ref Vector3 pos, float orientation)
        {
            this.m_EntryMap = map;
            this._entryPosition = pos;
            this._entryOrientation = orientation;
        }

        /// <summary>
        /// Gets the <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundRelation" /> for the given Battleground for
        /// the Character.
        /// </summary>
        /// <param name="bgId"></param>
        /// <returns></returns>
        public BattlegroundRelation GetRelation(BattlegroundId bgId)
        {
            for (int index = 0; index < this._relations.Length; ++index)
            {
                BattlegroundRelation relation = this._relations[index];
                if (relation != null && relation.Queue.ParentQueue.Template.Id == bgId)
                    return relation;
            }

            return (BattlegroundRelation) null;
        }

        public bool IsEnqueuedFor(BattlegroundId bgId)
        {
            BattlegroundRelation relation = this.GetRelation(bgId);
            if (relation != null)
                return relation.IsEnqueued;
            return false;
        }

        public bool CancelIfEnqueued(BattlegroundId bgId)
        {
            this._chr.ContextHandler.EnsureContext();
            for (int index = 0; index < this._relations.Length; ++index)
            {
                BattlegroundRelation relation = this._relations[index];
                if (relation != null && relation.Queue.ParentQueue.Template.Id == bgId)
                {
                    if (!relation.IsEnqueued)
                        return false;
                    relation.Cancel();
                    return true;
                }
            }

            return false;
        }

        public void Cancel(BattlegroundInvitation invite)
        {
            this.RemoveRelation(invite.QueueIndex);
        }

        public int RemoveRelation(BattlegroundRelation relation)
        {
            return this.RemoveRelation(relation.BattlegroundId);
        }

        /// <summary>
        /// Removes the <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundRelation" /> for the given Battleground.
        /// This also cancels invitations and leaves the Battleground.
        /// If it was a Queue request for the Group and this is the GroupLeader, it also
        /// removes everyone else from the Queue.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The index of the removed relation or -1 if none removed</returns>
        public int RemoveRelation(BattlegroundId id)
        {
            this._chr.EnsureContext();
            for (int index = 0; index < this._relations.Length; ++index)
            {
                BattlegroundRelation relation = this._relations[index];
                if (relation != null && relation.Queue.ParentQueue.Template.Id == id)
                {
                    this.RemoveRelation(index, relation, true);
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes the corresponding relation and removes it from the queue
        /// if it is enqueued.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int CancelRelation(BattlegroundId id)
        {
            this._chr.EnsureContext();
            for (int index = 0; index < this._relations.Length; ++index)
            {
                BattlegroundRelation relation = this._relations[index];
                if (relation != null && relation.Queue.ParentQueue.Template.Id == id)
                {
                    this.CancelRelation(index, relation, true);
                    return index;
                }
            }

            return -1;
        }

        public int CancelRelation(int index, BattlegroundRelation relation, bool charActive)
        {
            this._chr.EnsureContext();
            BattlegroundTeamQueue queue = relation.Queue;
            if (queue != null)
                queue.Remove(relation);
            this.RemoveRelation(index, relation, charActive);
            return index;
        }

        /// <summary>Make sure the given index is valid</summary>
        /// <param name="index"></param>
        public void RemoveRelation(int index)
        {
            this.RemoveRelation(index, this._relations[index], true);
        }

        internal void RemoveRelation(int index, BattlegroundRelation relation, bool isCharActive)
        {
            this._chr.EnsureContext();
            this._relations[index] = (BattlegroundRelation) null;
            BattlegroundInvitation invitation = this.Invitation;
            if (invitation != null)
            {
                --invitation.Team.ReservedSlots;
                this.Invitation = (BattlegroundInvitation) null;
            }

            BattlegroundId bgId = relation.BattlegroundId;
            Battleground map = this._chr.Map as Battleground;
            if (map != null && map.Template.Id == bgId && (!relation.IsEnqueued && !this._chr.IsTeleporting) &&
                isCharActive)
                map.TeleportOutside(this._chr);
            if (isCharActive)
                BattlegroundHandler.ClearStatus((IPacketReceiver) this._chr, index);
            if (!relation.IsEnqueued || relation.Characters.CharacterCount <= 1)
                return;
            Group group = this._chr.Group;
            if (group == null || !group.IsLeader(this._chr))
                return;
            relation.Characters.ForeachCharacter((Action<Character>) (chr =>
            {
                if (chr == this._chr)
                    return;
                chr.ExecuteInContext((Action) (() => chr.Battlegrounds.RemoveRelation(bgId)));
            }));
        }

        /// <summary>
        /// Invites this Character to the given Battleground or enqueues him/her.
        /// </summary>
        internal void InviteTo(BattlegroundTeam team)
        {
            int queueIndex = this.GetIndex(team.Battleground.Template.Id);
            BattlegroundRelation relation;
            if (queueIndex == -1)
                queueIndex =
                    this.AddRelation(relation = new BattlegroundRelation(team.Queue, (ICharacterSet) this._chr, false));
            else
                relation = this._relations[queueIndex];
            this.InviteTo(team, queueIndex, relation);
        }

        internal void InviteTo(BattlegroundTeam team, int queueIndex, BattlegroundRelation relation)
        {
            this._chr.EnsureContext();
            relation.IsEnqueued = false;
            Battleground bg = team.Battleground;
            BattlegroundInvitation battlegroundInvitation = new BattlegroundInvitation(team, queueIndex);
            this._chr.Battlegrounds.Invitation = battlegroundInvitation;
            battlegroundInvitation.CancelTimer = this._chr.CallDelayed(BattlegroundMgr.InvitationTimeoutMillis,
                (Action<WorldObject>) (obj => this.RemoveRelation(bg.Template.Id)));
            BattlegroundHandler.SendStatusInvited(this._chr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relation"></param>
        /// <returns>The index of the newly created relation</returns>
        public int AddRelation(BattlegroundRelation relation)
        {
            this._chr.EnsureContext();
            int index = (int) ArrayUtil.AddOnlyOne<BattlegroundRelation>(ref this._relations, relation);
            ++this._relationCount;
            BattlegroundQueue parentQueue = relation.Queue.ParentQueue;
            if (parentQueue != null)
                BattlegroundHandler.SendStatusEnqueued(this._chr, index, relation, parentQueue);
            return index;
        }

        public int GetIndex(BattlegroundRelation request)
        {
            BattlegroundQueue parentQueue = request.Queue.ParentQueue;
            if (parentQueue != null)
                return this.GetIndex(parentQueue.Template.Id);
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>Requires Context</remarks>
        public int GetIndex(BattlegroundId id)
        {
            this._chr.ContextHandler.EnsureContext();
            for (int index = 0; index < this._relations.Length; ++index)
            {
                BattlegroundRelation relation = this._relations[index];
                if (relation != null && relation.Queue.ParentQueue.Template.Id == id)
                    return index;
            }

            return -1;
        }

        /// <summary>Cancel all relations</summary>
        public void CancelAllRelations()
        {
            for (int index = 0; index < this._relations.Length; ++index)
            {
                BattlegroundRelation relation = this._relations[index];
                if (relation != null)
                    this.CancelRelation(index, relation, false);
            }
        }

        internal void OnLogout()
        {
            this.CancelAllRelations();
        }

        public bool IsParticipating(BattlegroundId bgId)
        {
            if (this.Team != null)
                return this.Team.Battleground.Template.Id == bgId;
            return false;
        }
    }
}