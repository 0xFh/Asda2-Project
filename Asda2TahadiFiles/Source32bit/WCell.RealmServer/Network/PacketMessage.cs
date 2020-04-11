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
      catch(EndOfStreamException ex)
      {
        LogUtil.ErrorException(ex, "End of stream on {0}. Length {1}. Position {2}",
          (object) Parameter2, (object) Parameter2.Length, (object) Parameter2.Position);
        Parameter2.Position = 0;
        string stringRepresentation =
          Utility.GetStringRepresentation(Parameter2.ReadBytes(Parameter2.Length));
        LogUtil.ErrorException(ex, "Data {0}", (object) stringRepresentation);
      }
      catch(SocketException ex)
      {
        Parameter1.Disconnect(false);
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex, "Client {0} triggered an Exception.", (object) Parameter1);
      }
      finally
      {
        Parameter2.Dispose();
      }
    }
  }
}