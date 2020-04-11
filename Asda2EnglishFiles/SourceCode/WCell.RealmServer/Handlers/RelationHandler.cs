using WCell.Constants.Relations;
using WCell.Core;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class RelationHandler
    {
        /// <summary>Handles an incoming friend list request</summary>
        /// <param name="client">the client that sent the packet</param>
        /// <param name="packet">the packet we received</param>
        public static void ContactListRequest(IRealmClient client, RealmPacketIn packet)
        {
            RelationTypeFlag flag = (RelationTypeFlag) packet.ReadUInt32();
            Singleton<RelationMgr>.Instance.SendRelationList(client.ActiveCharacter, flag);
        }

        /// <summary>Handles an incoming add friend request</summary>
        /// <param name="client">the client that sent the packet</param>
        /// <param name="packet">the packet we received</param>
        public static void AddFriendRequest(IRealmClient client, RealmPacketIn packet)
        {
            string relatedCharName = packet.ReadCString();
            string note = packet.ReadCString();
            Singleton<RelationMgr>.Instance.AddRelation(client.ActiveCharacter, relatedCharName, note,
                CharacterRelationType.Friend);
        }

        /// <summary>Handles an incoming remove friend request</summary>
        /// <param name="client">the client that sent the packet</param>
        /// <param name="packet">the packet we received</param>
        public static void RemoveFriendRequest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId entityId = packet.ReadEntityId();
            Singleton<RelationMgr>.Instance.RemoveRelation(client.ActiveCharacter.EntityId.Low, entityId.Low,
                CharacterRelationType.Friend);
        }

        /// <summary>Handles an incoming friend set note request</summary>
        /// <param name="client">the client that sent the packet</param>
        /// <param name="packet">the packet we received</param>
        public static void SetRelationNoteRequest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId entityId = packet.ReadEntityId();
            string note = packet.ReadCString();
            Singleton<RelationMgr>.Instance.SetRelationNote(client.ActiveCharacter.EntityId.Low, entityId.Low, note,
                CharacterRelationType.Friend);
        }

        /// <summary>Handles an incoming add ignore request</summary>
        /// <param name="client">the client that sent the packet</param>
        /// <param name="packet">the packet we received</param>
        public static void AddIgnoreRequest(IRealmClient client, RealmPacketIn packet)
        {
            string relatedCharName = packet.ReadCString();
            Singleton<RelationMgr>.Instance.AddRelation(client.ActiveCharacter, relatedCharName, string.Empty,
                CharacterRelationType.Ignored);
        }

        /// <summary>Handles an incoming remove ignore request</summary>
        /// <param name="client">the client that sent the packet</param>
        /// <param name="packet">the packet we received</param>
        public static void RemoveIgnoreRequest(IRealmClient client, RealmPacketIn packet)
        {
            EntityId entityId = packet.ReadEntityId();
            Singleton<RelationMgr>.Instance.RemoveRelation(client.ActiveCharacter.EntityId.Low, entityId.Low,
                CharacterRelationType.Ignored);
        }

        public static void BugRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadUInt32();
            int num2 = (int) packet.ReadUInt32();
            packet.ReadString();
            int num3 = (int) packet.ReadUInt32();
            packet.ReadString();
        }
    }
}