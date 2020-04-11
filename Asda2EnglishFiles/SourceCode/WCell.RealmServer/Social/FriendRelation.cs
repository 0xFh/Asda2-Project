using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a friend relationship between two <see cref="T:WCell.RealmServer.Entities.Character" /> entities.
    /// </summary>
    public sealed class FriendRelation : PersistedRelation
    {
        public FriendRelation(uint charId, uint relatedCharId)
            : base(charId, relatedCharId)
        {
        }

        public override bool RequiresOnlineNotification
        {
            get { return true; }
        }

        public override CharacterRelationType Type
        {
            get { return CharacterRelationType.Friend; }
        }

        public override bool Validate(CharacterRecord charInfo, CharacterRecord relatedCharInfo,
            out RelationResult relResult)
        {
            if (charInfo == null)
            {
                relResult = RelationResult.FRIEND_DB_ERROR;
                return false;
            }

            if (relatedCharInfo == null)
            {
                relResult = RelationResult.FRIEND_NOT_FOUND;
                return false;
            }

            if ((int) charInfo.EntityLowId == (int) relatedCharInfo.EntityLowId)
            {
                relResult = RelationResult.FRIEND_SELF;
                return false;
            }

            if (FactionMgr.GetFactionGroup(charInfo.Race) != FactionMgr.GetFactionGroup(relatedCharInfo.Race))
            {
                relResult = RelationResult.FRIEND_ENEMY;
                return false;
            }

            if (Singleton<RelationMgr>.Instance.HasRelation(charInfo.EntityLowId, relatedCharInfo.EntityLowId,
                this.Type))
            {
                relResult = RelationResult.FRIEND_ALREADY;
                return false;
            }

            relResult = World.GetCharacter(relatedCharInfo.EntityLowId) == null
                ? RelationResult.FRIEND_ADDED_OFFLINE
                : RelationResult.FRIEND_ADDED_ONLINE;
            return true;
        }
    }
}