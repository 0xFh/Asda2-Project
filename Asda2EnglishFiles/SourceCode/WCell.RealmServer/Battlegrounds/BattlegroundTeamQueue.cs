using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>Each side of a Battelground has its own Queue</summary>
    public abstract class BattlegroundTeamQueue
    {
        public readonly LinkedList<BattlegroundRelation> PendingRequests = new LinkedList<BattlegroundRelation>();
        protected readonly BattlegroundQueue _parentQueue;
        protected readonly BattlegroundSide m_Side;
        protected int m_chrCount;

        protected BattlegroundTeamQueue(BattlegroundQueue parentQueue, BattlegroundSide side)
        {
            this._parentQueue = parentQueue;
            this.m_Side = side;
        }

        /// <summary>
        /// Warning: May be null if this belongs to an <see cref="T:WCell.RealmServer.Battlegrounds.InstanceBattlegroundQueue" /> after the BG has been disposed
        /// </summary>
        public BattlegroundQueue ParentQueue
        {
            get { return this._parentQueue; }
        }

        public int CharacterCount
        {
            get { return this.m_chrCount; }
        }

        public int RelationCount
        {
            get { return this.PendingRequests.Count; }
        }

        public BattlegroundSide Side
        {
            get { return this.m_Side; }
        }

        public ICharacterSet GetCharacterSet(Character chr, bool asGroup)
        {
            if (asGroup)
            {
                Group group = chr.Group;
                if (group == null || !group.IsLeader(chr))
                {
                    GroupHandler.SendResult((IPacketReceiver) chr, GroupResult.DontHavePermission);
                }
                else
                {
                    Character[] allCharacters = group.GetAllCharacters();
                    if (allCharacters.Length == 0)
                        return (ICharacterSet) chr;
                    if (allCharacters.Length > this._parentQueue.Template.MaxPlayersPerTeam)
                    {
                        BattlegroundHandler.SendBattlegroundError((IPacketReceiver) chr,
                            BattlegroundJoinError.GroupJoinedNotEligible);
                        return (ICharacterSet) null;
                    }

                    foreach (Character character in allCharacters)
                    {
                        if (character != null && character.Battlegrounds.IsDeserter)
                        {
                            BattlegroundHandler.SendBattlegroundError((IPacketReceiver) group,
                                BattlegroundJoinError.Deserter);
                            return (ICharacterSet) null;
                        }
                    }

                    SynchronizedCharacterList synchronizedCharacterList =
                        new SynchronizedCharacterList(this.Side.GetFactionGroup());
                    foreach (Character chr1 in allCharacters)
                    {
                        if (chr1.IsInWorld && (chr1.GodMode || this._parentQueue.CanEnter(chr1)))
                            synchronizedCharacterList.Add(chr1);
                        else
                            BattlegroundHandler.SendBattlegroundError((IPacketReceiver) chr1,
                                BattlegroundJoinError.GroupJoinedNotEligible);
                    }

                    return (ICharacterSet) synchronizedCharacterList;
                }
            }
            else
            {
                if (!chr.Battlegrounds.IsDeserter)
                    return (ICharacterSet) chr;
                BattlegroundHandler.SendBattlegroundError((IPacketReceiver) chr, BattlegroundJoinError.Deserter);
            }

            return (ICharacterSet) null;
        }

        internal void Enqueue(BattlegroundRelation request)
        {
            if (this._parentQueue.RequiresLocking)
            {
                lock (this.PendingRequests)
                {
                    this.PendingRequests.AddLast(request);
                    this.m_chrCount += request.Characters.CharacterCount;
                }
            }
            else
            {
                this.PendingRequests.AddLast(request);
                this.m_chrCount += request.Characters.CharacterCount;
            }
        }

        public BattlegroundRelation Enqueue(Character chr, bool asGroup)
        {
            ICharacterSet characterSet = this.GetCharacterSet(chr, asGroup);
            if (characterSet != null)
                return this.Enqueue(characterSet);
            return (BattlegroundRelation) null;
        }

        public virtual BattlegroundRelation Enqueue(ICharacterSet chrs)
        {
            BattlegroundRelation request = new BattlegroundRelation(this, chrs);
            chrs.ForeachCharacter((Action<Character>) (chr =>
                chr.ExecuteInContext((Action) (() => chr.Battlegrounds.AddRelation(request)))));
            this.Enqueue(request);
            return request;
        }

        public BattlegroundQueue ParentQueueBase
        {
            get { return this._parentQueue; }
        }

        internal void Remove(BattlegroundRelation relation)
        {
            if (this._parentQueue.RequiresLocking)
            {
                lock (this.PendingRequests)
                    this.RemoveUnlocked(relation);
            }
            else
                this.RemoveUnlocked(relation);
        }

        private void RemoveUnlocked(BattlegroundRelation relation)
        {
            LinkedListNode<BattlegroundRelation> node = this.PendingRequests.First;
            while (node != null)
            {
                if (node.Value == relation)
                {
                    this.m_chrCount -= relation.Characters.CharacterCount;
                    LinkedListNode<BattlegroundRelation> next = node.Next;
                    relation.IsEnqueued = false;
                    this.PendingRequests.Remove(node);
                    node = next;
                    if (node == null)
                        break;
                }
                else
                    node = node.Next;
            }
        }

        /// <summary>
        /// Removes the given amount of Characters from this Queue and adds them
        /// to the given <see cref="T:WCell.RealmServer.Battlegrounds.Battleground" />
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="bg"></param>
        /// <returns>The amount of dequeued Characters</returns>
        internal int DequeueCharacters(int amount, Battleground bg)
        {
            bg.EnsureContext();
            LinkedListNode<BattlegroundRelation> first = this.PendingRequests.First;
            if (first == null)
                return 0;
            if (!this._parentQueue.RequiresLocking)
                return this.Dequeue(amount, bg, first);
            lock (this.PendingRequests)
                return this.Dequeue(amount, bg, first);
        }

        private int Dequeue(int amount, Battleground bg, LinkedListNode<BattlegroundRelation> node)
        {
            BattlegroundTeam team = bg.GetTeam(this.Side);
            int num = 0;
            do
            {
                BattlegroundRelation battlegroundRelation = node.Value;
                if (battlegroundRelation.Count <= amount)
                {
                    this.m_chrCount -= battlegroundRelation.Characters.CharacterCount;
                    num += team.Invite(battlegroundRelation.Characters);
                    battlegroundRelation.IsEnqueued = false;
                    LinkedListNode<BattlegroundRelation> next = node.Next;
                    this.PendingRequests.Remove(node);
                    node = next;
                    if (node == null)
                        break;
                }
            } while ((node = node.Next) != null && num <= amount);

            return num;
        }
    }
}