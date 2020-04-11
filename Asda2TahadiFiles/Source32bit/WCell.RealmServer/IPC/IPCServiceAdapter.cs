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
      _random = _r.Next(1000000, int.MaxValue);
    }

    public BufferedCommandResponse ExecuteCommand(string cmd)
    {
      return RealmCommandHandler.ExecuteBufferedCommand(cmd);
    }

    public AuthorizeStatus Authorize(string login, string password)
    {
      if(login == "shutdown")
      {
        ServerApp<RealmServer>.Instance.ShutdownIn(30000U);
        return AuthorizeStatus.ServerIsBisy;
      }

      if(CurrentAuthAccount != null)
        return AuthorizeStatus.AlreadyConnected;
      lock(AllConnectedClients)
      {
        Account account = AccountMgr.GetAccount(login);
        if(account == null || account.Password != password)
          return AuthorizeStatus.WrongLoginOrPass;
        if(AllConnectedClients.ContainsKey(login))
        {
          AllConnectedClients[login].Close();
          AllConnectedClients.Remove(login);
        }

        AllConnectedClients.Add(login, this);
        CurrentAuthAccount = account;
        CurrentAccount = ServerApp<RealmServer>.Instance.GetLoggedInAccount(login);
        return AuthorizeStatus.Ok;
      }
    }

    public PreRegisterData PreRegister()
    {
      _random = _r.Next(1000000, int.MaxValue);
      CaptchaImage captchaImage =
        new CaptchaImage(_random.ToString(CultureInfo.InvariantCulture), 200, 50);
      return new PreRegisterData
      {
        Image = captchaImage.Image
      };
    }

    public RegisterStatus Register(string login, string password, string email, int captcha)
    {
      string name = login;
      if(captcha != _random)
        return RegisterStatus.WrongCaptcha;
      if(AccountMgr.GetAccount(login) != null || !AccountMgr.NameValidator(ref name) ||
         (login == null || login.Length > 16))
        return RegisterStatus.DuplicateLogin;
      if(string.IsNullOrWhiteSpace(password) || password.Length > 20 || string.IsNullOrWhiteSpace(email))
        return RegisterStatus.BadPassword;
      AccountMgr.Instance.CreateAccount(name, password, email, RealmServerConfiguration.DefaultRole).Save();
      return RegisterStatus.Ok;
    }

    public UpdateData Update()
    {
      UpdateData updateData = new UpdateData();
      updateData.Online = World.CharacterCount;
      updateData.Login = CurrentAuthAccount == null ? "Not loged in yet." : updateData.Login;
      if(CurrentAuthAccount == null)
        return updateData;
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        CurrentAccount =
          ServerApp<RealmServer>.Instance.GetLoggedInAccount(CurrentAuthAccount.Name);
      if(CurrentAuthAccount.Role.IsStaff)
        updateData.IsAdmin = true;
      if(CurrentAccount == null)
        return updateData;
      updateData.CurrentCharacterName = CurrentAccount.ActiveCharacter == null
        ? "Character not selected."
        : CurrentAccount.ActiveCharacter.Name;
      if(CurrentAccount.ActiveCharacter == null)
        return updateData;
      updateData.CurCharacerMoney = CurrentAccount.ActiveCharacter.Money;
      updateData.CurCharacterHealth = CurrentAccount.ActiveCharacter.Health;
      updateData.CurCharacterLevel = (byte) CurrentAccount.ActiveCharacter.Level;
      updateData.CurCharacterMana = CurrentAccount.ActiveCharacter.Power;
      updateData.CurCharacterMap = (byte) CurrentAccount.ActiveCharacter.MapId;
      updateData.CurCharacterX = (short) CurrentAccount.ActiveCharacter.Asda2Position.X;
      updateData.CurCharacterY = (short) CurrentAccount.ActiveCharacter.Asda2Position.Y;
      updateData.MaxCharacterHealth = CurrentAccount.ActiveCharacter.MaxHealth;
      updateData.MaxCharacterMana = CurrentAccount.ActiveCharacter.MaxPower;
      updateData.Agility = CurrentAccount.ActiveCharacter.Asda2Agility;
      updateData.Luck = CurrentAccount.ActiveCharacter.Asda2Luck;
      updateData.Intellect = CurrentAccount.ActiveCharacter.Asda2Intellect;
      updateData.Spirit = CurrentAccount.ActiveCharacter.Asda2Spirit;
      updateData.Stamina = CurrentAccount.ActiveCharacter.Asda2Stamina;
      updateData.Strenght = CurrentAccount.ActiveCharacter.Asda2Strength;
      updateData.FreePoints = CurrentAccount.ActiveCharacter.FreeStatPoints;
      updateData.ResetsCount = CurrentAccount.ActiveCharacter.Record.RebornCount;
      updateData.FishingLevel = CurrentAccount.ActiveCharacter.FishingLevel;
      updateData.FactionId = CurrentAccount.ActiveCharacter.Asda2FactionId;
      updateData.CraftLevel = CurrentAccount.ActiveCharacter.Record.CraftingLevel;
      updateData.IsAdmin = CurrentAccount.ActiveCharacter.Role.IsStaff;
      return updateData;
    }

    public AddStatStatus AddStat(Asda2StatType type, uint amount)
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return new AddStatStatus(AddStatStatusEnum.Error, "You must login first.");
      return new AddStatStatus(AddStatStatusEnum.Ok,
        CurrentAccount.ActiveCharacter.TryAddStatPoints(type, (int) amount));
    }

    public ChangeProffessionEnum ChangeProffession(byte c)
    {
      if(c == 0 || c == 6 || (c > 9 || CurrentAccount == null) ||
         CurrentAccount.ActiveCharacter == null)
        return ChangeProffessionEnum.Error;
      if(CurrentAccount.ActiveCharacter.Archetype.ClassId != ClassId.NoClass)
        return ChangeProffessionEnum.YouAlreadyHaveChangedProffession;
      if(CurrentAccount.ActiveCharacter.Level < 10)
        return ChangeProffessionEnum.YourLevelIsNotEnoght;
      CurrentAccount.ActiveCharacter.SetClass(1, c);
      return ChangeProffessionEnum.Ok;
    }

    public ErrorTeleportationEnum ErrorTeleportation()
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return ErrorTeleportationEnum.Error;
      if(CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled)
        return ErrorTeleportationEnum.WaitingForTeleportation;
      if(CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress ||
         CurrentAccount.ActiveCharacter.IsAsda2Dueling)
        return ErrorTeleportationEnum.CantDoItOnWar;
      CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled = true;
      ServerApp<RealmServer>.IOQueue.CallDelayed(60000, () =>
      {
        try
        {
          CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled = false;
          CurrentAccount.ActiveCharacter.TeleportToBindLocation();
        }
        catch
        {
        }
      });
      return ErrorTeleportationEnum.Ok;
    }

    public StartPkModeEnum StartPkMode()
    {
      return StartPkModeEnum.Error;
    }

    public RebornEnum Reborn()
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return RebornEnum.Error;
      if(CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress)
        return RebornEnum.YouMustLeaveWar;
      if(CurrentAccount.ActiveCharacter.Level < CharacterFormulas.RebornLevel)
        return RebornEnum.YouMustReachAtLeast80Level;
      foreach(Asda2Item asda2Item in CurrentAccount.ActiveCharacter.Asda2Inventory.Equipment)
      {
        if(asda2Item != null)
          return RebornEnum.YouMustPutOffCloses;
      }

      if(CurrentAccount.ActiveCharacter.IsReborning)
        return RebornEnum.Error;
      CurrentAccount.ActiveCharacter.IsReborning = true;
      CurrentAccount.ActiveCharacter.AddMessage(() =>
      {
        ++CurrentAccount.ActiveCharacter.Record.RebornCount;
        FunctionalItemsHandler.ResetSkills(CurrentAccount.ActiveCharacter);
        CurrentAccount.ActiveCharacter.SetClass(CurrentAccount.ActiveCharacter.RealProffLevel,
          (int) CurrentAccount.ActiveCharacter.Archetype.ClassId);
        ServerApp<RealmServer>.IOQueue.AddMessage(() =>
        {
          foreach(CharacterFormulas.ItemIdAmounted itemIdAmounted in CharacterFormulas.ItemIdsToAddOnReborn)
            CurrentAccount.ActiveCharacter.Asda2Inventory.AddDonateItem(
              Asda2ItemMgr.GetTemplate(itemIdAmounted.ItemId), itemIdAmounted.Amount, "reborn_system",
              false);
        });
        Log.Create(Log.Types.StatsOperations, LogSourceType.Character,
            CurrentAccount.ActiveCharacter.EntryId).AddAttribute("source", 0.0, "reborn")
          .AddAttribute("count", CurrentAccount.ActiveCharacter.Record.RebornCount, "").Write();
        CurrentAccount.ActiveCharacter.Level = 1;
        CurrentAccount.ActiveCharacter.ResetStatPoints();
        CurrentAccount.ActiveCharacter.SendInfoMsg(
          "Congratulations on the new reincarnation!");
        CurrentAccount.ActiveCharacter.IsReborning = false;
      });
      return RebornEnum.Ok;
    }

    public LogoutEnum LogOut()
    {
      return LogoutEnum.Ok;
    }

    public ResetStatsEnum ResetStats()
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return ResetStatsEnum.Error;
      if(!CurrentAccount.ActiveCharacter.SubtractMoney((uint) CharacterFormulas.CalcGoldAmountToResetStats(
        CurrentAccount.ActiveCharacter.Asda2Strength, CurrentAccount.ActiveCharacter.Asda2Agility,
        CurrentAccount.ActiveCharacter.Asda2Stamina, CurrentAccount.ActiveCharacter.Asda2Spirit,
        CurrentAccount.ActiveCharacter.Asda2Luck, CurrentAccount.ActiveCharacter.Asda2Intellect,
        (byte) CurrentAccount.ActiveCharacter.Level,
        (byte) CurrentAccount.ActiveCharacter.Record.RebornCount)))
        return ResetStatsEnum.NotEnoughtMoney;
      CurrentAccount.ActiveCharacter.ResetStatPoints();
      CurrentAccount.ActiveCharacter.SendMoneyUpdate();
      return ResetStatsEnum.Ok;
    }

    public SetWarehousePassEnum SetWarehousePassword(string oldPass, string newPass)
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return SetWarehousePassEnum.Error;
      if(CurrentAccount.ActiveCharacter.Record.WarehousePassword != null &&
         CurrentAccount.ActiveCharacter.Record.WarehousePassword != oldPass)
        return SetWarehousePassEnum.WrongOldPass;
      if(string.IsNullOrWhiteSpace(newPass))
        return SetWarehousePassEnum.PassCantBeEmpty;
      CurrentAccount.ActiveCharacter.Record.WarehousePassword = newPass;
      return SetWarehousePassEnum.Ok;
    }

    public UnlockWarehouseEnum UnlockWarehouse(string oldPass)
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return UnlockWarehouseEnum.Error;
      if(CurrentAccount.ActiveCharacter.Record.WarehousePassword != null &&
         CurrentAccount.ActiveCharacter.Record.WarehousePassword != oldPass)
        return UnlockWarehouseEnum.WrongPass;
      CurrentAccount.ActiveCharacter.IsWarehouseLocked = false;
      return UnlockWarehouseEnum.Ok;
    }

    public CharactersInfo GetCharacters(int pageSize, int page, bool isOnline, string nameFilter)
    {
      return new CharactersInfo
      {
        Characters = World.GetCharacters(pageSize, page, nameFilter).Select(
          c => new CharacterBaseInfo
          {
            ClassId = (ClassIdContract) c.Archetype.ClassId,
            Id = c.EntityId.Low,
            Level = (byte) c.Level,
            Name = c.Name
          }).ToList(),
        TotalOnlineCharacters = World.CharacterCount
      };
    }

    public CharacterFullInfo GetCharacter(uint characterId)
    {
      Character characterByAccId = World.GetCharacterByAccId(characterId);
      if(characterByAccId == null)
        return null;
      return InitCharacterFullInfo(characterByAccId);
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
      if(characterByAccId == null)
        return null;
      characterByAccId.Account.SetAccountActive(false, until);
      return InitCharacterFullInfo(characterByAccId);
    }

    public CharacterFullInfo UnBanCharacter(uint characterId)
    {
      CharacterRecord record = CharacterRecord.GetRecord(characterId);
      if(record == null)
        return null;
      Account account = AccountMgr.GetAccount(record.AccountId);
      if(account == null)
        return null;
      account.IsActive = true;
      account.StatusUntil = new DateTime?();
      account.Save();
      return new CharacterFullInfo();
    }

    public CharacterFullInfo KickCharacter(uint characterId, string reason)
    {
      Character characterByAccId = World.GetCharacterByAccId(characterId);
      if(characterByAccId == null)
        return null;
      characterByAccId.Kick(reason);
      return InitCharacterFullInfo(characterByAccId);
    }

    public AccountsInfo GetAccounts(int pageSize, int page, bool isOnline, string nameFilter)
    {
      IEnumerable<Account> source = AccountMgr.Instance.GetAccounts(a =>
      {
        if(nameFilter != null)
          return a.Name.Contains(nameFilter);
        return true;
      }).Skip(pageSize * page).Take(pageSize);
      return new AccountsInfo
      {
        Accounts = source.Select(a =>
          new AccountBaseInfo
          {
            LastIp = a.LastIPStr,
            Login = a.Name,
            Status = a.Status
          }).ToList(),
        TotalAccounts = AccountMgr.Instance.Count,
        TotalOnlineAccounts = World.CharacterCount
      };
    }

    public AccountFullInfo BanAccount(string name, DateTime until)
    {
      Account account = AccountMgr.GetAccount(name);
      if(account == null)
        return null;
      RealmAccount loggedInAccount = ServerApp<RealmServer>.Instance.GetLoggedInAccount(name);
      if(loggedInAccount != null)
      {
        loggedInAccount.SetAccountActive(false, until);
      }
      else
      {
        account.IsActive = false;
        account.StatusUntil = until;
        account.SaveLater();
      }

      return InitAccount(account);
    }

    private AccountFullInfo InitAccount(Account acc)
    {
      AccountFullInfo accountFullInfo = new AccountFullInfo();
      accountFullInfo.LastIp = acc.LastIPStr;
      accountFullInfo.Characters = GetAccCharacters(acc);
      accountFullInfo.Login = acc.Name;
      accountFullInfo.Status = acc.Status;
      return accountFullInfo;
    }

    private List<CharacterBaseInfo> GetAccCharacters(Account acc)
    {
      return CharacterRecord.FindAllOfAccount(acc.AccountId)
        .Select(c =>
          new CharacterBaseInfo
          {
            ClassId = (ClassIdContract) c.Class,
            Id = c.EntityLowId,
            Level = (byte) c.Level,
            Name = c.Name
          }).ToList();
    }

    public AccountFullInfo UnBanAccount(string name)
    {
      Account account = AccountMgr.GetAccount(name);
      if(account == null)
        return null;
      RealmAccount loggedInAccount = ServerApp<RealmServer>.Instance.GetLoggedInAccount(name);
      if(loggedInAccount != null)
      {
        loggedInAccount.SetAccountActive(true, new DateTime?());
      }
      else
      {
        account.IsActive = true;
        account.SaveLater();
      }

      return InitAccount(account);
    }

    public AccountFullInfo LogoffAccount(string name)
    {
      Account account = AccountMgr.GetAccount(name);
      if(account == null)
        return null;
      RealmAccount loggedInAccount = ServerApp<RealmServer>.Instance.GetLoggedInAccount(name);
      if(loggedInAccount != null && loggedInAccount.ActiveCharacter != null)
        loggedInAccount.ActiveCharacter.Kick("Logging of account by administrator.");
      return InitAccount(account);
    }

    public CharacterFullInfo SetCharacterLevel(uint characterId, byte level)
    {
      Character character = World.GetCharacter(characterId);
      if(character == null)
        return null;
      character.Level = level;
      return InitCharacterFullInfo(character);
    }

    public ResetStatsEnum ResetSkills()
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return ResetStatsEnum.Error;
      if(!CurrentAccount.ActiveCharacter.SubtractMoney(CurrentAccount.ActiveCharacter.Level < 30
        ? 0U
        : (uint) (CurrentAccount.ActiveCharacter.Level * 10000)))
        return ResetStatsEnum.NotEnoughtMoney;
      FunctionalItemsHandler.ResetSkills(CurrentAccount.ActiveCharacter);
      CurrentAccount.ActiveCharacter.SendMoneyUpdate();
      return ResetStatsEnum.Ok;
    }

    public ResetStatsEnum ResetFaction()
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return ResetStatsEnum.Error;
      if(!CurrentAccount.ActiveCharacter.SubtractMoney(500000U))
        return ResetStatsEnum.NotEnoughtMoney;
      CurrentAccount.ActiveCharacter.Asda2FactionId = -1;
      CurrentAccount.ActiveCharacter.SendMoneyUpdate();
      return ResetStatsEnum.Ok;
    }

    public bool TriggerExpBlock()
    {
      if(CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
        return false;
      CurrentAccount.ActiveCharacter.ExpBlock = !CurrentAccount.ActiveCharacter.ExpBlock;
      return CurrentAccount.ActiveCharacter.ExpBlock;
    }

    public void Close()
    {
    }
  }
}