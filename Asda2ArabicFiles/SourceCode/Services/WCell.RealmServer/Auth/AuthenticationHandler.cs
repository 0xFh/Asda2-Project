using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using NLog;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Auth.Firewall;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.Util.Threading;
using AccountMgr = WCell.RealmServer.Auth.Accounts.AccountMgr;

namespace WCell.RealmServer.Auth
{
    /// <summary>
    /// Class that handles all authentication of the client.
    /// </summary>
    public static class AuthenticationHandler
    {
        public delegate void LoginFailedHandler(IRealmClient client, AccountStatus error);

        /// <summary>
        /// Is called whenever a client failed to authenticate itself
        /// </summary>
        public static event LoginFailedHandler LoginFailed;

        private static Logger s_log = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// The time in milliseconds to let a client wait when failing to login
        /// </summary>
        public static int FailedLoginDelay = 200;

        private static readonly Dictionary<IPAddress, LoginFailInfo> failedLogins = new Dictionary<IPAddress, LoginFailInfo>();


        #region RealmServerOpCode.AuthorizeRequest
        [PacketHandler(RealmServerOpCode.AuthorizeRequest, IsGamePacket = false, RequiresLogin = false)]
        public static void AuthChallengeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.Account != null)
                return;
            packet.Position += 20;
            var login = packet.ReadAsdaString(32, Locale.En);
            //packet.Position += 31 - login.Length;
            if (client.Locale == Locale.Ru)
                packet.Position += 19;
            var password = packet.ReadAsdaString(32, Locale.En);

            s_log.Debug(string.Format("{0} starting login chalange", login));

            client.AccountName = login;
            client.Password = password;

            RealmServer.IOQueue.AddMessage(new Message1<IRealmClient>(client, AuthChallengeRequestCallback));
        }
        private static void AuthChallengeRequestCallback(IRealmClient client)
        {
            if (!client.IsConnected)
            {
                // Client disconnected in the meantime
                return;
            }

            if (BanMgr.IsBanned(client.ClientAddress))
            {
                OnLoginError(client, AccountStatus.CloseClient);
            }
            else
            {
                var acctQuery = new Action(() =>
                {
                    var acc = AccountMgr.GetAccount(client.AccountName);
                    //if(acc != null && DateTime.Now - acc.LastLogin < new TimeSpan(0,0,0,30))
                    //    return;
                    if (acc == null)
                    {
                        if (RealmServerConfiguration.AutocreateAccounts)
                            QueryAccountCallback(client, null);
                        else
                            OnLoginError(client, AccountStatus.WrongLoginOrPass);
                    }
                    else
                    {
                        if (acc.Password != client.Password)
                        {
                            if (client.ClientAddress != null)
                                Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint)acc.AccountId)
                                        .AddAttribute("operation", 1, "login_wrong_pass")
                                        .AddAttribute("name", 0, acc.Name)
                                        .AddAttribute("ip", 0, client.ClientAddress.ToString())
                                        .Write();
                            OnLoginError(client, AccountStatus.WrongLoginOrPass);
                        }
                        else
                        {
                            QueryAccountCallback(client, acc);
                        }
                    }
                });

                RealmServer.IOQueue.AddMessage(acctQuery);
            }
        }
        public static void OnLoginError(IRealmClient client, AccountStatus error)
        {
            OnLoginError(client, error, false);
        }
        public static void OnLoginError(IRealmClient client, AccountStatus error, bool silent)
        {
            if (!silent)
            {
                s_log.Debug("Client {0} failed to login: {1}", client, error);
            }

            LoginFailInfo failInfo;
            if (!failedLogins.TryGetValue(client.ClientAddress, out failInfo))
            {
                failedLogins.Add(client.ClientAddress, failInfo = new LoginFailInfo(DateTime.Now));
            }
            else
            {
                failInfo.LastAttempt = DateTime.Now;
                failInfo.Count++;
                // TODO: Ban, if trying too often?
            }

            // delay the reply
            ThreadPool.RegisterWaitForSingleObject(failInfo.Handle, (state, timedOut) =>
            {
                if (client.IsConnected)
                {
                    var evt = LoginFailed;
                    if (evt != null)
                    {
                        evt(client, error);
                    }

                    SendAuthChallengeFailReply(client, error);
                }
            }, null, FailedLoginDelay, true);
        }
        private static void QueryAccountCallback(IRealmClient client, Account acct)
        {

            if (client == null || !client.IsConnected)
            {
                return;
            }
            var accName = client.AccountName;
            var realmAcc = RealmServer.Instance.GetLoggedInAccount(accName);
            if (acct != null && acct.IsLogedOn)
            {
                Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint)acct.AccountId)
                                    .AddAttribute("operation", 1, "account_in_use")
                                    .AddAttribute("name", 0, acct.Name)
                                    .AddAttribute("ip", 0, client.ClientAddress.ToString())
                                    .Write();
                OnLoginError(client, AccountStatus.AccountInUse);
                return;
            }
            if (realmAcc != null && realmAcc.ActiveCharacter != null && realmAcc.Client != null && realmAcc.Client.IsConnected)
            {
                Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint)acct.AccountId)
                                       .AddAttribute("operation", 1, "account_in_use")
                                       .AddAttribute("name", 0, acct.Name)
                                    .AddAttribute("ip", 0, client.ClientAddress.ToString())
                                       .Write();
                OnLoginError(client, AccountStatus.AccountInUse);
                return;
            }


            if (acct == null)
            {
                // Account doesn't exist yet -> Check for auto creation
                if (RealmServerConfiguration.AutocreateAccounts)
                {
                    if (!AccountMgr.NameValidator(ref accName) || client.Password == null || client.Password.Length > 20)
                    {
                        OnLoginError(client, AccountStatus.WrongLoginOrPass);
                        return;
                    }
                    client.AuthAccount = AccountMgr.Instance.CreateAccount(accName, client.Password, "", RealmServerConfiguration.DefaultRole);
                    client.AuthAccount.Save();

                    //client.Account = new RealmAccount(accName,new AccountInfo(){ClientId = ClientId.Original,AccountId = client.AuthAccount.AccountId,EmailAddress = "",LastIP = client.ClientAddress.GetAddressBytes(),LastLogin = DateTime.Now,Locale = ClientLocale.English,RoleGroupName = "Player"});
                    SendAuthChallengeSuccessReply(client);
                }
                else
                {
                    OnLoginError(client, AccountStatus.WrongLoginOrPass);
                    return;
                }
                client.AuthAccount.IsLogedOn = true;
            }
            else
            {
                // check if Account may be used

                if (acct.CheckActive())
                {
                    client.AuthAccount = acct;
                    if (realmAcc == null)
                        SendAuthChallengeSuccessReply(client);
                }
                else
                {
                    Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint)acct.AccountId)
                                    .AddAttribute("operation", 1, "login_banned")
                                    .AddAttribute("name", 0, acct.Name)
                                    .AddAttribute("ip", 0, client.ClientAddress.ToString())
                                    .Write();
                    // Account has been deactivated
                    if (client.AuthAccount == null || client.AuthAccount.StatusUntil == null)
                    {
                        // temporarily suspended
                        OnLoginError(client, AccountStatus.WrongLoginOrPass);
                    }
                    else
                    {
                        // deactivated
                        OnLoginError(client, AccountStatus.WrongLoginOrPass);
                    }
                    return;
                }
            }
            if (realmAcc == null)
            {
                if (acct != null)
                    Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint)acct.AccountId)
                        .AddAttribute("operation", 1, "login_ok")
                        .AddAttribute("name", 0, acct.Name)
                        .AddAttribute("ip", 0, client.ClientAddress.ToString())
                        .Write();
                RealmAccount.InitializeAccount(client, client.AuthAccount.Name);
            }
            else if (acct != null)
            {
                if (realmAcc.Client != null)
                {
                    if (realmAcc.Client.ActiveCharacter != null)
                    {
                        realmAcc.Client.ActiveCharacter.SendInfoMsg("Some one loggin in to your account. Disconnecting.");
                    }
                    realmAcc.Client.Disconnect();
                }
                if (client.ClientAddress == null) return;
                realmAcc.LastIP = client.ClientAddress.GetAddressBytes();
                acct.LastIP = client.ClientAddress.GetAddressBytes();
                acct.Save();
                client.Account = realmAcc;
                if (realmAcc.ActiveCharacter != null)
                {
                    ConnectClientToIngameCharacter(client, acct, realmAcc);
                }
                else
                {
                    Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint)acct.AccountId)
                       .AddAttribute("operation", 1, "character_select_menu")
                       .AddAttribute("name", 0, acct.Name)
                       .AddAttribute("ip", 0, client.ClientAddress.ToString())
                       .Write();
                    SendAuthChallengeSuccessReply(client);
                }
            }
        }

        private static void ConnectClientToIngameCharacter(IRealmClient client, Account acct, RealmAccount realmAcc)
        {
            Log.Create(Log.Types.AccountOperations, LogSourceType.Account, (uint)acct.AccountId)
                .AddAttribute("operation", 1, "connecting_to_already_connected_character")
                .AddAttribute("name", 0, acct.Name)
                .AddAttribute("ip", 0, client.ClientAddress.ToString())
                .Write();
            realmAcc.ActiveCharacter.AddMessage(() =>
            {
                //realmAcc.ActiveCharacter.Logout(false);
                Asda2LoginHandler.PreLoginCharacter(client, realmAcc.ActiveCharacter.UniqId, true);
            });
        }

        public static void SendAuthChallengeSuccessReply(IRealmClient client)
        {
            client.AuthAccount.OnLogin(client);

            using (var packet = new RealmPacketOut(RealmServerOpCode.AuthorizeResponse))
            {
                packet.Write((Int16)AccountStatus.Success);//status
                packet.WriteInt32(client.AuthAccount.AccountId);//unknown
                packet.WriteByte(1);//avalible
                client.Send(packet, addEnd: false);
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.ChanelInfoResponse))//1013
            {
                packet.WriteByte(0xF4);// unknown
                packet.WriteInt16(2049);//unknown
                packet.WriteInt16(100);//chanel1
                packet.WriteInt16(-1);//chanel2
                packet.WriteInt16(-1);//chanel3
                packet.WriteInt16(-1);//chanel4
                packet.WriteInt16(-1);//chanel5
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt16(-1);//unknown
                client.Send(packet, addEnd: false);
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.ChanelInfoResponse))//1013
            {
                packet.WriteByte(0xF4);// unknown
                packet.WriteInt16(2049);//unknown
                packet.WriteInt16(100);//chanel1
                packet.WriteInt16(-1);//chanel2
                packet.WriteInt16(-1);//chanel3
                packet.WriteInt16(-1);//chanel4
                packet.WriteInt16(-1);//chanel5
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt64(-1);//unknown
                packet.WriteInt16(-1);//unknown
                client.Send(packet, addEnd: false);
            }
        }
        public static void SendAuthChallengeFailReply(IRealmClient client, AccountStatus error)
        {
            if (error == AccountStatus.AccountBanned)
            {
                SendDisconnectResponse(client, DisconnectStatus.AccountBanned);
            }
            else
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.AuthorizeResponse))
                {
                    packet.Write((int)error);
                    packet.Write((Int16)5);
                    packet.Write((byte)0x00);
                    client.Send(packet, addEnd: false);
                }
            }
        }
        #endregion


        [PacketHandler(RealmServerOpCode.SelectChanelRequest, IsGamePacket = false, RequiresLogin = false)]//1005
        public static void SelectChanelRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.AuthAccount == null)
            {
                OnLoginError(client, AccountStatus.CloseClient);
                return;
            }
            RealmServer.IOQueue.AddMessage(new Message1<IRealmClient>(client, SendCharacterNamesResponse));
        }
        public static void SendCharacterNamesResponse(IRealmClient client)
        {
            SendCharacterInfoLSResponse(client);
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterNames))//1006
            {
                byte hair1 = 0;
                byte hair2 = 0;
                byte hair3 = 0;
                byte color1 = 0;
                byte color2 = 0;
                byte color3 = 0;
                byte zodiac1 = 0;
                byte zodiac2 = 0;
                byte zodiac3 = 0;
                byte face1 = 0;
                byte face2 = 0;
                byte face3 = 0;
                packet.WriteInt32(0);//unknown
                foreach (var character in client.AuthAccount.Characters)
                {
                    if (character.CharNum == 10)
                    {
                        hair1 = character.HairStyle;
                        color1 = character.HairColor;
                        face1 = character.Face;
                        zodiac1 = character.Zodiac;
                    }
                    if (character.CharNum == 11)
                    {
                        hair2 = character.HairStyle;
                        color2 = character.HairColor;
                        face2 = character.Face;
                        zodiac2 = character.Zodiac;
                    }
                    if (character.CharNum == 12)
                    {
                        hair3 = character.HairStyle;
                        color3 = character.HairColor;
                        face3 = character.Face;
                        zodiac3 = character.Zodiac;
                    }
                    packet.WriteByte(character.CharNum);//default value : 0
                    packet.WriteAsdaString(character.Name, 21);//default value : "",21
                    packet.WriteByte((byte)character.Gender);//default value : 1
                    packet.WriteByte(character.ProfessionLevel);//value name : _
                    packet.WriteByte((byte)character.Class);//value name : _
                    packet.WriteByte(character.Level);//default value : 0
                    packet.WriteInt64(0);//value name : _
                    packet.WriteInt32(character.Health);//default value : 0
                    packet.WriteInt16(character.Power);//default value : 0
                    packet.WriteInt32(character.Health);//default value : 265
                    packet.WriteInt16(character.Power);//default value : 100
                    packet.WriteInt16(character.BaseStrength);//default value : 1
                    packet.WriteInt16(character.BaseAgility);//default value : 2
                    packet.WriteInt16(character.BaseStamina);//default value : 3
                    packet.WriteInt16(character.BaseSpirit);//default value : 4
                    packet.WriteInt16(character.BaseIntellect);//default value : 5
                    packet.WriteInt16(10);//default value : 6
                    packet.WriteByte(0);//value name : _
                }
                for (int i = 0; i < 3 - client.AuthAccount.Characters.Count; i++)
                {
                    packet.WriteByte(0);//default value : 0
                    packet.WriteAsdaString("", 21);//default value : "",21
                    packet.WriteByte(0);//default value : 1
                    packet.WriteByte(0);//value name : _
                    packet.WriteByte(0);//value name : _
                    packet.WriteByte(0);//default value : 0
                    packet.WriteInt64(0);//value name : _
                    packet.WriteInt32(0);//default value : 0
                    packet.WriteInt16(0);//default value : 0
                    packet.WriteInt32(0);//default value : 265
                    packet.WriteInt16(0);//default value : 100
                    packet.WriteInt16(0);//default value : 1
                    packet.WriteInt16(0);//default value : 2
                    packet.WriteInt16(0);//default value : 3
                    packet.WriteInt16(0);//default value : 4
                    packet.WriteInt16(0);//default value : 5
                    packet.WriteInt16(0);//default value : 6
                    packet.WriteByte(0);//value name : _
                }
                packet.WriteByte(hair1);//default value : 1
                packet.WriteByte(color1);//default value : 1
                packet.WriteByte(face1);//default value : 1
                packet.WriteByte(hair2);//default value : 1
                packet.WriteByte(color2);//default value : 1
                packet.WriteByte(face2);//default value : 1
                packet.WriteByte(hair3);//default value : 1
                packet.WriteByte(color3);//default value : 1
                packet.WriteByte(face3);//default value : 1
                packet.WriteByte(zodiac1);//default value : 1
                packet.WriteByte(zodiac2);//default value : 1
                packet.WriteByte(zodiac3);//default value : 1
                for (int i = 0; i < 16; i++)
                {
                    packet.WriteByte(1);
                }
                packet.WriteInt32(63);
                client.Send(packet, addEnd: false);
            }
            SendShowCharactersViewResponse(client);
        }
        public static void SendShowCharactersViewResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ShowCharactersView))//1025
            {
                packet.WriteByte(1);//viewNum
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendCharacterInfoLSResponse(IRealmClient client)
        {
            if (client.IsConnected == false || client.AuthAccount == null)
                return;
            foreach (var character in client.AuthAccount.Characters)
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterInfoLS)) //1007
                {
                    packet.WriteInt32(0); //default value : 0
                    packet.WriteByte(character.CharNum); //default value : 0
                    packet.WriteByte(2); //value name : _
                    for (int i = 0; i < 20; i++)
                    {
                        packet.WriteInt32(0); //default value : 0 id
                        packet.WriteInt16(0); //default value : 0 slot
                        packet.WriteInt32(-1); //value name : _
                        packet.WriteInt32(0); //value name : _
                    }
                    client.Send(packet, addEnd: false);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.Ping)]//1000
        public static void PingRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter == null)
                return;
            if (DateTime.Now.Subtract(client.ActiveCharacter.LastPing).TotalMilliseconds < CharacterFormulas.MinPingDelay)
            {
                client.ActiveCharacter.BadPingsCount++;
                if (client.ActiveCharacter.BadPingsCount > CharacterFormulas.MaxBadPings)
                {
                    if (client.ActiveCharacter.Role.IsStaff)
                    {
                        client.ActiveCharacter.SendInfoMsg("Good luck my lovely cheating GM");
                        client.ActiveCharacter.BadPingsCount = -100;
                    }
                    else 
                    {
                        if (client.Account.SetAccountActive(true, DateTime.MaxValue))
                        {
                            if (client.ActiveCharacter.IsInWorld)
                            {
                                client.ActiveCharacter.Kick("Cheating Killer", "DeadPlayer", 300);
                            }
                        }
                    }
                }

            }
            else
            {
                client.ActiveCharacter.BadPingsCount = 0;
                client.ActiveCharacter.LastPing = DateTime.Now;
            }
        }

        public static void SendDisconnectResponse(IRealmClient client, DisconnectStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.Disconnect))//4101
            {
                packet.WriteInt32(0);//value name : unk4 default value : 0Len : 4
                packet.WriteByte((byte)status);//{Message}default value : 0 Len : 1
                client.Send(packet);
            }
        }

    }
    public enum DisconnectStatus
    {
        EmptyValue = 20,// Unable to connect because parameter value is empty.
        CountryBlocked = 21, //Unable to connect due to the IP of blocked country.
        FraudUserIp = 22,// Unable to connect due to  Fraud User IP.
        InternalError = 24,// Authentication server error.
        UnlachingHasValue = 25,// Unlatching HashValue.
        NoUserInfo = 26,// Unable to find user info.
        WrongPassword = 27,// Password is incorrect.
        ServerOnMaintance = 28,// Server maintenance in progress.
        NotCbtUser = 29,// You are not a CBT tester.
        NotServiceArea = 30,// You are not located in the service area.
        ExceedMaxNuberOfConnectionToThisIp = 31,// You have exceeded the maximum number of IP Addresses allowed per User ID for today.
        U1 = 32,// You have exceeded the maximum number of IP Addresses allowed per User ID for today.
        WelcomeBack = 33,// Welcome back! It's been a while since your last log in. Please log into the GameCampus website to verify your account!
        U2 = 40,// Password is incorrect. Disconect type 40.
        AccountBanned = 103,// Restricted account.
        DeletedAccount = 104,//  Deleted account.
        U3 = 105,// Unable to connect due to  Fraud User IP.
        DataDoesnotExsist = 106,// Data Does Not Exist.
        Timeout = 107,// Time Out Error.
    }
}