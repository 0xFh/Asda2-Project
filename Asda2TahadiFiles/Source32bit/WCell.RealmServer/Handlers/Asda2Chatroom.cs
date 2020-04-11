using System.Collections.Generic;
using System.Linq;
using WCell.Core.Network;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Chat
{
  public class Asda2Chatroom
  {
    public Dictionary<uint, Character> Members = new Dictionary<uint, Character>();

    public Character Owner { get; set; }

    public string Password { get; set; }

    public byte MaxMembersCount { get; set; }

    public bool IsPrivate { get; set; }

    public string Name { get; set; }

    public Asda2Chatroom(Character activeCharacter, bool isPrivate, byte maxMemberCount, string roomName,
      string password)
    {
      Owner = activeCharacter;
      Members.Add(activeCharacter.AccId, activeCharacter);
      IsPrivate = isPrivate;
      MaxMembersCount = maxMemberCount;
      Name = roomName;
      Password = password;
    }

    public void TryJoin(Character joiner, string password)
    {
      if(MaxMembersCount <= Members.Count)
        ChatMgr.SendEnterChatRoomResultResponse(joiner.Client, EnterChatRoomStatus.RoomisFull,
          null);
      else if(IsPrivate && password != Password)
      {
        ChatMgr.SendEnterChatRoomResultResponse(joiner.Client, EnterChatRoomStatus.WrongPassword,
          null);
      }
      else
      {
        lock(this)
          Members.Add(joiner.AccId, joiner);
        joiner.ChatRoom = this;
        ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.Joined, joiner);
        ChatMgr.SendEnterChatRoomResultResponse(joiner.Client, EnterChatRoomStatus.Ok, this);
      }
    }

    public void Leave(Character leaver)
    {
      lock(this)
        Members.Remove(leaver.AccId);
      leaver.ChatRoom = null;
      ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.Left, leaver);
      if(Owner == leaver && Members.Count > 0)
      {
        Owner = Members.Values.First();
        ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.LeaderChanged, Owner);
        ChatMgr.SendChatRoomVisibleResponse(Owner, ChatRoomVisibilityStatus.Visible, this,
          null);
      }

      ChatMgr.SendChatRoomVisibleResponse(leaver, ChatRoomVisibilityStatus.Closed, null,
        null);
    }

    public void Dissmiss(Character dissmiser, uint targetAccId)
    {
      lock(this)
      {
        if(dissmiser != Owner || (int) dissmiser.EntityId.Low == (int) targetAccId)
        {
          dissmiser.SendInfoMsg("You are not chat room owner.");
          ChatMgr.SendDissmisedFromCharRoomResultResponse(dissmiser.Client,
            DissmissCharacterFromChatRoomResult.Fail);
        }

        if(!Members.ContainsKey(targetAccId))
        {
          dissmiser.SendInfoMsg("Target not founded.");
          ChatMgr.SendDissmisedFromCharRoomResultResponse(dissmiser.Client,
            DissmissCharacterFromChatRoomResult.Fail);
        }

        Character member = Members[targetAccId];
        ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.Banned, member);
        member.ChatRoom = null;
        Members.Remove(targetAccId);
        ChatMgr.SendChatRoomClosedResponse(member.Client, ChatRoomClosedStatus.Banned);
        ChatMgr.SendDissmisedFromCharRoomResultResponse(dissmiser.Client,
          DissmissCharacterFromChatRoomResult.Ok);
      }
    }

    public void Send(RealmPacketOut packet, bool addEnd, Locale locale)
    {
      lock(this)
      {
        foreach(Character character in Members.Values)
        {
          if(locale == Locale.Any || character.Client.Locale == locale)
            character.Send(packet, addEnd);
        }
      }
    }
  }
}