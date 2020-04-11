using System;
using System.Net.Sockets;

namespace Cell.Core
{
    public static class SocketHelpers
    {
        static SocketHelpers()
        {
            if (ObjectPoolMgr.ContainsType<SocketAsyncEventArgs>())
                return;
            ObjectPoolMgr.RegisterType<SocketAsyncEventArgs>(
                new Func<SocketAsyncEventArgs>(SocketHelpers.CreateSocketArg));
            ObjectPoolMgr.SetMinimumSize<SocketAsyncEventArgs>(100);
        }

        private static SocketAsyncEventArgs CreateSocketArg()
        {
            return new SocketAsyncEventArgs();
        }

        private static void CleanSocketArg(SocketAsyncEventArgs arg)
        {
        }

        public static SocketAsyncEventArgs AcquireSocketArg()
        {
            SocketAsyncEventArgs socketAsyncEventArgs = ObjectPoolMgr.ObtainObject<SocketAsyncEventArgs>();
            SocketHelpers.CleanSocketArg(socketAsyncEventArgs);
            return socketAsyncEventArgs;
        }

        public static void ReleaseSocketArg(SocketAsyncEventArgs arg)
        {
            ObjectPoolMgr.ReleaseObject<SocketAsyncEventArgs>(arg);
        }

        public static void SetListenSocketOptions(Socket socket)
        {
            socket.NoDelay = true;
        }
    }
}