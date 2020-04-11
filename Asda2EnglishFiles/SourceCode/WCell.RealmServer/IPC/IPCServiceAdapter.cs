using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using WCell.Constants;
using WCell.Core;
using WCell.Intercommunication;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;

namespace WCell.RealmServer.IPC
{
    /// <summary>
    /// Defines the service that runs on the authentication server that
    /// the realm servers connect to in order to request account information
    /// and register their presence.
    /// Most methods require to be executed from a remote IPC channel and are often
    /// only valid from a registered Realm.
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerSession)]
    public class IPCServiceAdapter : IWCellIntercomService
    {
        public static Dictionary<string, IPCServiceAdapter> AllConnectedClients =
            new Dictionary<string, IPCServiceAdapter>();

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private Random _r = new Random();
        private int _random = -1;

        public RealmAccount CurrentAccount { get; set; }

        public Account CurrentAuthAccount { get; set; }

        public IPCServiceAdapter()
        {
            this._random = this._r.Next(1000000, int.MaxValue);
        }

        public BufferedCommandResponse ExecuteCommand(string cmd)
        {
            return RealmCommandHandler.ExecuteBufferedCommand(cmd);
        }

        public AuthorizeStatus Authorize(string login, string password)
        {
            if (login == "shutdown")
            {
                ServerApp<WCell.RealmServer.RealmServer>.Instance.ShutdownIn(30000U);
                return AuthorizeStatus.ServerIsBisy;
            }

            if (this.CurrentAuthAccount != null)
                return AuthorizeStatus.AlreadyConnected;
            lock (IPCServiceAdapter.AllConnectedClients)
            {
                Account account = AccountMgr.GetAccount(login);
                if (account == null || account.Password != password)
                    return AuthorizeStatus.WrongLoginOrPass;
                if (IPCServiceAdapter.AllConnectedClients.ContainsKey(login))
                {
                    IPCServiceAdapter.AllConnectedClients[login].Close();
                    IPCServiceAdapter.AllConnectedClients.Remove(login);
                }

                IPCServiceAdapter.AllConnectedClients.Add(login, this);
                this.CurrentAuthAccount = account;
                this.CurrentAccount = ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(login);
                return AuthorizeStatus.Ok;
            }
        }

        public PreRegisterData PreRegister()
        {
            this._random = this._r.Next(1000000, int.MaxValue);
            CaptchaImage captchaImage =
                new CaptchaImage(this._random.ToString((IFormatProvider) CultureInfo.InvariantCulture), 200, 50);
            return new PreRegisterData()
            {
                Image = captchaImage.Image
            };
        }

        public RegisterStatus Register(string login, string password, string email, int captcha)
        {
            string name = login;
            if (captcha != this._random)
                return RegisterStatus.WrongCaptcha;
            if (AccountMgr.GetAccount(login) != null || !AccountMgr.NameValidator(ref name) ||
                (login == null || login.Length > 16))
                return RegisterStatus.DuplicateLogin;
            if (string.IsNullOrWhiteSpace(password) || password.Length > 20 || string.IsNullOrWhiteSpace(email))
                return RegisterStatus.BadPassword;
            AccountMgr.Instance.CreateAccount(name, password, email, RealmServerConfiguration.DefaultRole).Save();
            return RegisterStatus.Ok;
        }

        public UpdateData Update()
        {
            UpdateData updateData = new UpdateData();
            updateData.Online = World.CharacterCount;
            updateData.Login = this.CurrentAuthAccount == null ? "Not loged in yet." : updateData.Login;
            if (this.CurrentAuthAccount == null)
                return updateData;
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                this.CurrentAccount =
                    ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(this.CurrentAuthAccount.Name);
            if (this.CurrentAuthAccount.Role.IsStaff)
                updateData.IsAdmin = true;
            if (this.CurrentAccount == null)
                return updateData;
            updateData.CurrentCharacterName = this.CurrentAccount.ActiveCharacter == null
                ? "Character not selected."
                : this.CurrentAccount.ActiveCharacter.Name;
            if (this.CurrentAccount.ActiveCharacter == null)
                return updateData;
            updateData.CurCharacerMoney = this.CurrentAccount.ActiveCharacter.Money;
            updateData.CurCharacterHealth = this.CurrentAccount.ActiveCharacter.Health;
            updateData.CurCharacterLevel = (byte) this.CurrentAccount.ActiveCharacter.Level;
            updateData.CurCharacterMana = this.CurrentAccount.ActiveCharacter.Power;
            updateData.CurCharacterMap = (byte) this.CurrentAccount.ActiveCharacter.MapId;
            updateData.CurCharacterX = (short) this.CurrentAccount.ActiveCharacter.Asda2Position.X;
            updateData.CurCharacterY = (short) this.CurrentAccount.ActiveCharacter.Asda2Position.Y;
            updateData.MaxCharacterHealth = this.CurrentAccount.ActiveCharacter.MaxHealth;
            updateData.MaxCharacterMana = this.CurrentAccount.ActiveCharacter.MaxPower;
            updateData.Agility = this.CurrentAccount.ActiveCharacter.Asda2Agility;
            updateData.Luck = this.CurrentAccount.ActiveCharacter.Asda2Luck;
            updateData.Intellect = this.CurrentAccount.ActiveCharacter.Asda2Intellect;
            updateData.Spirit = this.CurrentAccount.ActiveCharacter.Asda2Spirit;
            updateData.Stamina = this.CurrentAccount.ActiveCharacter.Asda2Stamina;
            updateData.Strenght = this.CurrentAccount.ActiveCharacter.Asda2Strength;
            updateData.FreePoints = this.CurrentAccount.ActiveCharacter.FreeStatPoints;
            updateData.ResetsCount = this.CurrentAccount.ActiveCharacter.Record.RebornCount;
            updateData.FishingLevel = this.CurrentAccount.ActiveCharacter.FishingLevel;
            updateData.FactionId = this.CurrentAccount.ActiveCharacter.Asda2FactionId;
            updateData.CraftLevel = this.CurrentAccount.ActiveCharacter.Record.CraftingLevel;
            updateData.IsAdmin = this.CurrentAccount.ActiveCharacter.Role.IsStaff;
            return updateData;
        }

        public AddStatStatus AddStat(Asda2StatType type, uint amount)
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return new AddStatStatus(AddStatStatusEnum.Error, "You must login first.");
            return new AddStatStatus(AddStatStatusEnum.Ok,
                this.CurrentAccount.ActiveCharacter.TryAddStatPoints(type, (int) amount));
        }

        public ChangeProffessionEnum ChangeProffession(byte c)
        {
            if (c == (byte) 0 || c == (byte) 6 || (c > (byte) 9 || this.CurrentAccount == null) ||
                this.CurrentAccount.ActiveCharacter == null)
                return ChangeProffessionEnum.Error;
            if (this.CurrentAccount.ActiveCharacter.Archetype.ClassId != ClassId.NoClass)
                return ChangeProffessionEnum.YouAlreadyHaveChangedProffession;
            if (this.CurrentAccount.ActiveCharacter.Level < 10)
                return ChangeProffessionEnum.YourLevelIsNotEnoght;
            this.CurrentAccount.ActiveCharacter.SetClass(1, (int) c);
            return ChangeProffessionEnum.Ok;
        }

        public ErrorTeleportationEnum ErrorTeleportation()
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return ErrorTeleportationEnum.Error;
            if (this.CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled)
                return ErrorTeleportationEnum.WaitingForTeleportation;
            if (this.CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress ||
                this.CurrentAccount.ActiveCharacter.IsAsda2Dueling)
                return ErrorTeleportationEnum.CantDoItOnWar;
            this.CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled = true;
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.CallDelayed(60000, (Action) (() =>
            {
                try
                {
                    this.CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled = false;
                    this.CurrentAccount.ActiveCharacter.TeleportToBindLocation();
                }
                catch
                {
                }
            }));
            return ErrorTeleportationEnum.Ok;
        }

        public StartPkModeEnum StartPkMode()
        {
            return StartPkModeEnum.Error;
        }

        public RebornEnum Reborn()
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return RebornEnum.Error;
            if (this.CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress)
                return RebornEnum.YouMustLeaveWar;
            if (this.CurrentAccount.ActiveCharacter.Level < CharacterFormulas.RebornLevel)
                return RebornEnum.YouMustReachAtLeast80Level;
            foreach (Asda2Item asda2Item in this.CurrentAccount.ActiveCharacter.Asda2Inventory.Equipment)
            {
                if (asda2Item != null)
                    return RebornEnum.YouMustPutOffCloses;
            }

            if (this.CurrentAccount.ActiveCharacter.IsReborning)
                return RebornEnum.Error;
            this.CurrentAccount.ActiveCharacter.IsReborning = true;
            this.CurrentAccount.ActiveCharacter.AddMessage((Action) (() =>
            {
                ++this.CurrentAccount.ActiveCharacter.Record.RebornCount;
                FunctionalItemsHandler.ResetSkills(this.CurrentAccount.ActiveCharacter);
                this.CurrentAccount.ActiveCharacter.SetClass((int) this.CurrentAccount.ActiveCharacter.RealProffLevel,
                    (int) this.CurrentAccount.ActiveCharacter.Archetype.ClassId);
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                {
                    foreach (CharacterFormulas.ItemIdAmounted itemIdAmounted in CharacterFormulas.ItemIdsToAddOnReborn)
                        this.CurrentAccount.ActiveCharacter.Asda2Inventory.AddDonateItem(
                            Asda2ItemMgr.GetTemplate(itemIdAmounted.ItemId), itemIdAmounted.Amount, "reborn_system",
                            false);
                }));
                Log.Create(Log.Types.StatsOperations, LogSourceType.Character,
                        this.CurrentAccount.ActiveCharacter.EntryId).AddAttribute("source", 0.0, "reborn")
                    .AddAttribute("count", (double) this.CurrentAccount.ActiveCharacter.Record.RebornCount, "").Write();
                this.CurrentAccount.ActiveCharacter.Level = 1;
                this.CurrentAccount.ActiveCharacter.ResetStatPoints();
                this.CurrentAccount.ActiveCharacter.SendInfoMsg(
                    string.Format("Congratulations on the new reincarnation!"));
                this.CurrentAccount.ActiveCharacter.IsReborning = false;
            }));
            return RebornEnum.Ok;
        }

        public LogoutEnum LogOut()
        {
            return LogoutEnum.Ok;
        }

        public ResetStatsEnum ResetStats()
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return ResetStatsEnum.Error;
            if (!this.CurrentAccount.ActiveCharacter.SubtractMoney((uint) CharacterFormulas.CalcGoldAmountToResetStats(
                this.CurrentAccount.ActiveCharacter.Asda2Strength, this.CurrentAccount.ActiveCharacter.Asda2Agility,
                this.CurrentAccount.ActiveCharacter.Asda2Stamina, this.CurrentAccount.ActiveCharacter.Asda2Spirit,
                this.CurrentAccount.ActiveCharacter.Asda2Luck, this.CurrentAccount.ActiveCharacter.Asda2Intellect,
                (byte) this.CurrentAccount.ActiveCharacter.Level,
                (int) (byte) this.CurrentAccount.ActiveCharacter.Record.RebornCount)))
                return ResetStatsEnum.NotEnoughtMoney;
            this.CurrentAccount.ActiveCharacter.ResetStatPoints();
            this.CurrentAccount.ActiveCharacter.SendMoneyUpdate();
            return ResetStatsEnum.Ok;
        }

        public SetWarehousePassEnum SetWarehousePassword(string oldPass, string newPass)
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return SetWarehousePassEnum.Error;
            if (this.CurrentAccount.ActiveCharacter.Record.WarehousePassword != null &&
                this.CurrentAccount.ActiveCharacter.Record.WarehousePassword != oldPass)
                return SetWarehousePassEnum.WrongOldPass;
            if (string.IsNullOrWhiteSpace(newPass))
                return SetWarehousePassEnum.PassCantBeEmpty;
            this.CurrentAccount.ActiveCharacter.Record.WarehousePassword = newPass;
            return SetWarehousePassEnum.Ok;
        }

        public UnlockWarehouseEnum UnlockWarehouse(string oldPass)
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return UnlockWarehouseEnum.Error;
            if (this.CurrentAccount.ActiveCharacter.Record.WarehousePassword != null &&
                this.CurrentAccount.ActiveCharacter.Record.WarehousePassword != oldPass)
                return UnlockWarehouseEnum.WrongPass;
            this.CurrentAccount.ActiveCharacter.IsWarehouseLocked = false;
            return UnlockWarehouseEnum.Ok;
        }

        public CharactersInfo GetCharacters(int pageSize, int page, bool isOnline, string nameFilter)
        {
            return new CharactersInfo()
            {
                Characters = World.GetCharacters(pageSize, page, nameFilter).Select<Character, CharacterBaseInfo>(
                    (Func<Character, CharacterBaseInfo>) (c => new CharacterBaseInfo()
                    {
                        ClassId = (ClassIdContract) c.Archetype.ClassId,
                        Id = c.EntityId.Low,
                        Level = (byte) c.Level,
                        Name = c.Name
                    })).ToList<CharacterBaseInfo>(),
                TotalOnlineCharacters = World.CharacterCount
            };
        }

        public CharacterFullInfo GetCharacter(uint characterId)
        {
            Character characterByAccId = World.GetCharacterByAccId(characterId);
            if (characterByAccId == null)
                return (CharacterFullInfo) null;
            return this.InitCharacterFullInfo(characterByAccId);
        }

        private CharacterFullInfo InitCharacterFullInfo(Character c)
        {
            CharacterFullInfo characterFullInfo = new CharacterFullInfo();
            characterFullInfo.AccId = c.AccId;
            characterFullInfo.ClassId = (ClassIdContract) c.Archetype.ClassId;
            characterFullInfo.Id = c.EntityId.Low;
            characterFullInfo.Level = (byte) c.Level;
            characterFullInfo.Name = c.Name;
            return characterFullInfo;
        }

        public CharacterFullInfo BanCharacter(uint characterId, DateTime until)
        {
            Character characterByAccId = World.GetCharacterByAccId(characterId);
            if (characterByAccId == null)
                return (CharacterFullInfo) null;
            characterByAccId.Account.SetAccountActive(false, new DateTime?(until));
            return this.InitCharacterFullInfo(characterByAccId);
        }

        public CharacterFullInfo UnBanCharacter(uint characterId)
        {
            CharacterRecord record = CharacterRecord.GetRecord(characterId);
            if (record == null)
                return (CharacterFullInfo) null;
            Account account = AccountMgr.GetAccount((long) record.AccountId);
            if (account == null)
                return (CharacterFullInfo) null;
            account.IsActive = true;
            account.StatusUntil = new DateTime?();
            account.Save();
            return new CharacterFullInfo();
        }

        public CharacterFullInfo KickCharacter(uint characterId, string reason)
        {
            Character characterByAccId = World.GetCharacterByAccId(characterId);
            if (characterByAccId == null)
                return (CharacterFullInfo) null;
            characterByAccId.Kick(reason);
            return this.InitCharacterFullInfo(characterByAccId);
        }

        public AccountsInfo GetAccounts(int pageSize, int page, bool isOnline, string nameFilter)
        {
            IEnumerable<Account> source = AccountMgr.Instance.GetAccounts((Predicate<Account>) (a =>
            {
                if (nameFilter != null)
                    return a.Name.Contains(nameFilter);
                return true;
            })).Skip<Account>(pageSize * page).Take<Account>(pageSize);
            return new AccountsInfo()
            {
                Accounts = source.Select<Account, AccountBaseInfo>((Func<Account, AccountBaseInfo>) (a =>
                    new AccountBaseInfo()
                    {
                        LastIp = a.LastIPStr,
                        Login = a.Name,
                        Status = a.Status
                    })).ToList<AccountBaseInfo>(),
                TotalAccounts = AccountMgr.Instance.Count,
                TotalOnlineAccounts = World.CharacterCount
            };
        }

        public AccountFullInfo BanAccount(string name, DateTime until)
        {
            Account account = AccountMgr.GetAccount(name);
            if (account == null)
                return (AccountFullInfo) null;
            RealmAccount loggedInAccount = ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(name);
            if (loggedInAccount != null)
            {
                loggedInAccount.SetAccountActive(false, new DateTime?(until));
            }
            else
            {
                account.IsActive = false;
                account.StatusUntil = new DateTime?(until);
                account.SaveLater();
            }

            return this.InitAccount(account);
        }

        private AccountFullInfo InitAccount(Account acc)
        {
            AccountFullInfo accountFullInfo = new AccountFullInfo();
            accountFullInfo.LastIp = acc.LastIPStr;
            accountFullInfo.Characters = this.GetAccCharacters(acc);
            accountFullInfo.Login = acc.Name;
            accountFullInfo.Status = acc.Status;
            return accountFullInfo;
        }

        private List<CharacterBaseInfo> GetAccCharacters(Account acc)
        {
            return ((IEnumerable<CharacterRecord>) CharacterRecord.FindAllOfAccount(acc.AccountId))
                .Select<CharacterRecord, CharacterBaseInfo>((Func<CharacterRecord, CharacterBaseInfo>) (c =>
                    new CharacterBaseInfo()
                    {
                        ClassId = (ClassIdContract) c.Class,
                        Id = c.EntityLowId,
                        Level = (byte) c.Level,
                        Name = c.Name
                    })).ToList<CharacterBaseInfo>();
        }

        public AccountFullInfo UnBanAccount(string name)
        {
            Account account = AccountMgr.GetAccount(name);
            if (account == null)
                return (AccountFullInfo) null;
            RealmAccount loggedInAccount = ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(name);
            if (loggedInAccount != null)
            {
                loggedInAccount.SetAccountActive(true, new DateTime?());
            }
            else
            {
                account.IsActive = true;
                account.SaveLater();
            }

            return this.InitAccount(account);
        }

        public AccountFullInfo LogoffAccount(string name)
        {
            Account account = AccountMgr.GetAccount(name);
            if (account == null)
                return (AccountFullInfo) null;
            RealmAccount loggedInAccount = ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(name);
            if (loggedInAccount != null && loggedInAccount.ActiveCharacter != null)
                loggedInAccount.ActiveCharacter.Kick("Logging of account by administrator.");
            return this.InitAccount(account);
        }

        public CharacterFullInfo SetCharacterLevel(uint characterId, byte level)
        {
            Character character = World.GetCharacter(characterId);
            if (character == null)
                return (CharacterFullInfo) null;
            character.Level = (int) level;
            return this.InitCharacterFullInfo(character);
        }

        public ResetStatsEnum ResetSkills()
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return ResetStatsEnum.Error;
            if (!this.CurrentAccount.ActiveCharacter.SubtractMoney(this.CurrentAccount.ActiveCharacter.Level < 30
                ? 0U
                : (uint) (this.CurrentAccount.ActiveCharacter.Level * 10000)))
                return ResetStatsEnum.NotEnoughtMoney;
            FunctionalItemsHandler.ResetSkills(this.CurrentAccount.ActiveCharacter);
            this.CurrentAccount.ActiveCharacter.SendMoneyUpdate();
            return ResetStatsEnum.Ok;
        }

        public ResetStatsEnum ResetFaction()
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return ResetStatsEnum.Error;
            if (!this.CurrentAccount.ActiveCharacter.SubtractMoney(500000U))
                return ResetStatsEnum.NotEnoughtMoney;
            this.CurrentAccount.ActiveCharacter.Asda2FactionId = (short) -1;
            this.CurrentAccount.ActiveCharacter.SendMoneyUpdate();
            return ResetStatsEnum.Ok;
        }

        public bool TriggerExpBlock()
        {
            if (this.CurrentAccount == null || this.CurrentAccount.ActiveCharacter == null)
                return false;
            this.CurrentAccount.ActiveCharacter.ExpBlock = !this.CurrentAccount.ActiveCharacter.ExpBlock;
            return this.CurrentAccount.ActiveCharacter.ExpBlock;
        }

        public void Close()
        {
        }
    }
}