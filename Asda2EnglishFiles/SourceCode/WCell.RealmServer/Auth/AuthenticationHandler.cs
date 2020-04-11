using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Auth.Firewall;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.Util.Threading;

namespace WCell.RealmServer.Auth
{
    /// <summary>Class that handles all authentication of the client.</summary>
    public static class AuthenticationHandler
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The time in milliseconds to let a client wait when failing to login
        /// </summary>
        public static int FailedLoginDelay = 200;

        private static readonly Dictionary<IPAddress, LoginFailInfo> failedLogins =
            new Dictionary<IPAddress, LoginFailInfo>();

        /// <summary>
        /// Is called whenever a client failed to authenticate itself
        /// </summary>
        public static event AuthenticationHandler.LoginFailedHandler LoginFailed;

        [PacketHandler(RealmServerOpCode.AuthorizeRequest, IsGamePacket = false, RequiresLogin = false)]
        public static void AuthChallengeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.Account != null)
                return;
            packet.Position += 20;
            string str1 = packet.ReadAsdaString(32, Locale.Start);
            if (client.Locale == Locale.Ru)
                packet.Position += 19;
            string str2 = packet.ReadAsdaString(32, Locale.Start);
            AuthenticationHandler.s_log.Debug(string.Format("{0} starting login chalange", (object) str1));
            client.AccountName = str1;
            client.Password = str2;
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message1<IRealmClient>(client,
                new Action<IRealmClient>(AuthenticationHandler.AuthChallengeRequestCallback)));
        }

        private static void AuthChallengeRequestCallback(IRealmClient client)
        {
            if (!client.IsConnected)
                return;
            if (BanMgr.IsBanned(client.ClientAddress))
                AuthenticationHandler.OnLoginError(client, AccountStatus.CloseClient);
            else
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                {
                    Account account = AccountMgr.GetAccount(client.AccountName);
                    if (account == null)
                    {
                        if (RealmServerConfiguration.AutocreateAccounts)
                            AuthenticationHandler.QueryAccountCallback(client, (Account) null);
                        else
                            AuthenticationHandler.OnLoginError(client, AccountStatus.WrongLoginOrPass);
                    }
                    else if (account.Password != client.Password)
                    {
                        if (client.ClientAddress != null)
                            Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) account.AccountId)
                                .AddAttribute("operation", 1.0, "login_wrong_pass")
                                .AddAttribute("name", 0.0, account.Name)
                                .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                        AuthenticationHandler.OnLoginError(client, AccountStatus.WrongLoginOrPass);
                    }
                    else
                        AuthenticationHandler.QueryAccountCallback(client, account);
                }));
        }

        public static void OnLoginError(IRealmClient client, AccountStatus error)
        {
            AuthenticationHandler.OnLoginError(client, error, false);
        }

        public static void OnLoginError(IRealmClient client, AccountStatus error, bool silent)
        {
            if (!silent)
                AuthenticationHandler.s_log.Debug("Client {0} failed to login: {1}", (object) client, (object) error);
            LoginFailInfo loginFailInfo;
            if (!AuthenticationHandler.failedLogins.TryGetValue(client.ClientAddress, out loginFailInfo))
            {
                AuthenticationHandler.failedLogins.Add(client.ClientAddress,
                    loginFailInfo = new LoginFailInfo(DateTime.Now));
            }
            else
            {
                loginFailInfo.LastAttempt = DateTime.Now;
                ++loginFailInfo.Count;
            }

            ThreadPool.RegisterWaitForSingleObject(loginFailInfo.Handle, (WaitOrTimerCallback) ((state, timedOut) =>
            {
                if (!client.IsConnected)
                    return;
                AuthenticationHandler.LoginFailedHandler loginFailed = AuthenticationHandler.LoginFailed;
                if (loginFailed != null)
                    loginFailed(client, error);
                AuthenticationHandler.SendAuthChallengeFailReply(client, error);
            }), (object) null, AuthenticationHandler.FailedLoginDelay, true);
        }

        private static void QueryAccountCallback(IRealmClient client, Account acct)
        {
            if (client == null || !client.IsConnected)
                return;
            if (acct != null)
            {
                Character characterByAccId = World.GetCharacterByAccId((uint) acct.AccountId);
                if (characterByAccId != null)
                {
                    characterByAccId.Logout(true, 0);
                    AuthenticationHandler.OnLoginError(client,
                        AccountStatus.WrongLoginOrPass | AccountStatus.AccountInUse);
                    return;
                }
            }

            string accountName = client.AccountName;
            RealmAccount loggedInAccount =
                ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(accountName);
            if (acct != null && acct.IsLogedOn)
            {
                Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) acct.AccountId)
                    .AddAttribute("operation", 1.0, "account_in_use").AddAttribute("name", 0.0, acct.Name)
                    .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                AuthenticationHandler.OnLoginError(client, AccountStatus.AccountInUse);
            }
            else if (loggedInAccount != null && loggedInAccount.ActiveCharacter != null &&
                     (loggedInAccount.Client != null && loggedInAccount.Client.IsConnected))
            {
                Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) acct.AccountId)
                    .AddAttribute("operation", 1.0, "account_in_use").AddAttribute("name", 0.0, acct.Name)
                    .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                AuthenticationHandler.OnLoginError(client, AccountStatus.AccountInUse);
            }
            else
            {
                if (acct == null)
                {
                    if (RealmServerConfiguration.AutocreateAccounts)
                    {
                        if (!AccountMgr.NameValidator(ref accountName) || client.Password == null ||
                            client.Password.Length > 20)
                        {
                            AuthenticationHandler.OnLoginError(client, AccountStatus.WrongLoginOrPass);
                            return;
                        }

                        client.AuthAccount = AccountMgr.Instance.CreateAccount(accountName, client.Password, "",
                            RealmServerConfiguration.DefaultRole);
                        client.AuthAccount.Save();
                        AuthenticationHandler.SendAuthChallengeSuccessReply(client);
                        client.AuthAccount.IsLogedOn = true;
                    }
                    else
                    {
                        AuthenticationHandler.OnLoginError(client, AccountStatus.WrongLoginOrPass);
                        return;
                    }
                }
                else if (acct.CheckActive())
                {
                    client.AuthAccount = acct;
                    if (loggedInAccount == null)
                        AuthenticationHandler.SendAuthChallengeSuccessReply(client);
                }
                else
                {
                    Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) acct.AccountId)
                        .AddAttribute("operation", 1.0, "login_banned").AddAttribute("name", 0.0, acct.Name)
                        .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                    if (client.AuthAccount == null || !client.AuthAccount.StatusUntil.HasValue)
                    {
                        AuthenticationHandler.OnLoginError(client, AccountStatus.WrongLoginOrPass);
                        return;
                    }

                    AuthenticationHandler.OnLoginError(client, AccountStatus.WrongLoginOrPass);
                    return;
                }

                if (loggedInAccount == null)
                {
                    if (acct != null)
                        Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) acct.AccountId)
                            .AddAttribute("operation", 1.0, "login_ok").AddAttribute("name", 0.0, acct.Name)
                            .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                    RealmAccount.InitializeAccount(client, client.AuthAccount.Name);
                }
                else
                {
                    if (acct == null)
                        return;
                    if (loggedInAccount.Client != null)
                    {
                        if (loggedInAccount.Client.ActiveCharacter != null)
                            loggedInAccount.Client.ActiveCharacter.SendInfoMsg(
                                "Some one loggin in to your account. Disconnecting.");
                        loggedInAccount.Client.Disconnect(false);
                    }

                    if (client.ClientAddress == null)
                        return;
                    loggedInAccount.LastIP = client.ClientAddress.GetAddressBytes();
                    acct.LastIP = client.ClientAddress.GetAddressBytes();
                    acct.Save();
                    client.Account = loggedInAccount;
                    if (loggedInAccount.ActiveCharacter != null)
                    {
                        AuthenticationHandler.ConnectClientToIngameCharacter(client, acct, loggedInAccount);
                    }
                    else
                    {
                        Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) acct.AccountId)
                            .AddAttribute("operation", 1.0, "character_select_menu")
                            .AddAttribute("name", 0.0, acct.Name)
                            .AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
                        AuthenticationHandler.SendAuthChallengeSuccessReply(client);
                    }
                }
            }
        }

        private static void ConnectClientToIngameCharacter(IRealmClient client, Account acct, RealmAccount realmAcc)
        {
            Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint) acct.AccountId)
                .AddAttribute("operation", 1.0, "connecting_to_already_connected_character")
                .AddAttribute("name", 0.0, acct.Name).AddAttribute("ip", 0.0, client.ClientAddress.ToString()).Write();
            realmAcc.ActiveCharacter.AddMessage((Action) (() =>
                Asda2LoginHandler.PreLoginCharacter(client, realmAcc.ActiveCharacter.UniqId, true)));
        }

        public static void SendAuthChallengeSuccessReply(IRealmClient client)
        {
            client.AuthAccount.OnLogin(client);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AuthorizeResponse))
            {
                packet.Write((short) 1);
                packet.WriteInt32(client.AuthAccount.AccountId);
                packet.WriteByte(1);
                client.Send(packet, false);
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChanelInfoResponse))
            {
                packet.WriteByte(244);
                packet.WriteInt16(2049);
                packet.WriteInt16(100);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt16(-1);
                client.Send(packet, false);
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ChanelInfoResponse))
            {
                packet.WriteByte(244);
                packet.WriteInt16(2049);
                packet.WriteInt16(100);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt64(-1L);
                packet.WriteInt16(-1);
                client.Send(packet, false);
            }
        }

        public static void SendAuthChallengeFailReply(IRealmClient client, AccountStatus error)
        {
            if (error == AccountStatus.AccountBanned)
            {
                AuthenticationHandler.SendDisconnectResponse(client, DisconnectStatus.AccountBanned);
            }
            else
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AuthorizeResponse))
                {
                    packet.Write((int) error);
                    packet.Write((short) 5);
                    packet.Write((byte) 0);
                    client.Send(packet, false);
                }
            }
        }

        [PacketHandler((RealmServerOpCode) 1028, IsGamePacket = false, RequiresLogin = false)]
        public static void ReturnToAuth(IRealmClient client, RealmPacketIn packet)
        {
            ServerApp<WCell.RealmServer.RealmServer>.Instance.UnregisterAccount(client.Account);
            client.Account = (RealmAccount) null;
            client.AuthAccount = (Account) null;
        }

        [PacketHandler(RealmServerOpCode.SelectChanelRequest, IsGamePacket = false, RequiresLogin = false)]
        public static void SelectChanelRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.AuthAccount == null)
                AuthenticationHandler.OnLoginError(client, AccountStatus.CloseClient);
            else
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                    (IMessage) new Message1<IRealmClient>(client,
                        new Action<IRealmClient>(AuthenticationHandler.SendCharacterNamesResponse)));
        }

        public static void SendCharacterNamesResponse(IRealmClient client)
        {
            AuthenticationHandler.SendCharacterInfoLSResponse(client);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterNames))
            {
                byte val1 = 0;
                byte val2 = 0;
                byte val3 = 0;
                byte val4 = 0;
                byte val5 = 0;
                byte val6 = 0;
                byte val7 = 0;
                byte val8 = 0;
                byte val9 = 0;
                byte val10 = 0;
                byte val11 = 0;
                byte val12 = 0;
                packet.WriteInt32(0);
                foreach (CharacterRecord character in client.AuthAccount.Characters)
                {
                    if (character.CharNum == (byte) 10)
                    {
                        val1 = character.HairStyle;
                        val4 = character.HairColor;
                        val10 = character.Face;
                        val7 = character.Zodiac;
                    }

                    if (character.CharNum == (byte) 11)
                    {
                        val2 = character.HairStyle;
                        val5 = character.HairColor;
                        val11 = character.Face;
                        val8 = character.Zodiac;
                    }

                    if (character.CharNum == (byte) 12)
                    {
                        val3 = character.HairStyle;
                        val6 = character.HairColor;
                        val12 = character.Face;
                        val9 = character.Zodiac;
                    }

                    packet.WriteByte(character.CharNum);
                    packet.WriteAsdaString(character.Name, 21, Locale.Start);
                    packet.WriteByte((byte) character.Gender);
                    packet.WriteByte(character.ProfessionLevel);
                    packet.WriteByte((byte) character.Class);
                    packet.WriteByte(character.Level);
                    packet.WriteInt64(0L);
                    packet.WriteInt32(character.Health);
                    packet.WriteInt16(character.Power);
                    packet.WriteInt32(character.Health);
                    packet.WriteInt16(character.Power);
                    packet.WriteInt16(character.BaseStrength);
                    packet.WriteInt16(character.BaseAgility);
                    packet.WriteInt16(character.BaseStamina);
                    packet.WriteInt16(character.BaseSpirit);
                    packet.WriteInt16(character.BaseIntellect);
                    packet.WriteInt16(10);
                    packet.WriteByte(0);
                }

                for (int index = 0; index < 3 - client.AuthAccount.Characters.Count; ++index)
                {
                    packet.WriteByte(0);
                    packet.WriteAsdaString("", 21, Locale.Start);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteInt64(0L);
                    packet.WriteInt32(0);
                    packet.WriteInt16(0);
                    packet.WriteInt32(0);
                    packet.WriteInt16(0);
                    packet.WriteInt16(0);
                    packet.WriteInt16(0);
                    packet.WriteInt16(0);
                    packet.WriteInt16(0);
                    packet.WriteInt16(0);
                    packet.WriteInt16(0);
                    packet.WriteByte(0);
                }

                packet.WriteByte(val1);
                packet.WriteByte(val4);
                packet.WriteByte(val10);
                packet.WriteByte(val2);
                packet.WriteByte(val5);
                packet.WriteByte(val11);
                packet.WriteByte(val3);
                packet.WriteByte(val6);
                packet.WriteByte(val12);
                packet.WriteByte(val7);
                packet.WriteByte(val8);
                packet.WriteByte(val9);
                for (int index = 0; index < 16; ++index)
                    packet.WriteByte(1);
                packet.WriteInt32(63);
                client.Send(packet, false);
            }

            AuthenticationHandler.SendShowCharactersViewResponse(client);
        }

        public static void SendShowCharactersViewResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ShowCharactersView))
            {
                packet.WriteByte(1);
                client.Send(packet, false);
            }
        }

        public static void SendCharacterInfoLSResponse(IRealmClient client)
        {
            if (!client.IsConnected || client.AuthAccount == null)
                return;
            foreach (CharacterRecord character in client.AuthAccount.Characters)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterInfos))
                {
                    packet.WriteInt32(0);
                    packet.WriteByte(character.CharNum);
                    packet.WriteByte(2);
                    for (int index = 0; index < 20; ++index)
                    {
                        bool flag = false;
                        foreach (Asda2ItemRecord asda2LoadedItem in (IEnumerable<Asda2ItemRecord>) character
                            .Asda2LoadedItems)
                        {
                            if ((int) asda2LoadedItem.Slot == index && asda2LoadedItem.InventoryType == (byte) 3)
                            {
                                packet.WriteInt32(asda2LoadedItem.ItemId);
                                packet.WriteInt16(asda2LoadedItem.Slot);
                                packet.WriteInt32(-1);
                                packet.WriteInt32(0);
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                        {
                            packet.WriteInt32(0);
                            packet.WriteInt16(0);
                            packet.WriteInt32(-1);
                            packet.WriteInt32(0);
                        }
                    }

                    client.Send(packet, false);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.Ping)]
        public static void PingRequest(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void SendDisconnectResponse(IRealmClient client, DisconnectStatus status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.Disconnect))
            {
                packet.WriteInt32(0);
                packet.WriteByte((byte) status);
                client.Send(packet, false);
            }
        }

        public delegate void LoginFailedHandler(IRealmClient client, AccountStatus error);
    }
}