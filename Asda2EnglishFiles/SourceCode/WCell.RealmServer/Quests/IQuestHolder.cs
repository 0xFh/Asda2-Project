using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Quests
{
    /// <summary>
    /// A QuestHolder are usually NPCs and GameObjects that trigger the start or end of a Quest
    /// </summary>
    public interface IQuestHolder : IEntity
    {
        bool CanGiveQuestTo(Character chr);

        /// <summary>
        /// Called whenever the QuestGiver status is sent to a Character
        /// </summary>
        void OnQuestGiverStatusQuery(Character chr);

        /// <summary>
        /// All Quest-information that this QuestGiver holds.
        /// Is null if this is not an actual QuestGiver.
        /// </summary>
        QuestHolderInfo QuestHolderInfo { get; }
    }
}