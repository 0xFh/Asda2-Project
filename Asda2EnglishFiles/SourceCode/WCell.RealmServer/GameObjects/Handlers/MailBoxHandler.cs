using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 19</summary>
    public class MailboxHandler : GameObjectHandler
    {
        private static Logger sLog = LogManager.GetCurrentClassLogger();

        public override bool Use(Character user)
        {
            if (user == null)
                return false;
            user.MailAccount.MailBox = this.m_go;
            return true;
        }
    }
}