using NLog;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WCell.Intercommunication;

namespace WCell.RealmServer.IPC
{
    /// <summary>Defines a host with the authentication service.</summary>
    public static class IPCServiceHost
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static ServiceHost Host;

        public static bool IsOpen
        {
            get
            {
                if (IPCServiceHost.Host != null)
                    return IPCServiceHost.Host.State == CommunicationState.Opened;
                return false;
            }
        }

        /// <summary>Starts the authentication service</summary>
        public static void StartService()
        {
            if (IPCServiceHost.IsOpen)
                return;
            IPCServiceHost.StopService();
            lock (typeof(IPCServiceHost))
            {
                IPCServiceHost.Host = new ServiceHost(typeof(IPCServiceAdapter), new Uri[0]);
                IPCServiceHost.Host.AddServiceEndpoint(typeof(IWCellIntercomService), (Binding) new NetTcpBinding()
                {
                    Security =
                    {
                        Mode = SecurityMode.None
                    }
                }, RealmServerConfiguration.IPCAddress);
                IPCServiceHost.Host.Open();
                string absoluteUri = IPCServiceHost.Host.Description.Endpoints[0].ListenUri.AbsoluteUri;
                IPCServiceHost.log.Info("IPC service started on {0}", absoluteUri);
            }
        }

        /// <summary>Stops the service.</summary>
        public static void StopService()
        {
            lock (typeof(IPCServiceHost))
            {
                if (IPCServiceHost.Host != null && IPCServiceHost.Host.State != CommunicationState.Closed)
                {
                    if (IPCServiceHost.Host.State != CommunicationState.Faulted)
                    {
                        try
                        {
                            IPCServiceHost.Host.Close();
                        }
                        catch (Exception ex)
                        {
                        }

                        IPCServiceHost.log.Info("IPC service stoped.");
                    }
                }

                IPCServiceHost.Host = (ServiceHost) null;
            }
        }
    }
}