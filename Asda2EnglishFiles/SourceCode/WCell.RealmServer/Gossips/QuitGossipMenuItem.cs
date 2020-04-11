using WCell.Constants;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
    public class QuitGossipMenuItem : LocalizedGossipMenuItem
    {
        public QuitGossipMenuItem(GossipMenuIcon type = GossipMenuIcon.Talk, RealmLangKey msg = RealmLangKey.Done)
            : base(type, msg, new object[0])
        {
        }

        public QuitGossipMenuItem(GossipMenuIcon type, RealmLangKey msg, params object[] args)
            : base(type, msg, args)
        {
        }

        public QuitGossipMenuItem(RealmLangKey msg)
            : base(msg, new object[0])
        {
            this.Action =
                (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
                    convo.Character.GossipConversation.StayOpen = false));
        }

        public QuitGossipMenuItem(RealmLangKey msg, params object[] args)
            : base(msg, args)
        {
            this.Action =
                (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
                    convo.Character.GossipConversation.StayOpen = false));
        }

        public QuitGossipMenuItem(GossipActionHandler callback, RealmLangKey msg = RealmLangKey.Done,
            params object[] args)
            : base(msg, args)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
            {
                convo.Character.GossipConversation.StayOpen = false;
                callback(convo);
            }));
        }

        public QuitGossipMenuItem(RealmLangKey text, GossipActionHandler callback, params GossipMenuItem[] items)
            : base(text, (object[]) items)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
            {
                convo.Character.GossipConversation.StayOpen = false;
                callback(convo);
            }));
        }

        public QuitGossipMenuItem(GossipMenu subMenu, RealmLangKey msg, params object[] args)
            : base(subMenu, msg, args)
        {
            this.Action =
                (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
                    convo.Character.GossipConversation.StayOpen = false));
        }

        public QuitGossipMenuItem(GossipActionHandler callback, GossipMenu subMenu, RealmLangKey msg,
            params object[] args)
            : base(subMenu, msg, args)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
            {
                convo.Character.GossipConversation.StayOpen = false;
                callback(convo);
            }));
        }

        public QuitGossipMenuItem(RealmLangKey text, params GossipMenuItem[] items)
            : base(text, (object[]) items)
        {
            this.Action =
                (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
                    convo.Character.GossipConversation.StayOpen = false));
        }

        public QuitGossipMenuItem(GossipMenuIcon icon, RealmLangKey text, params GossipMenuItem[] items)
            : base(icon, text, (object[]) items)
        {
            this.Action =
                (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
                    convo.Character.GossipConversation.StayOpen = false));
        }

        public QuitGossipMenuItem(GossipMenuIcon icon, GossipActionHandler callback, RealmLangKey msg,
            params object[] args)
            : base(icon, msg, args)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction((GossipActionHandler) (convo =>
            {
                convo.Character.GossipConversation.StayOpen = false;
                callback(convo);
            }));
        }
    }
}