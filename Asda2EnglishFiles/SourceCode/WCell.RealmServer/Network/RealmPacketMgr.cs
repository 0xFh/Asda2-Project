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
                if (packet.PacketId.RawId == 1000U)
                    return true;
                PacketHandler<IRealmClient, RealmPacketIn> handlerDesc =
                    this.m_handlers.Get<PacketHandler<IRealmClient, RealmPacketIn>>(packet.PacketId.RawId);
                try
                {
                    if (handlerDesc == null)
                    {
                        this.HandleUnhandledPacket(client, packet);
                        return true;
                    }

                    IContextHandler contextHandler = this.CheckConstraints(client, handlerDesc, packet);
                    if (contextHandler == null)
                        return false;
                    contextHandler.AddMessage((IMessage) new PacketMessage(handlerDesc.Handler, client, packet));
                    flag = false;
                    return true;
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, WCell_RealmServer.PacketHandleException, new object[2]
                    {
                        (object) client,
                        (object) packet.PacketId
                    });
                    return false;
                }
            }
            finally
            {
                if (flag)
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
            if (!client.IsConnected)
                return (IContextHandler) null;
            Character activeCharacter = client.ActiveCharacter;
            if (handlerDesc.RequiresLogIn && activeCharacter == null)
            {
                RealmPacketMgr.s_log.Warn("Client {0} sent Packet {1} without selected Character.", (object) client,
                    (object) packet);
                return (IContextHandler) null;
            }

            RealmAccount account = client.Account;
            if (!handlerDesc.IsGamePacket)
            {
                if (account == null || !account.IsEnqueued)
                    return (IContextHandler) ServerApp<WCell.RealmServer.RealmServer>.IOQueue;
                RealmPacketMgr.s_log.Warn("Enqueued client {0} sent: {1}", (object) client, (object) packet);
                return (IContextHandler) null;
            }

            if (activeCharacter == null || account == null)
            {
                RealmPacketMgr.s_log.Warn("Client {0} sent Packet {1} before login completed.", (object) client,
                    (object) packet);
                client.Disconnect(false);
            }
            else
            {
                if (activeCharacter.Map != null)
                    return (IContextHandler) activeCharacter;
                RealmPacketMgr.s_log.Warn("Received packet {0} from Character {1} while not in world.", (object) packet,
                    (object) activeCharacter);
                client.Disconnect(false);
            }

            return (IContextHandler) null;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Second, "Register packet handlers")]
        public static void RegisterPacketHandlers()
        {
            RealmPacketMgr.Instance.RegisterAll(Assembly.GetExecutingAssembly());
            RealmPacketMgr.s_log.Debug(WCell_RealmServer.RegisteredAllHandlers);
        }
    }
}