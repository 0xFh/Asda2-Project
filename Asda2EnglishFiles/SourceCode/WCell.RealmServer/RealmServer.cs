using Cell.Core;
using System;
using System.Collections.Generic;
using System.Net;
using WCell.Constants.Login;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.IPC;
using WCell.RealmServer.Network;
using WCell.RealmServer.Res;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer
{
    /// <summary>
    /// Server class for the realm server. Handles all initial
    /// connections and verifies authentication with the
    /// authentication server
    /// </summary>
    [VariableClass(true)]
    public sealed class RealmServer : ServerApp<WCell.RealmServer.RealmServer>
    {
        private readonly AuthenticationClient m_authServiceClient = new AuthenticationClient();

        public readonly Dictionary<string, RealmAccount> LoggedInAccounts =
            new Dictionary<string, RealmAccount>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        public readonly Dictionary<uint, RealmAccount> LoggedInAccountsById = new Dictionary<uint, RealmAccount>();
        private readonly byte[] m_authSeed = BitConverter.GetBytes(new Random().Next());
        private static DateTime timeStart;
        private static long timeStartTicks;
        private readonly RealmServerConfiguration m_configuration;
        private volatile int m_acceptedClients;

        public event Action<RealmStatus> StatusChanged;

        [Variable(IsReadOnly = true)]
        public static DateTime IngameTime
        {
            get
            {
                return WCell.RealmServer.RealmServer.timeStart.AddMinutes(
                    (double) (DateTime.Now.Ticks - WCell.RealmServer.RealmServer.timeStartTicks) *
                    (double) RealmServerConfiguration.IngameMinutesPerSecond / 10000000.0);
            }
            set
            {
                WCell.RealmServer.RealmServer.timeStart = value;
                WCell.RealmServer.RealmServer.timeStartTicks = DateTime.Now.Ticks;
                if (!ServerApp<WCell.RealmServer.RealmServer>.Instance.IsRunning)
                    return;
                using (List<Character>.Enumerator enumerator = World.GetAllCharacters().GetEnumerator())
                {
                    do
                        ;
                    while (enumerator.MoveNext() && enumerator.Current.Client != null);
                }
            }
        }

        /// <summary>Default constructor</summary>
        public RealmServer()
        {
            this.m_configuration = new RealmServerConfiguration(ServerApp<WCell.RealmServer.RealmServer>.EntryLocation);
            this.m_acceptedClients = 0;
        }

        /// <summary>The configuration for the realm server.</summary>
        public RealmServerConfiguration Configuration
        {
            get { return this.m_configuration; }
        }

        /// <summary>The authentication service client instance.</summary>
        public AuthenticationClient AuthClient
        {
            get { return this.m_authServiceClient; }
        }

        /// <summary>Number of clients fully accepted and authenticated.</summary>
        public int AcceptedClients
        {
            get { return this.m_acceptedClients; }
        }

        /// <summary>
        /// The randomly-generated seed used for pre-login authentication.
        /// </summary>
        public byte[] AuthSeed
        {
            get { return this.m_authSeed; }
        }

        public override string Host
        {
            get { return RealmServerConfiguration.Host; }
        }

        public override int Port
        {
            get { return RealmServerConfiguration.Port; }
        }

        /// <summary>
        /// Can only be used if RealmServerConfiguration.RegisterExternalAddress is true
        /// or if already connected, else will throw Exception.
        /// </summary>
        public string ExternalAddress
        {
            get
            {
                return !RealmServerConfiguration.RegisterExternalAddress
                    ? (string) null
                    : RealmServerConfiguration.ExternalAddress;
            }
        }

        /// <summary>
        /// Starts the server and begins accepting connections.
        /// Requires IO-Context.
        /// Also see <c>StartLater</c>
        /// </summary>
        public override void Start()
        {
            base.Start();
            int num = this._running ? 1 : 0;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Last)]
        public static void FinishSetup()
        {
            WCell.RealmServer.RealmServer.timeStart = DateTime.Now;
            WCell.RealmServer.RealmServer.timeStartTicks = WCell.RealmServer.RealmServer.timeStart.Ticks;
        }

        internal static void ResetTimeStart()
        {
            WCell.RealmServer.RealmServer.timeStart = WCell.RealmServer.RealmServer.IngameTime;
            WCell.RealmServer.RealmServer.timeStartTicks = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Establishes the initial connection with the authentication service.
        /// </summary>
        private void ConnectToAuthService()
        {
            this.m_authServiceClient.StartConnect(RealmServerConfiguration.AuthenticationServerAddress);
        }

        internal void OnStatusChange(RealmStatus oldStatus)
        {
            ServerApp<WCell.RealmServer.RealmServer>.Instance.UpdateRealm();
            Action<RealmStatus> statusChanged = this.StatusChanged;
            if (statusChanged == null)
                return;
            statusChanged(oldStatus);
        }

        /// <summary>Registers this Realm with the Authentication-Server</summary>
        public void RegisterRealm()
        {
        }

        /// <summary>
        /// Updates this Realm at the Authentication-Server.
        /// Is called automatically on a regular basis.
        /// </summary>
        public bool UpdateRealm()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void UnregisterRealm()
        {
            ServerBase.log.Info(WCell_RealmServer.IPCProxyDisconnected);
        }

        /// <summary>Called when a UDP packet is received</summary>
        /// <param name="num_bytes">the number of bytes received</param>
        /// <param name="buf">byte[] of the datagram</param>
        /// <param name="ip">the source IP of the datagram</param>
        protected override void OnReceiveUDP(int num_bytes, byte[] buf, IPEndPoint ip)
        {
        }

        /// <summary>Called when a UDP packet is sent</summary>
        /// <param name="clientIP">the destination IP of the datagram</param>
        /// <param name="num_bytes">the number of bytes sent</param>
        protected override void OnSendTo(IPEndPoint clientIP, int num_bytes)
        {
        }

        /// <summary>Creates a client object for a newly connected client</summary>
        /// <returns>a new IRealmClient object</returns>
        protected override IClient CreateClient()
        {
            return (IClient) new RealmClient(this);
        }

        /// <summary>
        /// Called when a client connects.
        /// A client cannot connect while the Realm is not connected
        /// to the AuthServer.
        /// </summary>
        /// <param name="client">the client object</param>
        /// <returns>false to shutdown the server</returns>
        protected override bool OnClientConnected(IClient client)
        {
            base.OnClientConnected(client);
            return true;
        }

        /// <summary>Called when a client disconnects</summary>
        /// <param name="client">the client object</param>
        /// <param name="forced">indicates if the client disconnection was forced</param>
        protected override void OnClientDisconnected(IClient client, bool forced)
        {
            IRealmClient client1 = client as IRealmClient;
            if (client1 != null && !client1.IsOffline)
            {
                client1.IsOffline = true;
                if (client1.AuthAccount != null)
                    client1.AuthAccount.IsLogedOn = false;
                LoginHandler.NotifyLogout(client1);
                RealmAccount account = client1.Account;
                if (account != null)
                {
                    account.Client = (IRealmClient) null;
                    Character chr = client1.ActiveCharacter;
                    if (chr != null && client1.IsGameServerConnection)
                    {
                        chr.IsConnected = false;
                        chr.AddMessage((Action) (() =>
                        {
                            if (chr.IsAsda2Teleporting)
                                chr.IsAsda2Teleporting = false;
                            else
                                chr.Logout(true, 0);
                        }));
                    }
                }
            }

            --this.m_acceptedClients;
            base.OnClientDisconnected(client, forced);
        }

        /// <summary>Called when a login is accepted.</summary>
        /// <param name="sender">the caller of the event</param>
        /// <param name="args">the arguments of the event</param>
        internal void OnClientAccepted(object sender, EventArgs args)
        {
            ++this.m_acceptedClients;
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsAccountLoggedIn(string name)
        {
            return this.LoggedInAccounts.ContainsKey(name);
        }

        /// <summary>
        /// Returns the logged in account with the given name.
        /// Requires IO-Context.
        /// </summary>
        public RealmAccount GetLoggedInAccount(string name)
        {
            RealmAccount realmAccount;
            this.LoggedInAccounts.TryGetValue(name, out realmAccount);
            return realmAccount;
        }

        /// <summary>
        /// Returns the logged in account with the given id.
        /// Requires IO-Context.
        /// </summary>
        public RealmAccount GetLoggedInAccount(uint id)
        {
            RealmAccount realmAccount;
            this.LoggedInAccountsById.TryGetValue(id, out realmAccount);
            return realmAccount;
        }

        /// <summary>
        /// Registers the given Account as currently connected.
        /// Requires IO-Context.
        /// </summary>
        internal void RegisterAccount(RealmAccount acc)
        {
            this.LoggedInAccounts.Add(acc.Name, acc);
            this.LoggedInAccountsById.Add((uint) acc.AccountId, acc);
            this.SetAccountLoggedIn(acc, true);
        }

        /// <summary>
        /// Removes the given Account from the list from currently connected Accounts.
        /// Requires IO-Context.
        /// </summary>
        /// <returns>Whether the Account was even flagged as logged in.</returns>
        internal void UnregisterAccount(RealmAccount acc)
        {
            if (acc == null)
                return;
            acc.ActiveCharacter = (Character) null;
            if (this.LoggedInAccounts.ContainsKey(acc.Name))
            {
                this.SetAccountLoggedIn(acc, false);
                this.LoggedInAccounts.Remove(acc.Name);
                this.LoggedInAccountsById.Remove((uint) acc.AccountId);
                if (!IPCServiceAdapter.AllConnectedClients.ContainsKey(acc.Name))
                    return;
                IPCServiceAdapter.AllConnectedClients[acc.Name].CurrentAccount = (RealmAccount) null;
            }
            else
                ServerApp<WCell.RealmServer.RealmServer>.Log.Warn(
                    "Tried to unregister non-registered account: " + (object) acc);
        }

        /// <summary>
        /// Updates the AuthServer about the login-status of the account with the given name.
        /// Accounts that are flagged as logged-in cannot connect again until its unset again.
        /// Called whenever client connects/disconnects.
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="loggedIn"></param>
        internal void SetAccountLoggedIn(RealmAccount acc, bool loggedIn)
        {
            if (loggedIn)
                acc.OnLogin();
            else
                acc.OnLogout();
        }

        public override void ShutdownIn(uint delayMillis)
        {
            World.BroadcastMsg("~Server~",
                string.Format("Shutdowning in {0} secs.Thanks for playing! :)", (object) (delayMillis / 1000U)),
                Color.DodgerBlue);
            base.ShutdownIn(delayMillis);
        }

        public override void CancelShutdown()
        {
            if (ServerApp<WCell.RealmServer.RealmServer>.IsPreparingShutdown)
                World.BroadcastMsg("~Server~", string.Format("Yeah! Some one stoped shutdowning :)"), Color.DodgerBlue);
            base.CancelShutdown();
        }

        public override void Stop()
        {
            if (this.m_authServiceClient != null)
                this.m_authServiceClient.IsRunning = false;
            base.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnShutdown()
        {
            World.Broadcast("Initiating Shutdown...");
            World.Broadcast("Halting server and saving World...");
            World.Save(true);
            if (RealmServerConfiguration.Instance.AutoSave)
                RealmServerConfiguration.Instance.Save(true, true);
            World.Broadcast("World saved.");
            World.Broadcast("Shutting down...");
        }

        public static string Title
        {
            get
            {
                return string.Format("{0} - ACell {1}", (object) RealmServerConfiguration.RealmName,
                    (object) "Amethyst");
            }
        }

        public static string FormattedTitle
        {
            get { return ChatUtility.Colorize(WCell.RealmServer.RealmServer.Title, Color.Purple); }
        }

        public override string ToString()
        {
            return string.Format("{2} - ACell {0} (v{1})", (object) this.GetType().Name,
                (object) ServerApp<WCell.RealmServer.RealmServer>.AssemblyVersion,
                (object) RealmServerConfiguration.RealmName);
        }
    }
}