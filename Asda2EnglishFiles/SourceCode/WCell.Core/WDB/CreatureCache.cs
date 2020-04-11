using WCell.Constants.NPCs;

namespace WCell.Core.WDB
{
    public class CreatureCache
    {
        public uint Id;
        public string[] Names;
        public string Description;
        public string UnkString;
        public NPCEntryFlags Flags;

        /// <summary>CreatureType.dbc</summary>
        public CreatureType Type;

        /// <summary>CreatureFamily.dbc</summary>
        public uint Family;

        public CreatureRank Rank;

        /// <summary>
        /// Link creature to another creature that is required for a quest.
        /// </summary>
        public uint CreatureRelation1;

        public uint CreatureRelation2;

        /// <summary>CreatureDisplayInfo.dbc</summary>
        public uint MaleDisplayId;

        /// <summary>CreatureDisplayInfo.dbc</summary>
        public uint FemaleDisplayId;

        /// <summary>CreatureDisplayInfo.dbc</summary>
        public uint DisplayId3;

        /// <summary>CreatureDisplayInfo.dbc</summary>
        public uint DisplayId4;

        public float HpModifier;
        public float ManaModifier;
        public byte RacialLeader;
        public uint[] QuestItem;

        /// <summary>CreatureMovementInfo.dbc</summary>
        public uint MovementInfo;
    }
}