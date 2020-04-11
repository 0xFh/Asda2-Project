using System;
using System.Collections.Generic;
using System.Threading;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Handlers;
using WCell.Util.Collections;
using WCell.Util.NLog;

namespace WCell.RealmServer.Network
{
  /// <summary>
  /// Manages the queue of overflowed clients connecting to the server when it is full.
  /// </summary>
  public static class AuthQueue
  {
    private static LockfreeQueue<IRealmClient> s_queuedClients = new LockfreeQueue<IRealmClient>();
    private static Timer s_checkTimer = new Timer(ProcessQueuedClients);

    static AuthQueue()
    {
      s_checkTimer.Change(TimeSpan.FromSeconds(15.0), TimeSpan.FromSeconds(15.0));
    }

    /// <summary>The number of clients currently waiting in the queue.</summary>
    public static int QueuedClients
    {
      get { return s_queuedClients.Count; }
    }

    /// <summary>Adds a client to the queue.</summary>
    /// <param name="client">the client to add to the queue</param>
    public static void EnqueueClient(IRealmClient client)
    {
      client.Account.IsEnqueued = true;
      s_queuedClients.Enqueue(client);
      LoginHandler.SendAuthQueueStatus(client);
    }

    /// <summary>
    /// Goes through the queue, pulling out clients for the number of slots available at the time.
    /// </summary>
    /// <param name="state">the timer object</param>
    private static void ProcessQueuedClients(object state)
    {
      List<IRealmClient> realmClientList = new List<IRealmClient>();
      try
      {
        IRealmClient realmClient;
        for(int index = RealmServerConfiguration.MaxClientCount -
                        ServerApp<RealmServer>.Instance.AcceptedClients;
          index != 0 && s_queuedClients.TryDequeue(out realmClient);
          --index)
          realmClientList.Add(realmClient);
        int num = 0;
        using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_AUTH_RESPONSE))
        {
          packet.Write((byte) 27);
          packet.Write(0);
          foreach(IRealmClient queuedClient in s_queuedClients)
          {
            packet.InsertIntAt(num++, 5L, false);
            queuedClient.Send(packet, false);
          }
        }
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex, "AuthQueue raised an Exception.");
      }
      finally
      {
        foreach(IRealmClient client in realmClientList)
        {
          client.Account.IsEnqueued = false;
          LoginHandler.InviteToRealm(client);
        }
      }
    }
  }
}