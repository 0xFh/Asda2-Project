using NLog;
using System;
using System.IO;
using System.Net.Sockets;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Network
{
    public class PacketMessage : Message2<IRealmClient, RealmPacketIn>
    {
        protected static Logger s_log = LogManager.GetCurrentClassLogger();

        public PacketMessage(Action<IRealmClient, RealmPacketIn> callback, IRealmClient client, RealmPacketIn pkt)
            : base(client, pkt, callback)
        {
        }

        public override void Execute()
        {
            try
            {
                base.Execute();
            }
            catch (EndOfStreamException ex)
            {
                LogUtil.ErrorException((Exception) ex, "End of stream on {0}. Length {1}. Position {2}",
                    (object) this.Parameter2, (object) this.Parameter2.Length, (object) this.Parameter2.Position);
                this.Parameter2.Position = 0;
                string stringRepresentation =
                    Utility.GetStringRepresentation((object) this.Parameter2.ReadBytes(this.Parameter2.Length));
                LogUtil.ErrorException((Exception) ex, "Data {0}", new object[1]
                {
                    (object) stringRepresentation
                });
            }
            catch (SocketException ex)
            {
                this.Parameter1.Disconnect(false);
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, "Client {0} triggered an Exception.", new object[1]
                {
                    (object) this.Parameter1
                });
            }
            finally
            {
                this.Parameter2.Dispose();
            }
        }
    }
}