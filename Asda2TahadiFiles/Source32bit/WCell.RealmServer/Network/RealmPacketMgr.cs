using NLog;
using System;
using System.Reflection;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Res;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Network
{
  /// <summary>Manages packet handlers and the execution of them.</summary>
  public class RealmPacketMgr : PacketManager<IRealmClient, RealmPacketIn, PacketHandlerAttribute>
  {
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
    public static readonly RealmPacketMgr Instance = new RealmPacketMgr();

    public override uint MaxHandlers
    {
      get { return 20001; }
    }

    /// <summary>
    /// Attempts to handle an incoming packet.
    /// Constraints: OpCode must be valid.
    /// GamePackets cannot be sent if ActiveCharacter == null or Account == null.
    /// The packet is disposed after being handled.
    /// </summary>
    /// <param name="client">the client the packet is from</param>
    /// <param name="packet">the packet to be handled</param>
    /// <returns>true if the packet could be handled or false otherwise</returns>
    public override bool HandlePacket(IRealmClient client, RealmPacketIn packet)
    {
      bool flag = true;
      try
      {
        if(packet.PacketId.RawId == 1000U)
          return true;
        PacketHandler<IRealmClient, RealmPacketIn> handlerDesc =
          m_handlers.Get(packet.PacketId.RawId);
        try
        {
          if(handlerDesc == null)
          {
            HandleUnhandledPacket(client, packet);
            return true;
          }

          IContextHandler contextHandler = CheckConstraints(client, handlerDesc, packet);
          if(contextHandler == null)
            return false;
          contextHandler.AddMessage(new PacketMessage(handlerDesc.Handler, client, packet));
          flag = false;
          return true;
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex, WCell_RealmServer.PacketHandleException, (object) client,
            (object) packet.PacketId);
          return false;
        }
      }
      finally
      {
        if(flag)
          packet.Close();
      }
    }

    /// <summary>
    /// Gets the <see cref="T:WCell.Util.Threading.IContextHandler" /> that should handle this incoming packet
    /// </summary>
    /// <param name="client"></param>
    /// <param name="packet"></param>
    /// <returns></returns>
    public IContextHandler CheckConstraints(IRealmClient client,
      PacketHandler<IRealmClient, RealmPacketIn> handlerDesc, RealmPacketIn packet)
    {
      if(!client.IsConnected)
        return null;
      Character activeCharacter = client.ActiveCharacter;
      if(handlerDesc.RequiresLogIn && activeCharacter == null)
      {
        s_log.Warn("Client {0} sent Packet {1} without selected Character.", client,
          packet);
        return null;
      }

      RealmAccount account = client.Account;
      if(!handlerDesc.IsGamePacket)
      {
        if(account == null || !account.IsEnqueued)
          return ServerApp<RealmServer>.IOQueue;
        s_log.Warn("Enqueued client {0} sent: {1}", client, packet);
        return null;
      }

      if(activeCharacter == null || account == null)
      {
        s_log.Warn("Client {0} sent Packet {1} before login completed.", client,
          packet);
        client.Disconnect(false);
      }
      else
      {
        if(activeCharacter.Map != null)
          return activeCharacter;
        s_log.Warn("Received packet {0} from Character {1} while not in world.", packet,
          activeCharacter);
        client.Disconnect(false);
      }

      return null;
    }

    [Initialization(InitializationPass.Second, "Register packet handlers")]
    public static void RegisterPacketHandlers()
    {
      Instance.RegisterAll(Assembly.GetExecutingAssembly());
      s_log.Debug(WCell_RealmServer.RegisteredAllHandlers);
    }
  }
}