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
        [Variable("IPCUpdateInterval")] public static int UpdateInterval = 5;
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
            this.m_IsRunning = true;
            this.binding = new NetTcpBinding()
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
            get { return this.m_IsRunning; }
            set
            {
                this.m_IsRunning = value;
                this.ForceUpdate();
            }
        }

        /// <summary>Whether or not the service channel is open.</summary>
        public bool IsConnected
        {
            get
            {
                if (this.m_ClientProxy != null && this.m_ClientProxy.State == CommunicationState.Opened)
                    return ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning;
                return false;
            }
        }

        /// <summary>The adapter to the authentication service channel.</summary>
        public AuthenticationClientAdapter Channel
        {
            get { return this.m_ClientProxy; }
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
            this.RearmDisconnectWarning();
            this.lastUpdate = DateTime.Now - TimeSpan.FromSeconds((double) AuthenticationClient.UpdateInterval);
        }

        public void StartConnect(string netAddr)
        {
            this.RearmDisconnectWarning();
            this.m_netAddr = netAddr;
            this.m_IsRunning = true;
            if (!(this.lastUpdate == new DateTime()))
                return;
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.RegisterUpdatable(
                (IUpdatable) new SimpleUpdatable(new Action(this.MaintainConnectionCallback)));
            this.lastUpdate = DateTime.Now;
        }

        /// <summary>Must be executed in RealmServer context</summary>
        protected bool Connect()
        {
            if (!this.m_warned)
            {
                this.AddDisconnectWarningToTitle();
                AuthenticationClient.log.Info(WCell_RealmServer.ConnectingToAuthServer);
            }

            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.EnsureContext();
            this.Disconnect(true);
            this.m_ClientProxy =
                new AuthenticationClientAdapter((Binding) this.binding, new EndpointAddress(this.m_netAddr));
            this.m_ClientProxy.Error += new Action<Exception>(this.OnError);
            bool flag;
            try
            {
                this.m_ClientProxy.Open();
                ServerApp<WCell.RealmServer.RealmServer>.Instance.RegisterRealm();
                flag = this.IsConnected;
                this.lastUpdate = DateTime.Now;
            }
            catch (Exception ex)
            {
                this.m_ClientProxy = (AuthenticationClientAdapter) null;
                if (ex is EndpointNotFoundException)
                {
                    if (!this.m_warned)
                        AuthenticationClient.log.Error(WCell_RealmServer.IPCProxyFailed,
                            AuthenticationClient.UpdateInterval);
                }
                else
                    LogUtil.ErrorException(ex, WCell_RealmServer.IPCProxyFailedException, new object[1]
                    {
                        (object) AuthenticationClient.UpdateInterval
                    });

                flag = false;
            }

            this.m_warned = true;
            if (flag)
            {
                this.RearmDisconnectWarning();
                EventHandler connected = this.Connected;
                if (connected != null)
                    connected((object) this, (EventArgs) null);
            }
            else
                this.ScheduleReconnect();

            return flag;
        }

        protected void OnError(Exception ex)
        {
            if (ex is CommunicationException)
                AuthenticationClient.log.Warn("Lost connection to AuthServer. Scheduling reconnection attempt...");
            else
                LogUtil.ErrorException(ex, WCell_RealmServer.CommunicationException, new object[0]);
            this.ScheduleReconnect();
        }

        protected void ScheduleReconnect()
        {
            this.Disconnect(false);
        }

        private void MaintainConnectionCallback()
        {
            if ((DateTime.Now - this.lastUpdate).TotalSeconds < (double) AuthenticationClient.UpdateInterval)
                return;
            if (!this.m_IsRunning)
            {
                if (!this.IsConnected)
                    return;
                this.Disconnect(true);
            }
            else if (!this.IsConnected)
            {
                lock (this.lck)
                {
                    if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    {
                        this.RearmDisconnectWarning();
                    }
                    else
                    {
                        if (!this.Connect())
                            return;
                        AuthenticationClient.log.Info(WCell_RealmServer.IPCProxyReconnected);
                    }
                }
            }
            else
            {
                lock (this.lck)
                {
                    if (!this.IsConnected)
                        return;
                    ServerApp<WCell.RealmServer.RealmServer>.Instance.UpdateRealm();
                    this.lastUpdate = DateTime.Now;
                }
            }
        }

        protected void Disconnect(bool notify)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.EnsureContext();
            if (this.m_ClientProxy == null || this.m_ClientProxy.State == CommunicationState.Closed ||
                this.m_ClientProxy.State == CommunicationState.Closing)
                return;
            this.AddDisconnectWarningToTitle();
            try
            {
                if (notify && this.m_ClientProxy.State == CommunicationState.Opened)
                    ServerApp<WCell.RealmServer.RealmServer>.Instance.UnregisterRealm();
                lock (this.lck)
                {
                    this.m_ClientProxy.Close();
                    this.m_ClientProxy = (AuthenticationClientAdapter) null;
                }
            }
            catch
            {
            }

            EventHandler disconnected = this.Disconnected;
            if (disconnected == null)
                return;
            disconnected((object) this, (EventArgs) null);
        }

        private void AddDisconnectWarningToTitle()
        {
            if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.ConsoleActive)
                return;
            this.m_warnInfo = " - ######### " +
                              RealmLocalizer.Instance.Translate(RealmLangKey.NotConnectedToAuthServer, new object[0])
                                  .ToUpper() + " #########";
            Console.Title += this.m_warnInfo;
        }

        private void RearmDisconnectWarning()
        {
            this.m_warned = false;
            if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.ConsoleActive || this.m_warnInfo == null)
                return;
            Console.Title = Console.Title.Replace(this.m_warnInfo, "");
            this.m_warnInfo = (string) null;
        }
    }
}