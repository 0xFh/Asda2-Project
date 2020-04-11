namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a group invite relationship between two <see cref="T:WCell.RealmServer.Entities.Character" /> entities.
    /// </summary>
    public sealed class GroupInviteRelation : BaseRelation
    {
        public GroupInviteRelation(uint charId, uint relatedCharId)
            : base(charId, relatedCharId)
        {
        }

        public override bool RequiresOnlineNotification
        {
            get { return false; }
        }

        public override CharacterRelationType Type
        {
            get { return CharacterRelationType.GroupInvite; }
        }
    }
}