using WCell.Core.Network;

namespace WCell.RealmServer.Network
{
    public interface IPacketReceiver
    {
        Locale Locale { get; set; }

        /// <summary>Sends a packet to the target.</summary>
        /// <param name="packet">the packet to send</param>
        /// <param name="addEnd"> </param>
        void Send(RealmPacketOut packet, bool addEnd = false);
    }
}