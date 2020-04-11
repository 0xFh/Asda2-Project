using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a relationship between two <see cref="T:WCell.RealmServer.Entities.Character" /> entities.
    /// </summary>
    public abstract class BaseRelation : IBaseRelation
    {
        public static readonly IBaseRelation[] EmptyRelations = new IBaseRelation[0];
        public static readonly HashSet<IBaseRelation> EmptyRelationSet = new HashSet<IBaseRelation>();

        /// <summary>The Character who created this Relation</summary>
        public virtual uint CharacterId { get; set; }

        /// <summary>The related Character with who this Relation is with</summary>
        public virtual uint RelatedCharacterId { get; set; }

        /// <summary>The relation type</summary>
        public abstract CharacterRelationType Type { get; }

        /// <summary>A note describing the relation</summary>
        public virtual string Note { get; set; }

        /// <summary>
        /// Indicates if the relation requires sending a notification when a player change
        /// its online status
        /// </summary>
        public virtual bool RequiresOnlineNotification
        {
            get { return false; }
        }

        /// <summary>Default constructor</summary>
        protected BaseRelation()
        {
        }

        /// <summary>
        /// Creates a new character relation based on the chars EntityId
        /// </summary>
        protected BaseRelation(uint charId, uint relatedCharId)
        {
            this.CharacterId = charId;
            this.RelatedCharacterId = relatedCharId;
        }

        public override bool Equals(object otherRelation)
        {
            if (!(otherRelation is BaseRelation))
                return false;
            BaseRelation baseRelation = otherRelation as BaseRelation;
            if ((int) this.CharacterId == (int) baseRelation.CharacterId &&
                (int) this.RelatedCharacterId == (int) baseRelation.RelatedCharacterId)
                return this.Type == baseRelation.Type;
            return false;
        }

        public override int GetHashCode()
        {
            return (int) this.RelatedCharacterId;
        }

        public virtual bool Validate(CharacterRecord charInfo, CharacterRecord relatedCharInfo,
            out RelationResult relResult)
        {
            relResult = RelationResult.FRIEND_DB_ERROR;
            return true;
        }
    }
}