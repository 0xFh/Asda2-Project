using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Trade
{
    public class TradeMgr
    {
        public static float MaxTradeRadius = 10f;
        public const int MaxSlotCount = 7;
        public const int NontradeSlot = 6;
        public const int TradeSlotCount = 6;

        /// <summary>
        /// Makes initChr propose trading to targetChr
        /// Call CheckRequirements first
        /// </summary>
        /// <param name="initChr">initiator of trading</param>
        /// <param name="targetChr">traget of trading</param>
        public static void Propose(Character initChr, Character targetChr)
        {
            initChr.TradeWindow = new TradeWindow(initChr);
            targetChr.TradeWindow = new TradeWindow(targetChr);
            initChr.TradeWindow.m_otherWindow = targetChr.TradeWindow;
            targetChr.TradeWindow.m_otherWindow = initChr.TradeWindow;
            TradeHandler.SendTradeProposal((IPacketReceiver) targetChr.Client, initChr);
        }

        /// <summary>
        /// Checks requirements for trading between two characters
        /// </summary>
        /// <param name="initChr">possible initiator of trading</param>
        /// <param name="targetChr">possible target of trading</param>
        public static bool MayProposeTrade(Character initChr, Character targetChr)
        {
            TradeStatus tradeStatus1;
            if (targetChr == null || !targetChr.IsInContext)
            {
                tradeStatus1 = TradeStatus.PlayerNotFound;
                return false;
            }

            TradeStatus tradeStatus2;
            if (initChr.IsLoggingOut || targetChr.IsLoggingOut)
                tradeStatus2 = TradeStatus.LoggingOut;
            else if (!initChr.IsAlive)
                tradeStatus2 = TradeStatus.PlayerDead;
            else if (!targetChr.IsInRadius((WorldObject) initChr, TradeMgr.MaxTradeRadius))
                tradeStatus2 = TradeStatus.TooFarAway;
            else if (!targetChr.IsAlive)
                tradeStatus2 = TradeStatus.TargetDead;
            else if (targetChr.IsStunned)
                tradeStatus2 = TradeStatus.TargetStunned;
            else if (targetChr.IsIgnoring((IUser) initChr))
                tradeStatus2 = TradeStatus.PlayerIgnored;
            else if (targetChr.TradeWindow != null)
                tradeStatus2 = TradeStatus.AlreadyTrading;
            else if (targetChr.Faction.Group != initChr.Faction.Group && !initChr.Role.IsStaff)
                tradeStatus2 = TradeStatus.WrongFaction;
            else if (targetChr.IsLoggingOut)
            {
                tradeStatus2 = TradeStatus.TargetLoggingOut;
            }
            else
            {
                tradeStatus1 = TradeStatus.Proposed;
                return true;
            }

            TradeHandler.SendTradeStatus((IPacketReceiver) initChr, tradeStatus2);
            return false;
        }
    }
}