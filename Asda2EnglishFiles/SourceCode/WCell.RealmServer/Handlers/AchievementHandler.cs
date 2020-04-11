using NLog;
using System;
using System.IO;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    /// <summary>Stub class for containing achievement packets</summary>
    public static class AchievementHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void SendAchievementData(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ALL_ACHIEVEMENT_DATA,
                chr.Achievements.AchievementsCount * 2 * 4 + 4))
            {
                if (chr.Achievements.AchievementsCount <= 0)
                    return;
                AchievementHandler.CreateAchievementData(packet, chr);
                chr.Client.Send(packet, false);
            }
        }

        public static void SendAchievementEarned(uint achievementEntryId, Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ACHIEVEMENT_EARNED, 16))
            {
                chr.EntityId.WritePacked((BinaryWriter) packet);
                packet.WriteUInt(achievementEntryId);
                packet.WriteDateTime(DateTime.Now);
                packet.WriteUInt(0);
                chr.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendServerFirstAchievement(uint achievementEntryId, Character chr)
        {
        }

        public static RealmPacketOut CreateAchievementEarnedToGuild(uint achievementEntryId, Character chr)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT);
            realmPacketOut.WriteByte((byte) 48);
            realmPacketOut.WriteUInt(0U);
            realmPacketOut.Write((ulong) chr.EntityId);
            realmPacketOut.WriteUInt(5);
            realmPacketOut.Write((ulong) chr.EntityId);
            realmPacketOut.WriteUIntPascalString("|Hplayer:$N|h[$N]|h has earned the achievement $a!");
            realmPacketOut.WriteByte(0);
            realmPacketOut.WriteUInt(achievementEntryId);
            return realmPacketOut;
        }

        public static void SendAchievmentStatus(AchievementProgressRecord achievementProgressRecord, Character chr)
        {
            using (RealmPacketOut realmPacketOut =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_CRITERIA_UPDATE, 36))
            {
                realmPacketOut.WriteUInt(achievementProgressRecord.AchievementCriteriaId);
                realmPacketOut.WritePackedUInt64((ulong) achievementProgressRecord.Counter);
                chr.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                realmPacketOut.Write(0);
                realmPacketOut.WriteDateTime(DateTime.Now);
                realmPacketOut.Write(0);
                realmPacketOut.Write(0);
                chr.Client.Send(realmPacketOut, false);
            }
        }

        public static void HandleInspectAchievements(IRealmClient client, RealmPacketIn packet)
        {
            Character character = World.GetCharacter(packet.ReadPackedEntityId().Low);
            if (character == null || !character.IsInContext)
                return;
            AchievementHandler.SendRespondInspectAchievements(character);
        }

        public static void SendRespondInspectAchievements(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(
                (PacketId) RealmServerOpCode.SMSG_RESPOND_INSPECT_ACHIEVEMENTS,
                chr.Achievements.AchievementsCount * 2 * 4 + 4 + 8))
            {
                chr.EntityId.WritePacked((BinaryWriter) packet);
                AchievementHandler.CreateAchievementData(packet, chr);
                chr.Client.Send(packet, false);
            }
        }

        public static void CreateAchievementData(RealmPacketOut packet, Character chr)
        {
            foreach (AchievementRecord achievementRecord in chr.Achievements.m_completedAchievements.Values)
            {
                packet.WriteUInt(achievementRecord.AchievementEntryId);
                packet.WriteDateTime(achievementRecord.CompleteDate);
            }

            packet.WriteInt(uint.MaxValue);
        }
    }
}