using WCell.Constants.Misc;

namespace WCell.RealmServer.Gossips
{
    public abstract class DynamicTextGossipMenu : GossipMenu
    {
        private static readonly DynamicGossipEntry SharedEntry = new DynamicGossipEntry(88993333U,
            new DynamicGossipText[1]
            {
                new DynamicGossipText(new GossipStringFactory(DynamicTextGossipMenu.OnTextQuery), 1f,
                    ChatLanguage.Universal)
            });

        private static string OnTextQuery(GossipConversation convo)
        {
            return ((DynamicTextGossipMenu) convo.CurrentMenu).GetText(convo);
        }

        protected DynamicTextGossipMenu()
            : base((IGossipEntry) DynamicTextGossipMenu.SharedEntry)
        {
        }

        protected DynamicTextGossipMenu(params GossipMenuItemBase[] items)
            : base((IGossipEntry) DynamicTextGossipMenu.SharedEntry, items)
        {
        }

        public abstract string GetText(GossipConversation convo);
    }
}