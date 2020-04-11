using System;
using WCell.Util.Data;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOCustomEntry : GOEntry
    {
        private GOEntry.GOUseHandler m_UseHandler;

        [NotPersistent]
        public GOEntry.GOUseHandler UseHandler
        {
            get { return this.m_UseHandler; }
            set
            {
                this.m_UseHandler = value;
                this.HandlerCreator = (Func<GameObjectHandler>) (() => (GameObjectHandler) new CustomUseHandler(value));
            }
        }

        protected internal override void InitEntry()
        {
            if (this.Fields != null)
                return;
            this.Fields = new int[24];
        }
    }
}