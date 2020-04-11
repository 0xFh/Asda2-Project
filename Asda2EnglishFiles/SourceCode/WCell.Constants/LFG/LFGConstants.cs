using System;

namespace WCell.Constants.LFG
{
    public static class LFGConstants
    {
        public static TimeSpan RoleCheckTime = TimeSpan.FromMinutes(2.0);
        public static TimeSpan BootTime = TimeSpan.FromMinutes(2.0);
        public static TimeSpan ProposalTime = TimeSpan.FromMinutes(2.0);
        public static int TanksNeeded = 1;
        public static int HealersNeeded = 1;
        public static int DPSNeeded = 3;
        public static int MaxKicks = 3;
        public static int VotesNeededToKick = 3;
        public static TimeSpan QueueUpdateInterval = TimeSpan.FromMilliseconds(15.0);
    }
}