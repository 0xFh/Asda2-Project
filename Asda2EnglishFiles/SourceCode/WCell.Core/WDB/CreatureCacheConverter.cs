using System.IO;
using WCell.Constants.NPCs;

namespace WCell.Core.WDB
{
    public class CreatureCacheConverter : WDBRecordConverter<CreatureCache>
    {
        public override CreatureCache Convert(BinaryReader binReader)
        {
            CreatureCache creatureCache = new CreatureCache();
            creatureCache.Id = binReader.ReadUInt32();
            binReader.ReadInt32();
            creatureCache.Names = new string[4];
            for (int index = 0; index < creatureCache.Names.Length; ++index)
                creatureCache.Names[index] = binReader.ReadCString();
            creatureCache.Description = binReader.ReadCString();
            creatureCache.UnkString = binReader.ReadCString();
            creatureCache.Flags = (NPCEntryFlags) binReader.ReadUInt32();
            creatureCache.Type = (CreatureType) binReader.ReadUInt32();
            creatureCache.Family = binReader.ReadUInt32();
            creatureCache.Rank = (CreatureRank) binReader.ReadUInt32();
            creatureCache.CreatureRelation1 = binReader.ReadUInt32();
            creatureCache.CreatureRelation2 = binReader.ReadUInt32();
            creatureCache.MaleDisplayId = binReader.ReadUInt32();
            creatureCache.FemaleDisplayId = binReader.ReadUInt32();
            creatureCache.DisplayId3 = binReader.ReadUInt32();
            creatureCache.DisplayId4 = binReader.ReadUInt32();
            creatureCache.HpModifier = binReader.ReadSingle();
            creatureCache.ManaModifier = binReader.ReadSingle();
            creatureCache.RacialLeader = binReader.ReadByte();
            creatureCache.QuestItem = new uint[6];
            for (int index = 0; index < creatureCache.QuestItem.Length; ++index)
                creatureCache.QuestItem[index] = binReader.ReadUInt32();
            creatureCache.MovementInfo = binReader.ReadUInt32();
            return creatureCache;
        }
    }
}