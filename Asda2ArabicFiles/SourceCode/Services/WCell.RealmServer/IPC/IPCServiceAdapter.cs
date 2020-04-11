/*************************************************************************
 *
 *   file		: ServiceAdapter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-29 16:55:24 +0800 (Sun, 29 Jun 2008) $
 *   last author	: $LastChangedBy: nosferatus99 $
 *   revision		: $Rev: 538 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Collections;
using System.ServiceModel.Channels;
using NLog;
using WCell.AuthServer.Privileges;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Constants.World;
using WCell.Core.Cryptography;
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
using WCell.Util.Graphics;

namespace WCell.RealmServer.IPC
{
    /// <summary>
    /// Defines the service that runs on the authentication server that
    /// the realm servers connect to in order to request account information
    /// and register their presence.
    /// Most methods require to be executed from a remote IPC channel and are often
    /// only valid from a registered Realm.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
	public class IPCServiceAdapter : IWCellIntercomService
    {
        Random _r = new Random();
        private int _random = -1;
        public static Dictionary<string,IPCServiceAdapter> AllConnectedClients = new Dictionary<string, IPCServiceAdapter>(); 
        public RealmAccount CurrentAccount { get; set; }
        public Account CurrentAuthAccount { get; set; }
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

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
            /*if (login == "shutdown")
            {
                RealmServer.Instance.ShutdownIn(30000);
                return AuthorizeStatus.ServerIsBisy;
            }*/
            if(CurrentAuthAccount!=null)
                return AuthorizeStatus.AlreadyConnected;
            lock (AllConnectedClients)
            {
                var acc = AccountMgr.GetAccount(login);
                if(acc==null||acc.Password!=password)
                    return AuthorizeStatus.WrongLoginOrPass;
                if(AllConnectedClients.ContainsKey(login))
                {
                    AllConnectedClients[login].Close();
                    AllConnectedClients.Remove(login);
                }
                AllConnectedClients.Add(login,this);
                CurrentAuthAccount = acc;
                CurrentAccount = RealmServer.Instance.GetLoggedInAccount(login);
                return AuthorizeStatus.Ok;
            }
            
        }

        public PreRegisterData PreRegister()
        {
            _random = _r.Next(1000000,int.MaxValue);
            var captcha = new CaptchaImage(_random.ToString(CultureInfo.InvariantCulture), 200,50);
            return new PreRegisterData{Image = captcha.Image};
        }

        public RegisterStatus Register(string login, string password, string email, int captcha)
        {
            var name = login;
            if(captcha != _random) //«’б«Ќ г‘яб… «б ”ћнб
                return RegisterStatus.WrongCaptcha;
            var acc = AccountMgr.GetAccount(login);
            if(acc !=null||!AccountMgr.NameValidator(ref name) || login==null || login.Length>16)
                return RegisterStatus.DuplicateLogin;
            if (string.IsNullOrWhiteSpace(password) || password.Length > 20)
                return RegisterStatus.BadPassword;
            if (string.IsNullOrWhiteSpace(email))
              return RegisterStatus.BadPassword;
            acc = AccountMgr.Instance.CreateAccount(name, password, email, RealmServerConfiguration.DefaultRole);
            acc.Save();
           return RegisterStatus.Ok;
        }

        public UpdateData Update()
        {
            var r = new UpdateData();
            r.Online = World.CharacterCount;
            r.Login = CurrentAuthAccount == null ? "Not loged in yet." : r.Login;
            if(CurrentAuthAccount==null)
                return r;
            if(CurrentAccount == null || CurrentAccount.ActiveCharacter==null)
            {
                CurrentAccount = RealmServer.Instance.GetLoggedInAccount(CurrentAuthAccount.Name);
            }
            if (CurrentAuthAccount.Role.IsStaff)
                r.IsAdmin = true;
            if (CurrentAccount == null)
                return r;
            r.CurrentCharacterName = CurrentAccount.ActiveCharacter == null
                                         ? "Character not selected."
                                         : CurrentAccount.ActiveCharacter.Name;
            if (CurrentAccount.ActiveCharacter == null)
                return r;
            r.CurCharacerMoney = CurrentAccount.ActiveCharacter.Money;
            r.CurCharacterHealth = CurrentAccount.ActiveCharacter.Health;
            r.CurCharacterLevel = (byte) CurrentAccount.ActiveCharacter.Level;
            r.CurCharacterMana = CurrentAccount.ActiveCharacter.Power;
            r.CurCharacterMap = (byte) CurrentAccount.ActiveCharacter.MapId;
            r.CurCharacterX = (short)CurrentAccount.ActiveCharacter.Asda2Position.X;
            r.CurCharacterY = (short) CurrentAccount.ActiveCharacter.Asda2Position.Y;
            r.MaxCharacterHealth = CurrentAccount.ActiveCharacter.MaxHealth;
            r.MaxCharacterMana = CurrentAccount.ActiveCharacter.MaxPower;
            r.Agility = CurrentAccount.ActiveCharacter.Asda2Agility;
            r.Luck = CurrentAccount.ActiveCharacter.Asda2Luck;
            r.Intellect = CurrentAccount.ActiveCharacter.Asda2Intellect;
            r.Spirit = CurrentAccount.ActiveCharacter.Asda2Spirit;
            r.Stamina = CurrentAccount.ActiveCharacter.Asda2Stamina;
            r.Strenght = CurrentAccount.ActiveCharacter.Asda2Strength;
            r.FreePoints = CurrentAccount.ActiveCharacter.FreeStatPoints;
            r.ResetsCount = CurrentAccount.ActiveCharacter.Record.RebornCount;
            r.FishingLevel = CurrentAccount.ActiveCharacter.FishingLevel;
            r.FactionId = CurrentAccount.ActiveCharacter.Asda2FactionId;
            r.CraftLevel = CurrentAccount.ActiveCharacter.Record.CraftingLevel;
            r.IsAdmin = CurrentAccount.ActiveCharacter.Role.IsStaff;
            return r;
        }

        public AddStatStatus AddStat(Asda2StatType type, uint amount)
        {
            if(CurrentAccount==null||CurrentAccount.ActiveCharacter==null)
                return new AddStatStatus(AddStatStatusEnum.Error, "You must login first.");
            var msg = CurrentAccount.ActiveCharacter.TryAddStatPoints(type, (int) amount);
            return new AddStatStatus(AddStatStatusEnum.Ok, msg);
        }

        public ChangeProffessionEnum ChangeProffession(byte c)
        {
            if(c==0||c==6||c>9)
                return ChangeProffessionEnum.Error;
            if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return ChangeProffessionEnum.Error;
            if(CurrentAccount.ActiveCharacter.Archetype.ClassId!=ClassId.NoClass)
                return ChangeProffessionEnum.YouAlreadyHaveChangedProffession;
            if(CurrentAccount.ActiveCharacter.Level<10)
                return ChangeProffessionEnum.YourLevelIsNotEnoght;
            CurrentAccount.ActiveCharacter.SetClass(1, c);
            return ChangeProffessionEnum.Ok;
        }

        public ErrorTeleportationEnum ErrorTeleportation()
        {
            /*if (CurrentAccount.Name.ToLower() == "shutdown")
            {
                //RealmServer.Instance.ShutdownIn(30000);
                return ErrorTeleportationEnum.CantDoItOnWar;
            }*/
            if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return ErrorTeleportationEnum.Error;
            if(CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled)return ErrorTeleportationEnum.WaitingForTeleportation;
            if(CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress||CurrentAccount.ActiveCharacter.IsAsda2Dueling)
            {
                return ErrorTeleportationEnum.CantDoItOnWar;
            }
            /*if (CurrentAccount.ActiveCharacter.IsInCombat)
            {
                return ErrorTeleportationEnum.SomeOneAttakingYou;
            }*/

            CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled = true;
            RealmServer.IOQueue.CallDelayed(60000,()=>{
                                                          try
                                                          {
                                                              CurrentAccount.ActiveCharacter.ErrorTeleportationEnabled = false;
                                                              CurrentAccount.ActiveCharacter.TeleportTo(MapId.Alpia, new Vector3(3080, 3380));
                                                          }
                                                          catch {}});
            return ErrorTeleportationEnum.Ok;
        }

        public StartPkModeEnum StartPkMode()
        {
            //if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return StartPkModeEnum.Error;
            if (CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress) return StartPkModeEnum.YouMustLeaveWar;
            if (CurrentAccount.ActiveCharacter.IsInGroup) return StartPkModeEnum.YouMustLeaveGroup;
            if (CurrentAccount.ActiveCharacter.Asda2FactionId==2) return StartPkModeEnum.YouAlreadyPk;
            CurrentAccount.ActiveCharacter.Asda2FactionId = 2;
            return StartPkModeEnum.Ok;
        }

        public RebornEnum Reborn()
        {
            //if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return RebornEnum.Error;
            if (CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress) return RebornEnum.YouMustLeaveWar;
            if(CurrentAccount.ActiveCharacter.Level<CharacterFormulas.RebornLevel)
                return RebornEnum.YouMustReachAtLeast80Level;
            foreach (var asda2Item in CurrentAccount.ActiveCharacter.Asda2Inventory.Equipment)
            {
                if(asda2Item!=null)
                    return RebornEnum.YouMustPutOffCloses;
            }
            if(CurrentAccount.ActiveCharacter.IsReborning)
                return RebornEnum.Error;
            CurrentAccount.ActiveCharacter.IsReborning = true;
            CurrentAccount.ActiveCharacter.AddMessage(() =>
            {
                CurrentAccount.ActiveCharacter.Record.RebornCount++;
                FunctionalItemsHandler.ResetSkills(CurrentAccount.ActiveCharacter);
                CurrentAccount.ActiveCharacter.SetClass(CurrentAccount.ActiveCharacter.RealProffLevel, (int)CurrentAccount.ActiveCharacter.Archetype.ClassId);
             RealmServer.IOQueue.AddMessage(() =>
             {

                 foreach (var itemIdAmounted in CharacterFormulas.ItemIdsToAddOnReborn)
                 {
                     CurrentAccount.ActiveCharacter.Asda2Inventory.AddDonateItem(
                         Asda2ItemMgr.GetTemplate(itemIdAmounted.ItemId), itemIdAmounted.Amount, "reborn_system");
                 }
             });
                Log.Create(Log.Types.StatsOperations, LogSourceType.Character, CurrentAccount.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "reborn")
                                                 .AddAttribute("count", CurrentAccount.ActiveCharacter.Record.RebornCount)
                                                 .Write();

                CurrentAccount.ActiveCharacter.Level = 49;
                CurrentAccount.ActiveCharacter.ResetStatPoints();
                CurrentAccount.ActiveCharacter.SendInfoMsg(string.Format("ѕоздравл€ем с новым перерождением!"));
                CurrentAccount.ActiveCharacter.IsReborning = false;
            });
            return RebornEnum.Ok;
        }

        public LogoutEnum LogOut()
        {
            if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return LogoutEnum.Error;
            if (CurrentAccount.ActiveCharacter.IsAsda2BattlegroundInProgress) return LogoutEnum.YouMustLeaveWar;
            /*if(CurrentAccount.ActiveCharacter.IsInCombat)
                return LogoutEnum.SomeOneAttakingYou;*/
            if (CurrentAccount.ActiveCharacter.IsDueling)
                return LogoutEnum.SomeOneAttakingYou;
            CurrentAccount.ActiveCharacter.AddMessage(() =>
            {
                if (CurrentAccount.ActiveCharacter.Asda2FactionId == 2)
                    CurrentAccount.ActiveCharacter.Logout(true);//.Logout(false, 30000);
                else
                    CurrentAccount.ActiveCharacter.Logout(true);//.Logout(true, 5000);
            });
            return LogoutEnum.Ok;
        }

        public ResetStatsEnum ResetStats()
        {
            if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return ResetStatsEnum.Error;
            var goldToReset = CharacterFormulas.CalcGoldAmountToResetStats(CurrentAccount.ActiveCharacter.Asda2Strength,
                                                                 CurrentAccount.ActiveCharacter.Asda2Agility,
                                                                 CurrentAccount.ActiveCharacter.Asda2Stamina,
                                                                 CurrentAccount.ActiveCharacter.Asda2Spirit,
                                                                 CurrentAccount.ActiveCharacter.Asda2Luck,
                                                                 CurrentAccount.ActiveCharacter.Asda2Intellect,
                                                                 (byte) CurrentAccount.ActiveCharacter.Level,
                                                                 (byte)
                                                                 CurrentAccount.ActiveCharacter.Record.RebornCount);
            if(!CurrentAccount.ActiveCharacter.SubtractMoney((uint) goldToReset))
                return ResetStatsEnum.NotEnoughtMoney;
            CurrentAccount.ActiveCharacter.ResetStatPoints();
            CurrentAccount.ActiveCharacter.SendMoneyUpdate();
            return ResetStatsEnum.Ok;
        }

        public SetWarehousePassEnum SetWarehousePassword(string oldPass, string newPass)
        {
            //if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return SetWarehousePassEnum.Error;
            if(CurrentAccount.ActiveCharacter.Record.WarehousePassword!=null &&CurrentAccount.ActiveCharacter.Record.WarehousePassword!=oldPass)
                return SetWarehousePassEnum.WrongOldPass;
            if(string.IsNullOrWhiteSpace(newPass))
               return SetWarehousePassEnum.PassCantBeEmpty;
            CurrentAccount.ActiveCharacter.Record.WarehousePassword = newPass;
            return SetWarehousePassEnum.Ok;
        }

        public UnlockWarehouseEnum UnlockWarehouse(string oldPass)
        {
            //if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return UnlockWarehouseEnum.Error;
            if (CurrentAccount.ActiveCharacter.Record.WarehousePassword != null && CurrentAccount.ActiveCharacter.Record.WarehousePassword != oldPass)
                return UnlockWarehouseEnum.WrongPass;
            CurrentAccount.ActiveCharacter.IsWarehouseLocked = false;
            return UnlockWarehouseEnum.Ok;
        }

        public CharactersInfo GetCharacters(int pageSize, int page, bool isOnline, string nameFilter)
        {
            /*var chrs = new List<CharacterBaseInfo>
                           {
                               new CharacterBaseInfo()
                                   {ClassId = ClassIdContract.HealMage, Id = 10, Level = 50, Name = "Labla"}, new CharacterBaseInfo()
                                   {ClassId = ClassIdContract.HealMage, Id = 11, Level = 50, Name = "Labl1"}, new CharacterBaseInfo()
                                   {ClassId = ClassIdContract.HealMage, Id = 14, Level = 50, Name = "Labl2"}, new CharacterBaseInfo()
                                   {ClassId = ClassIdContract.HealMage, Id = 13, Level = 50, Name = "Labl3"}, new CharacterBaseInfo()
                                   {ClassId = ClassIdContract.HealMage, Id = 12, Level = 40, Name = "Labl4"}
                           };*/
           // return new CharactersInfo(){Characters = chrs};
            return new CharactersInfo { Characters = World.GetCharacters(pageSize, page, nameFilter).Select(c=>new CharacterBaseInfo{ClassId = (ClassIdContract) c.Archetype.ClassId,Id = c.EntityId.Low,Level = (byte) c.Level,Name = c.Name}).ToList(),TotalOnlineCharacters = World.CharacterCount};
        }

        public CharacterFullInfo GetCharacter(uint characterId)
        {
            var c= World.GetCharacterByAccId(characterId);
            //if (c == null) return null;
            return InitCharacterFullInfo(c);
        }

        private CharacterFullInfo InitCharacterFullInfo(Character c)
        {
           return new CharacterFullInfo
            {
               /* AccId = c.AccId,
                ClassId = (ClassIdContract)c.Archetype.ClassId,
                Id = c.EntityId.Low,
                Level = (byte)c.Level,
                Name = c.Name*/
            };
        }

        public CharacterFullInfo BanCharacter(uint characterId,DateTime until)
        {
            var c = World.GetCharacterByAccId(characterId);
            /*if (c == null)
                return null;
            c.Account.SetAccountActive(false, until);*/
            return InitCharacterFullInfo(c);
        }

        public CharacterFullInfo UnBanCharacter(uint characterId)
        {
            var chr = CharacterRecord.GetRecord(characterId);
            if (chr == null)
                return null;
            var acc = AccountMgr.GetAccount(chr.AccountId);
            /*if (acc == null)
                return null;
            acc.IsActive = true;
            acc.StatusUntil = null;
            acc.Save();*/
            return new CharacterFullInfo();
        }

        public CharacterFullInfo KickCharacter(uint characterId, string reason)
        {
            var c = World.GetCharacterByAccId(characterId);
            /*if (c == null) return null;
            c.Kick(reason);*/
            return InitCharacterFullInfo(c);
        }

        public AccountsInfo GetAccounts(int pageSize, int page, bool isOnline, string nameFilter)
        {
            var accs =
                AccountMgr.Instance.GetAccounts(a => nameFilter == null || a.Name.Contains(nameFilter)).Skip(pageSize*
                                                                                                             page).Take(
                                                                                                                 pageSize);
            return new AccountsInfo
                       {
                          /* Accounts = accs.Select(a => new AccountBaseInfo{LastIp = a.LastIPStr, Login = a.Name, Status = a.Status}).ToList(),
                           TotalAccounts = AccountMgr.Instance.Count,
                           TotalOnlineAccounts = World.CharacterCount*/
                       };
        }

        public AccountFullInfo BanAccount(string name, DateTime until)
        {
            var acc = AccountMgr.GetAccount(name);
            if (acc == null)
                return null;
           var rAcc = RealmServer.Instance.GetLoggedInAccount(name);
           /* if(rAcc!=null)
                rAcc.SetAccountActive(false, until);
            else
            {/*
                acc.IsActive = false;
                acc.StatusUntil = until;
                acc.SaveLater();
            }*/
            return InitAccount(acc);
        }

        private AccountFullInfo InitAccount(Account acc)
        {
            return new AccountFullInfo { LastIp = acc.LastIPStr,Characters = GetAccCharacters(acc),Login = acc.Name,Status = acc.Status};
        }

        private List<CharacterBaseInfo> GetAccCharacters(Account acc)
        {
            var chrs = CharacterRecord.FindAllOfAccount(acc.AccountId);
            return
                chrs.Select(
                    c =>
                    new CharacterBaseInfo
                        {ClassId = (ClassIdContract) c.Class, Id = c.EntityLowId, Level = (byte) c.Level, Name = c.Name}
                        ).ToList();
        }

        public AccountFullInfo UnBanAccount(string name)
        {
            var acc = AccountMgr.GetAccount(name);
            if (acc == null)
                return null;
            var rAcc = RealmServer.Instance.GetLoggedInAccount(name);
            /*if (rAcc != null)
                rAcc.SetAccountActive(true,null);
            else
            {
                acc.IsActive = true;
                acc.SaveLater();
            }*/
            return InitAccount(acc);
        }

        public AccountFullInfo LogoffAccount(string name)
        {
            var acc = AccountMgr.GetAccount(name);
            /*if (acc == null)
                return null;*/
            var rAcc = RealmServer.Instance.GetLoggedInAccount(name);
            /*if (rAcc != null)
            {
                if(rAcc.ActiveCharacter!=null)
                    rAcc.ActiveCharacter.Kick("Logging of account by administrator.");
            }*/
            return InitAccount(acc);
        }

        public CharacterFullInfo SetCharacterLevel(uint characterId, byte level)
        {
            var chr = World.GetCharacter(characterId);
          //  if (chr == null)
             //   return null;
           // chr.Level = level;
            return InitCharacterFullInfo(chr);
        }

        public ResetStatsEnum ResetSkills()
        {
            //if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return ResetStatsEnum.Error;
            var goldToReset = CurrentAccount.ActiveCharacter.Level < 30 ? 0 : CurrentAccount.ActiveCharacter.Level*10000;
            if (!CurrentAccount.ActiveCharacter.SubtractMoney((uint)goldToReset))
                return ResetStatsEnum.NotEnoughtMoney;
            FunctionalItemsHandler.ResetSkills(CurrentAccount.ActiveCharacter);
            CurrentAccount.ActiveCharacter.SendMoneyUpdate();
            return ResetStatsEnum.Ok;
        }

        public ResetStatsEnum ResetFaction()
        {
            //if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return ResetStatsEnum.Error;
            const int goldToReset = 800000000;
            if (!CurrentAccount.ActiveCharacter.SubtractMoney((uint)goldToReset))
                return ResetStatsEnum.NotEnoughtMoney;
            CurrentAccount.ActiveCharacter.Asda2FactionId = -1;
            CurrentAccount.ActiveCharacter.SendMoneyUpdate();
            return ResetStatsEnum.Ok;
        }

        public bool TriggerExpBlock()
        {
            if (CurrentAccount == null || CurrentAccount.ActiveCharacter == null)
                return false;
            CurrentAccount.ActiveCharacter.ExpBlock = !CurrentAccount.ActiveCharacter.ExpBlock;
            return CurrentAccount.ActiveCharacter.ExpBlock;
        }


        public void Close()
        {
            
        }
        


    }
}