using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Calendars
{
    internal class CalendarEventInvite
    {
        private uint m_id;
        private CalendarEvent m_event;
        private Character m_inviter;

        public uint Id
        {
            get { return this.m_id; }
            set { this.m_id = value; }
        }

        public CalendarEvent EventId
        {
            get { return this.m_event; }
            set { this.m_event = value; }
        }

        public Character Inviter
        {
            get { return this.m_inviter; }
            set { this.m_inviter = value; }
        }
    }
}