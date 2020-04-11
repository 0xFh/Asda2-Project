using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a friend relationship between two <see cref="T:WCell.RealmServer.Entities.Character" /> entities.
    /// </summary>
    public sealed class IgnoredRelation : PersistedRelation
    {
        public IgnoredRelation(uint charId, uint relatedCharId)
            : base(charId, relatedCharId)
        {
        }

        public override bool RequiresOnlineNotification
        {
            get { return false; }
        }

        public override CharacterRelationType Type
        {
            get { return CharacterRelationType.Ignored; }
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
                relResult = RelationResult.IGNORE_NOT_FOUND;
                return false;
            }

            if ((int) charInfo.EntityLowId == (int) relatedCharInfo.EntityLowId)
            {
                relResult = RelationResult.IGNORE_SELF;
                return false;
            }

            if (Singleton<RelationMgr>.Instance.HasRelation(charInfo.EntityLowId, relatedCharInfo.EntityLowId,
                this.Type))
            {
                relResult = RelationResult.IGNORE_ALREADY;
                return false;
            }

            relResult = RelationResult.IGNORE_ADDED;
            return true;
        }
    }
}