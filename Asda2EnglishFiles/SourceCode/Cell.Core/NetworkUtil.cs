using System.Net;

namespace Cell.Core
{
    public static class NetworkUtil
    {
        private static IPHostEntry hostEntry;

        public static IPHostEntry CachedHostEntry
        {
            get
            {
                if (NetworkUtil.hostEntry == null)
                    NetworkUtil.hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                return NetworkUtil.hostEntry;
            }
        }

        public static IPAddress GetMatchingLocalIP(IPAddress clientAddr)
        {
            if (clientAddr.Equals((object) IPAddress.Any) || clientAddr.Equals((object) IPAddress.Loopback))
                return IPAddress.Loopback;
            byte[] addressBytes1 = clientAddr.GetAddressBytes();
            foreach (IPAddress address in NetworkUtil.CachedHostEntry.AddressList)
            {
                byte[] addressBytes2 = address.GetAddressBytes();
                if (addressBytes2.Length == addressBytes1.Length)
                {
                    bool flag = true;
                    for (int index = addressBytes2.Length - 2; index >= 0; --index)
                    {
                        if ((int) addressBytes2[index] != (int) addressBytes1[index])
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                        return address;
                }
            }

            return (IPAddress) null;
        }
    }
}