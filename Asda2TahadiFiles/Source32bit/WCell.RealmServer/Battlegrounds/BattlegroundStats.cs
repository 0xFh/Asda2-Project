using WCell.Core.Network;

namespace WCell.RealmServer.Battlegrounds
{
    public class BattlegroundStats
    {
        public int KillingBlows;
        public int HonorableKills;
        public int Deaths;
        public int BonusHonor;
        public int TotalDamage;
        public int TotalHealing;

        public virtual int SpecialStatCount
        {
            get { return 0; }
        }

        /// <summary>Append bg-specific stats to pvp-stats Packet</summary>
        public virtual void WriteSpecialStats(RealmPacketOut packet)
        {
        }
    }
}