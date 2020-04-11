using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Groups
{
    public class SubGroup : IEnumerable<GroupMember>, IEnumerable, ICharacterSet, IPacketReceiver
    {
        public const int MaxMemberCount = 5;
        protected internal IList<GroupMember> m_members;
        private readonly Group m_group;
        private readonly byte m_Id;

        public SubGroup(Group group, byte groupUnitId)
        {
            this.m_group = group;
            this.m_Id = groupUnitId;
            this.m_members = (IList<GroupMember>) new List<GroupMember>(5);
        }

        public Group Group
        {
            get { return this.m_group; }
        }

        /// <summary>
        /// Whether this SubGroup has already the max amount of members
        /// </summary>
        public bool IsFull
        {
            get { return this.m_members.Count == 5; }
        }

        public byte Id
        {
            get { return this.m_Id; }
        }

        public int CharacterCount
        {
            get { return this.m_members.Count; }
        }

        public FactionGroup FactionGroup
        {
            get { return this.m_group.FactionGroup; }
        }

        public void ForeachCharacter(Action<Character> callback)
        {
            using (this.Group.SyncRoot.EnterReadLock())
            {
                foreach (GroupMember member in (IEnumerable<GroupMember>) this.m_members)
                {
                    Character character = member.Character;
                    if (character != null)
                        callback(character);
                }
            }
        }

        public void ForeachMember(Action<GroupMember> callback)
        {
            using (this.Group.SyncRoot.EnterReadLock())
            {
                foreach (GroupMember member in (IEnumerable<GroupMember>) this.m_members)
                    callback(member);
            }
        }

        public Character[] GetAllCharacters()
        {
            using (this.Group.SyncRoot.EnterReadLock())
            {
                Character[] array = new Character[this.Members.Length];
                int newSize = 0;
                foreach (GroupMember member in (IEnumerable<GroupMember>) this.m_members)
                {
                    Character character = member.Character;
                    if (character != null)
                        array[newSize++] = character;
                }

                if (newSize < this.Members.Length)
                    Array.Resize<Character>(ref array, newSize);
                return array;
            }
        }

        public GroupMember[] Members
        {
            get { return this.m_members.ToArray<GroupMember>(); }
        }

        public bool AddMember(GroupMember member)
        {
            if (this.IsFull)
                return false;
            this.m_members.Add(member);
            member.SubGroup = this;
            return true;
        }

        internal bool RemoveMember(GroupMember member)
        {
            if (!this.m_members.Remove(member))
                return false;
            member.SubGroup = (SubGroup) null;
            return true;
        }

        public GroupMember this[uint lowMemberId]
        {
            get
            {
                foreach (GroupMember member in (IEnumerable<GroupMember>) this.m_members)
                {
                    if ((int) member.Id == (int) lowMemberId)
                        return member;
                }

                return (GroupMember) null;
            }
        }

        public GroupMember this[string name]
        {
            get
            {
                string lower = name.ToLower();
                foreach (GroupMember member in (IEnumerable<GroupMember>) this.m_members)
                {
                    if (member.Name.ToLower() == lower)
                        return member;
                }

                return (GroupMember) null;
            }
        }

        public IEnumerator<GroupMember> GetEnumerator()
        {
            return this.m_members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.m_members.GetEnumerator();
        }

        /// <summary>
        /// Send a packet to every group member except for the one specified.
        /// </summary>
        /// <param name="packet">the packet to send</param>
        /// <param name="ignored">the member that won't receive the packet</param>
        public void Send(RealmPacketOut packet, GroupMember ignored)
        {
            Character charMember;
            this.ForeachMember((Action<GroupMember>) (member =>
            {
                if (member == ignored)
                    return;
                charMember = member.Character;
                if (charMember == null)
                    return;
                charMember.Client.Send(packet, false);
            }));
        }

        public bool IsRussianClient { get; set; }

        public Locale Locale { get; set; }

        public void Send(RealmPacketOut packet, bool addEnd = false)
        {
            this.Send(packet, (GroupMember) null);
        }
    }
}