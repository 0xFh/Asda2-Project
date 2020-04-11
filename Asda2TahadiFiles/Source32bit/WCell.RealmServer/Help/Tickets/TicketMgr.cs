using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WCell.RealmServer.Help.Tickets
{
  /// <summary>
  /// Manages support, help-requests, GM Tickets, surveys etc.
  /// 
  /// TODO: Commands - Summon, Goto, Next, Previous
  /// TODO: Staff chat-channeling for enforced help
  /// TODO: Individual settings for staff/ticket issuing? Use ticket types?
  /// TODO: Backup command to store certain tickets?
  /// </summary>
  public class TicketMgr
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    public static readonly TicketMgr Instance = new TicketMgr();
    internal readonly Dictionary<uint, Ticket> ticketsById = new Dictionary<uint, Ticket>();
    internal readonly ReaderWriterLockSlim lck = new ReaderWriterLockSlim();
    internal Ticket first;
    internal Ticket last;

    public Ticket First
    {
      get { return first; }
    }

    public Ticket Last
    {
      get { return last; }
    }

    public int TicketCount
    {
      get { return ticketsById.Count; }
    }

    public void AddTicket(Ticket ticket)
    {
      lck.EnterWriteLock();
      try
      {
        if(first == null)
        {
          first = last = ticket;
        }
        else
        {
          last.m_next = ticket;
          ticket.m_previous = last;
          last = ticket;
        }

        ticketsById.Add(ticket.CharId, ticket);
      }
      finally
      {
        lck.ExitWriteLock();
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="charId"></param>
    /// <returns>The Ticket of the Character with the given charId or null if the Character did not issue any.</returns>
    public Ticket GetTicket(uint charId)
    {
      lck.EnterReadLock();
      try
      {
        Ticket ticket;
        ticketsById.TryGetValue(charId, out ticket);
        return ticket;
      }
      finally
      {
        lck.ExitReadLock();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handler">May be null</param>
    /// <returns>Returns the next Ticket that is not already handled and may be handled by the given handler.</returns>
    public Ticket GetNextUnhandledTicket(ITicketHandler handler)
    {
      lck.EnterReadLock();
      try
      {
        for(Ticket ticket = first; ticket != null; ticket = ticket.Next)
        {
          if(handler == null || handler.MayHandle(ticket))
            return ticket;
        }
      }
      finally
      {
        lck.ExitReadLock();
      }

      return null;
    }

    public Ticket HandleNextUnhandledTicket(ITicketHandler handler)
    {
      Ticket nextUnhandledTicket;
      do
      {
        nextUnhandledTicket = GetNextUnhandledTicket(handler);
        if(nextUnhandledTicket != null)
        {
          lck.EnterWriteLock();
          try
          {
            if(nextUnhandledTicket.Handler == null)
            {
              nextUnhandledTicket.SetHandlerUnlocked(handler);
              break;
            }
          }
          finally
          {
            lck.ExitWriteLock();
          }
        }
      } while(nextUnhandledTicket != null);

      return nextUnhandledTicket;
    }

    public bool HandleTicket(ITicketHandler handler, Ticket ticket)
    {
      if(!handler.MayHandle(ticket))
        return false;
      ticket.Handler = handler;
      return true;
    }

    public Ticket[] GetAllTickets()
    {
      lck.EnterReadLock();
      Ticket[] ticketArray = new Ticket[ticketsById.Count];
      try
      {
        Ticket ticket = first;
        int num = 0;
        for(; ticket != null; ticket = ticket.Next)
          ticketArray[num++] = ticket;
      }
      finally
      {
        lck.ExitReadLock();
      }

      return ticketArray;
    }

    public IEnumerator<Ticket> GetEnumerator()
    {
      return new TicketEnumerator(this);
    }

    private class TicketEnumerator : IEnumerator<Ticket>, IDisposable, IEnumerator
    {
      private TicketMgr mgr;
      private Ticket current;

      public TicketEnumerator(TicketMgr mgr)
      {
        this.mgr = mgr;
      }

      public void Dispose()
      {
        mgr = null;
        current = null;
      }

      public bool MoveNext()
      {
        if(current == null)
        {
          current = mgr.first;
          return current != null;
        }

        if(current.Next == null)
          return false;
        current = current.Next;
        return true;
      }

      public void Reset()
      {
        current = null;
      }

      public Ticket Current
      {
        get { return current; }
      }

      object IEnumerator.Current
      {
        get { return current; }
      }
    }
  }
}