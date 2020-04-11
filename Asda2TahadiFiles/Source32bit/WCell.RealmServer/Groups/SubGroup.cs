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
      m_group = group;
      m_Id = groupUnitId;
      m_members = new List<GroupMember>(5);
    }

    public Group Group
    {
      get { return m_group; }
    }

    /// <summary>
    /// Whether this SubGroup has already the max amount of members
    /// </summary>
    public bool IsFull
    {
      get { return m_members.Count == 5; }
    }

    public byte Id
    {
      get { return m_Id; }
    }

    public int CharacterCount
    {
      get { return m_members.Count; }
    }

    public FactionGroup FactionGroup
    {
      get { return m_group.FactionGroup; }
    }

    public void ForeachCharacter(Action<Character> callback)
    {
      using(Group.SyncRoot.EnterReadLock())
      {
        foreach(GroupMember member in m_members)
        {
          Character character = member.Character;
          if(character != null)
            callback(character);
        }
      }
    }

    public void ForeachMember(Action<GroupMember> callback)
    {
      using(Group.SyncRoot.EnterReadLock())
      {
        foreach(GroupMember member in m_members)
          callback(member);
      }
    }

    public Character[] GetAllCharacters()
    {
      using(Group.SyncRoot.EnterReadLock())
      {
        Character[] array = new Character[Members.Length];
        int newSize = 0;
        foreach(GroupMember member in m_members)
        {
          Character character = member.Character;
          if(character != null)
            array[newSize++] = character;
        }

        if(newSize < Members.Length)
          Array.Resize(ref array, newSize);
        return array;
      }
    }

    public GroupMember[] Members
    {
      get { return m_members.ToArray(); }
    }

    public bool AddMember(GroupMember member)
    {
      if(IsFull)
        return false;
      m_members.Add(member);
      member.SubGroup = this;
      return true;
    }

    internal bool RemoveMember(GroupMember member)
    {
      if(!m_members.Remove(member))
        return false;
      member.SubGroup = null;
      return true;
    }

    public GroupMember this[uint lowMemberId]
    {
      get
      {
        foreach(GroupMember member in m_members)
        {
          if((int) member.Id == (int) lowMemberId)
            return member;
        }

        return null;
      }
    }

    public GroupMember this[string name]
    {
      get
      {
        string lower = name.ToLower();
        foreach(GroupMember member in m_members)
        {
          if(member.Name.ToLower() == lower)
            return member;
        }

        return null;
      }
    }

    public IEnumerator<GroupMember> GetEnumerator()
    {
      return m_members.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_members.GetEnumerator();
    }

    /// <summary>
    /// Send a packet to every group member except for the one specified.
    /// </summary>
    /// <param name="packet">the packet to send</param>
    /// <param name="ignored">the member that won't receive the packet</param>
    public void Send(RealmPacketOut packet, GroupMember ignored)
    {
      Character charMember;
      ForeachMember(member =>
      {
        if(member == ignored)
          return;
        charMember = member.Character;
        if(charMember == null)
          return;
        charMember.Client.Send(packet, false);
      });
    }

    public bool IsRussianClient { get; set; }

    public Locale Locale { get; set; }

    public void Send(RealmPacketOut packet, bool addEnd = false)
    {
      Send(packet, null);
    }
  }
}