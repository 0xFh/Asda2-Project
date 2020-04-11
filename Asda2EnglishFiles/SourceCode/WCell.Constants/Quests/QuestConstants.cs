namespace WCell.Constants.Quests
{
    public static class QuestConstants
    {
        /// <summary>Needed for certain Packets</summary>
        public static readonly uint[] MenuStatusLookup = new uint[11];

        /// <summary>Amounts of update fields, occupied by each quest</summary>
        public const int UpdateFieldCountPerQuest = 5;

        public const int MaxReputations = 5;
        public const int MaxRewardItems = 4;
        public const int MaxRewardChoiceItems = 6;
        public const int MaxObjectInteractions = 4;
        public const int MaxReceivedItems = 4;
        public const int MaxObjectiveTexts = 4;
        public const int MaxRequirements = 4;
        public const int MaxEmotes = 4;
        public const int MaxQuestsPerQuestGiver = 20;
        public const int MaxCollectableItems = 6;

        /// <summary>Used in certain Packets</summary>
        public const uint GOIndicator = 2147483648;

        static QuestConstants()
        {
            QuestConstants.MenuStatusLookup[10] = 4U;
            QuestConstants.MenuStatusLookup[6] = 4U;
            QuestConstants.MenuStatusLookup[2] = 6U;
            QuestConstants.MenuStatusLookup[5] = 1U;
        }
    }
}