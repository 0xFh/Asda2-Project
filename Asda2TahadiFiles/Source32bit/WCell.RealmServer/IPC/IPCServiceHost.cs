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
        if(Host != null)
          return Host.State == CommunicationState.Opened;
        return false;
      }
    }

    /// <summary>Starts the authentication service</summary>
    public static void StartService()
    {
      if(IsOpen)
        return;
      StopService();
      lock(typeof(IPCServiceHost))
      {
        Host = new ServiceHost(typeof(IPCServiceAdapter));
        Host.AddServiceEndpoint(typeof(IWCellIntercomService), new NetTcpBinding
        {
          Security =
          {
            Mode = SecurityMode.None
          }
        }, RealmServerConfiguration.IPCAddress);
        Host.Open();
        string absoluteUri = Host.Description.Endpoints[0].ListenUri.AbsoluteUri;
        log.Info("IPC service started on {0}", absoluteUri);
      }
    }

    /// <summary>Stops the service.</summary>
    public static void StopService()
    {
      lock(typeof(IPCServiceHost))
      {
        if(Host != null && Host.State != CommunicationState.Closed)
        {
          if(Host.State != CommunicationState.Faulted)
          {
            try
            {
              Host.Close();
            }
            catch(Exception ex)
            {
            }

            log.Info("IPC service stoped.");
          }
        }

        Host = null;
      }
    }
  }
}