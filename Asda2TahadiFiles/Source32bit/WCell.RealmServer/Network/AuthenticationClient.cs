using NLog;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WCell.Core;
using WCell.Core.Timers;
using WCell.Intercommunication.Client;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Res;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.RealmServer.Network
{
  /// <summary>
  /// Provides a client wrapper around the authentication service used for
  /// authentication-to-realm server communication.
  /// </summary>
  public class AuthenticationClient
  {
    protected static Logger log = LogManager.GetCurrentClassLogger();
    [Variable("IPCUpdateInterval")]public static int UpdateInterval = 5;
    private readonly object lck = new object();
    private AuthenticationClientAdapter m_ClientProxy;
    private string m_netAddr;
    private bool m_IsRunning;
    private readonly NetTcpBinding binding;
    private DateTime lastUpdate;
    private bool m_warned;
    private string m_warnInfo;

    /// <summary>
    /// Is called when the RealmServer successfully connects to the AuthServer
    /// </summary>
    public event EventHandler Connected;

    /// <summary>
    /// Is called when the RealmServer disconnects from or loses connection to the AuthServer
    /// </summary>
    public event EventHandler Disconnected;

    /// <summary>Initializes this Authentication Client</summary>
    public AuthenticationClient()
    {
      m_IsRunning = true;
      binding = new NetTcpBinding
      {
        Security =
        {
          Mode = SecurityMode.None
        }
      };
    }

    /// <summary>
    /// If set to false, will disonnect (if connected) and stop trying to re-connect.
    /// </summary>
    public bool IsRunning
    {
      get { return m_IsRunning; }
      set
      {
        m_IsRunning = value;
        ForceUpdate();
      }
    }

    /// <summary>Whether or not the service channel is open.</summary>
    public bool IsConnected
    {
      get
      {
        if(m_ClientProxy != null && m_ClientProxy.State == CommunicationState.Opened)
          return ServerApp<RealmServer>.Instance.IsRunning;
        return false;
      }
    }

    /// <summary>The adapter to the authentication service channel.</summary>
    public AuthenticationClientAdapter Channel
    {
      get { return m_ClientProxy; }
    }

    /// <summary>
    /// 
    /// </summary>
    public string ChannelId { get; internal set; }

    /// <summary>
    /// Notifies the conection maintenance to be re-scheduled immediately.
    /// Does not wait for the reconnect attempt to start or finish.
    /// </summary>
    public void ForceUpdate()
    {
      RearmDisconnectWarning();
      lastUpdate = DateTime.Now - TimeSpan.FromSeconds(UpdateInterval);
    }

    public void StartConnect(string netAddr)
    {
      RearmDisconnectWarning();
      m_netAddr = netAddr;
      m_IsRunning = true;
      if(!(lastUpdate == new DateTime()))
        return;
      ServerApp<RealmServer>.IOQueue.RegisterUpdatable(
        new SimpleUpdatable(MaintainConnectionCallback));
      lastUpdate = DateTime.Now;
    }

    /// <summary>Must be executed in RealmServer context</summary>
    protected bool Connect()
    {
      if(!m_warned)
      {
        AddDisconnectWarningToTitle();
        log.Info(WCell_RealmServer.ConnectingToAuthServer);
      }

      ServerApp<RealmServer>.IOQueue.EnsureContext();
      Disconnect(true);
      m_ClientProxy =
        new AuthenticationClientAdapter(binding, new EndpointAddress(m_netAddr));
      m_ClientProxy.Error += OnError;
      bool flag;
      try
      {
        m_ClientProxy.Open();
        ServerApp<RealmServer>.Instance.RegisterRealm();
        flag = IsConnected;
        lastUpdate = DateTime.Now;
      }
      catch(Exception ex)
      {
        m_ClientProxy = null;
        if(ex is EndpointNotFoundException)
        {
          if(!m_warned)
            log.Error(WCell_RealmServer.IPCProxyFailed,
              UpdateInterval);
        }
        else
          LogUtil.ErrorException(ex, WCell_RealmServer.IPCProxyFailedException, (object) UpdateInterval);

        flag = false;
      }

      m_warned = true;
      if(flag)
      {
        RearmDisconnectWarning();
        EventHandler connected = Connected;
        if(connected != null)
          connected(this, null);
      }
      else
        ScheduleReconnect();

      return flag;
    }

    protected void OnError(Exception ex)
    {
      if(ex is CommunicationException)
        log.Warn("Lost connection to AuthServer. Scheduling reconnection attempt...");
      else
        LogUtil.ErrorException(ex, WCell_RealmServer.CommunicationException);
      ScheduleReconnect();
    }

    protected void ScheduleReconnect()
    {
      Disconnect(false);
    }

    private void MaintainConnectionCallback()
    {
      if((DateTime.Now - lastUpdate).TotalSeconds < UpdateInterval)
        return;
      if(!m_IsRunning)
      {
        if(!IsConnected)
          return;
        Disconnect(true);
      }
      else if(!IsConnected)
      {
        lock(lck)
        {
          if(!ServerApp<RealmServer>.Instance.IsRunning)
          {
            RearmDisconnectWarning();
          }
          else
          {
            if(!Connect())
              return;
            log.Info(WCell_RealmServer.IPCProxyReconnected);
          }
        }
      }
      else
      {
        lock(lck)
        {
          if(!IsConnected)
            return;
          ServerApp<RealmServer>.Instance.UpdateRealm();
          lastUpdate = DateTime.Now;
        }
      }
    }

    protected void Disconnect(bool notify)
    {
      ServerApp<RealmServer>.IOQueue.EnsureContext();
      if(m_ClientProxy == null || m_ClientProxy.State == CommunicationState.Closed ||
         m_ClientProxy.State == CommunicationState.Closing)
        return;
      AddDisconnectWarningToTitle();
      try
      {
        if(notify && m_ClientProxy.State == CommunicationState.Opened)
          ServerApp<RealmServer>.Instance.UnregisterRealm();
        lock(lck)
        {
          m_ClientProxy.Close();
          m_ClientProxy = null;
        }
      }
      catch
      {
      }

      EventHandler disconnected = Disconnected;
      if(disconnected == null)
        return;
      disconnected(this, null);
    }

    private void AddDisconnectWarningToTitle()
    {
      if(!ServerApp<RealmServer>.Instance.ConsoleActive)
        return;
      m_warnInfo = " - ######### " +
                   RealmLocalizer.Instance.Translate(RealmLangKey.NotConnectedToAuthServer)
                     .ToUpper() + " #########";
      Console.Title += m_warnInfo;
    }

    private void RearmDisconnectWarning()
    {
      m_warned = false;
      if(!ServerApp<RealmServer>.Instance.ConsoleActive || m_warnInfo == null)
        return;
      Console.Title = Console.Title.Replace(m_warnInfo, "");
      m_warnInfo = null;
    }
  }
}