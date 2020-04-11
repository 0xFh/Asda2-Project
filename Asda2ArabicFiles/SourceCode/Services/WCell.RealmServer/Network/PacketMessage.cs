using System;
using NLog;
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
				//var contextHandler = RealmPacketMgr.Instance.CheckConstraints(Parameter1, Parameter2);
				//if (contextHandler != null && contextHandler.IsInContext)

				// Character-messages are always executed in the right context
				base.Execute();
            }
            catch (System.IO.EndOfStreamException e) { LogUtil.ErrorException(e, "End of stream on {0}. Length {1}. Position {2}",Parameter2, Parameter2.Length, Parameter2.Position);
                Parameter2.Position = 0;
                var packetStr = Utility.GetStringRepresentation(Parameter2.ReadBytes(Parameter2.Length));
                LogUtil.ErrorException(e,"Data {0}",packetStr);
            }
            catch (System.Net.Sockets.SocketException)
            {
                Parameter1.Disconnect();
            }
			catch (Exception e)
			{
				LogUtil.ErrorException(e, "Client {0} triggered an Exception.", Parameter1);
/*
				if (Parameter1.ActiveCharacter != null)
				{
					Parameter1.ActiveCharacter.SaveLater();
				}

				Parameter1.Disconnect();*/
			}
			finally
			{
				((IDisposable)Parameter2).Dispose();
			}
		}
	}
}