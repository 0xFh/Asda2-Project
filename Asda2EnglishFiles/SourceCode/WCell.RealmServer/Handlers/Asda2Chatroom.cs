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
            this.Owner = activeCharacter;
            this.Members.Add(activeCharacter.AccId, activeCharacter);
            this.IsPrivate = isPrivate;
            this.MaxMembersCount = maxMemberCount;
            this.Name = roomName;
            this.Password = password;
        }

        public void TryJoin(Character joiner, string password)
        {
            if ((int) this.MaxMembersCount <= this.Members.Count)
                ChatMgr.SendEnterChatRoomResultResponse(joiner.Client, EnterChatRoomStatus.RoomisFull,
                    (Asda2Chatroom) null);
            else if (this.IsPrivate && password != this.Password)
            {
                ChatMgr.SendEnterChatRoomResultResponse(joiner.Client, EnterChatRoomStatus.WrongPassword,
                    (Asda2Chatroom) null);
            }
            else
            {
                lock (this)
                    this.Members.Add(joiner.AccId, joiner);
                joiner.ChatRoom = this;
                ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.Joined, joiner);
                ChatMgr.SendEnterChatRoomResultResponse(joiner.Client, EnterChatRoomStatus.Ok, this);
            }
        }

        public void Leave(Character leaver)
        {
            lock (this)
                this.Members.Remove(leaver.AccId);
            leaver.ChatRoom = (Asda2Chatroom) null;
            ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.Left, leaver);
            if (this.Owner == leaver && this.Members.Count > 0)
            {
                this.Owner = this.Members.Values.First<Character>();
                ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.LeaderChanged, this.Owner);
                ChatMgr.SendChatRoomVisibleResponse(this.Owner, ChatRoomVisibilityStatus.Visible, this,
                    (Character) null);
            }

            ChatMgr.SendChatRoomVisibleResponse(leaver, ChatRoomVisibilityStatus.Closed, (Asda2Chatroom) null,
                (Character) null);
        }

        public void Dissmiss(Character dissmiser, uint targetAccId)
        {
            lock (this)
            {
                if (dissmiser != this.Owner || (int) dissmiser.EntityId.Low == (int) targetAccId)
                {
                    dissmiser.SendInfoMsg("You are not chat room owner.");
                    ChatMgr.SendDissmisedFromCharRoomResultResponse(dissmiser.Client,
                        DissmissCharacterFromChatRoomResult.Fail);
                }

                if (!this.Members.ContainsKey(targetAccId))
                {
                    dissmiser.SendInfoMsg("Target not founded.");
                    ChatMgr.SendDissmisedFromCharRoomResultResponse(dissmiser.Client,
                        DissmissCharacterFromChatRoomResult.Fail);
                }

                Character member = this.Members[targetAccId];
                ChatMgr.SendChatRoomEventResponse(this, ChatRoomEventType.Banned, member);
                member.ChatRoom = (Asda2Chatroom) null;
                this.Members.Remove(targetAccId);
                ChatMgr.SendChatRoomClosedResponse(member.Client, ChatRoomClosedStatus.Banned);
                ChatMgr.SendDissmisedFromCharRoomResultResponse(dissmiser.Client,
                    DissmissCharacterFromChatRoomResult.Ok);
            }
        }

        public void Send(RealmPacketOut packet, bool addEnd, Locale locale)
        {
            lock (this)
            {
                foreach (Character character in this.Members.Values)
                {
                    if (locale == Locale.Any || character.Client.Locale == locale)
                        character.Send(packet, addEnd);
                }
            }
        }
    }
}