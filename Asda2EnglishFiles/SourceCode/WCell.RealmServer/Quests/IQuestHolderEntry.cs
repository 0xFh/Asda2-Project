﻿using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Quests
{
    public interface IQuestHolderEntry
    {
        /// <summary>
        /// Ids of all IQuestHolderEntries are not globally unique.
        /// This might be an Id of GOEntry, NPCEntry, ItemTemplate (or other)
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// All Quest-information that this QuestGiver holds.
        /// Is null if this is not an actual QuestGiver.
        /// </summary>
        QuestHolderInfo QuestHolderInfo { get; }

        /// <summary>
        /// All "templates" of this entry that exist in the world (GOTemplate, SpawnEntry etc)
        /// </summary>
        IWorldLocation[] GetInWorldTemplates();
    }
}