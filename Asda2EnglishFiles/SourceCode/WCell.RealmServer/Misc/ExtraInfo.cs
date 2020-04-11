using System;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Help.Tickets;
using WCell.Util.Commands;

namespace WCell.RealmServer.Misc
{
    /// <summary>
    /// Addition to every Character that is a StaffMember to append staff-only related information without wasting extra memory
    /// on non-staff members.
    /// TODO: Get rid off this
    /// </summary>
    public class ExtraInfo : IDisposable
    {
        private Character m_owner;
        internal GOSelection m_goSelection;
        internal BaseCommand<RealmServerCmdArgs> m_selectedCommand;
        internal Ticket m_handlingTicket;

        public ExtraInfo(Character chr)
        {
            this.m_owner = chr;
        }

        /// <summary>
        /// The currently selected GO of this Character. Set to null to deselect.
        /// </summary>
        public GameObject SelectedGO
        {
            get
            {
                if (this.m_goSelection != null)
                    return this.m_goSelection.GO;
                return (GameObject) null;
            }
            set { GOSelectMgr.Instance[this.m_owner] = value; }
        }

        public BaseCommand<RealmServerCmdArgs> SelectedCommand
        {
            get { return this.m_selectedCommand; }
        }

        /// <summary>
        /// The ticket that this Character is currently working on (or null)
        /// </summary>
        public Ticket HandlingTicket
        {
            get { return this.m_handlingTicket; }
            set { this.m_handlingTicket = value; }
        }

        public Ticket EnsureSelectedHandlingTicket()
        {
            if (this.m_handlingTicket == null)
                this.m_handlingTicket = TicketMgr.Instance.GetNextUnhandledTicket((ITicketHandler) this.m_owner);
            return this.m_handlingTicket;
        }

        public void Dispose()
        {
            GOSelectMgr.Instance.Deselect(this);
            this.m_owner = (Character) null;
        }
    }
}