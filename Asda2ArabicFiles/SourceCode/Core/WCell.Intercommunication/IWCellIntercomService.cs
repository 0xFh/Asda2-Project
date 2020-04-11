/*************************************************************************
 *
 *   file		: ClientServiceContract.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-02 18:37:54 +0800 (Wed, 02 Sep 2009) $
 
 *   revision		: $Rev: 1070 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.ServiceModel;
using WCell.Intercommunication.DataTypes;

namespace WCell.Intercommunication
{
    [GeneratedCode("System.ServiceModel", "3.0.0.0")]
    [ServiceContract(ConfigurationName = "IAuthenticationService")]
    public interface IWCellIntercomService
    {
        [OperationContract(Action = "http://www.wcell.org/IServerIPC/ExecuteCommand",
            ReplyAction = "http://www.wcell.org/IServerIPC/ExecuteCommandResponse")]
        BufferedCommandResponse ExecuteCommand(string cmd);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/Authorize",
            ReplyAction = "http://www.wcell.org/IServerIPC/AuthorizeResponse")]
        AuthorizeStatus Authorize(string login, string password);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/PreRegister",
            ReplyAction = "http://www.wcell.org/IServerIPC/PreRegisterResponse")]
        PreRegisterData PreRegister();

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/Register",
            ReplyAction = "http://www.wcell.org/IServerIPC/RegisterResponse")]
        RegisterStatus Register(string login, string password, string email, int captcha);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/Update",
            ReplyAction = "http://www.wcell.org/IServerIPC/UpdateResponse")]
        UpdateData Update();

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/AddStat",
            ReplyAction = "http://www.wcell.org/IServerIPC/AddStatResponse")]
        AddStatStatus AddStat(Asda2StatType type, uint amount);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/ChangeProffession",
            ReplyAction = "http://www.wcell.org/IServerIPC/ChangeProffessionResponse")]
        ChangeProffessionEnum ChangeProffession(byte c);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/ErrorTeleportation",
            ReplyAction = "http://www.wcell.org/IServerIPC/ErrorTeleportationResponse")]
        ErrorTeleportationEnum ErrorTeleportation();

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/StartPKMode",
            ReplyAction = "http://www.wcell.org/IServerIPC/StartPKModeResponse")]
        StartPkModeEnum StartPkMode();

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/Reborn",
            ReplyAction = "http://www.wcell.org/IServerIPC/RebornResponse")]
        RebornEnum Reborn();

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/LogOut",
            ReplyAction = "http://www.wcell.org/IServerIPC/LogOutResponse")]
        LogoutEnum LogOut();

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/ResetStats",
            ReplyAction = "http://www.wcell.org/IServerIPC/ResetStatsResponse")]
        ResetStatsEnum ResetStats();

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/SetWarehousePassword",
            ReplyAction = "http://www.wcell.org/IServerIPC/SetWarehousePasswordResponse")]
        SetWarehousePassEnum SetWarehousePassword(string oldPass, string newPass);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/UnlockWarehousePassword",
            ReplyAction = "http://www.wcell.org/IServerIPC/UnlockWarehouseResponse")]
        UnlockWarehouseEnum UnlockWarehouse(string oldPass);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/GetCharacters",
            ReplyAction = "http://www.wcell.org/IServerIPC/GetCharactersResponse")]
        CharactersInfo GetCharacters(int pageSize, int page, bool isOnline, string nameFilter);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/GetCharacter",
            ReplyAction = "http://www.wcell.org/IServerIPC/GetCharacterResponse")]
        CharacterFullInfo GetCharacter(uint characterId);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/BanCharacter",
            ReplyAction = "http://www.wcell.org/IServerIPC/BanCharacterResponse")]
        CharacterFullInfo BanCharacter(uint characterId,DateTime until);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/UnBanCharacter",
            ReplyAction = "http://www.wcell.org/IServerIPC/UnBanCharacterResponse")]
        CharacterFullInfo UnBanCharacter(uint characterId);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/KickCharacter",
            ReplyAction = "http://www.wcell.org/IServerIPC/KickCharacterResponse")]
        CharacterFullInfo KickCharacter(uint characterId,string reason);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/GetAccounts",
            ReplyAction = "http://www.wcell.org/IServerIPC/GetAccountsResponse")]
        AccountsInfo GetAccounts(int pageSize, int page, bool isOnline, string nameFilter);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/BanAccount",
            ReplyAction = "http://www.wcell.org/IServerIPC/BanAccountResponse")]
        AccountFullInfo BanAccount(string name, DateTime until);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/UnBanAccount",
            ReplyAction = "http://www.wcell.org/IServerIPC/UnBanAccountResponse")]
        AccountFullInfo UnBanAccount(string name);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/LogoffAccount",
            ReplyAction = "http://www.wcell.org/IServerIPC/LogoffAccountResponse")]
        AccountFullInfo LogoffAccount(string name);
        [OperationContract(Action = "http://www.wcell.org/IServerIPC/SetCharacterLevel",
            ReplyAction = "http://www.wcell.org/IServerIPC/SetCharacterLevelResponse")]
        CharacterFullInfo SetCharacterLevel(uint characterId, byte level);
        [OperationContract(Action = "http://www.wcell.org/IServerIPC/ResetSkills",
            ReplyAction = "http://www.wcell.org/IServerIPC/ResetSkillsResponse")]
        ResetStatsEnum ResetSkills();
        [OperationContract(Action = "http://www.wcell.org/IServerIPC/ResetFaction",
            ReplyAction = "http://www.wcell.org/IServerIPC/ResetFactionResponse")]
        ResetStatsEnum ResetFaction();
        [OperationContract(Action = "http://www.wcell.org/IServerIPC/TriggerExpBlock",
            ReplyAction = "http://www.wcell.org/IServerIPC/TriggerExpBlockResponse")]
        bool TriggerExpBlock();

    }

    [DataContract]
    public class CharacterBaseInfo
    {
        [DataMember]
        public uint Id;
        [DataMember]
        public string Name;
        [DataMember]
        public byte Level;
        [DataMember]
        public ClassIdContract ClassId;
        public string Info
        {
            get { return string.Format("[{0}] [{1}] [level:{2}] [{3}]",  Id, Name, Level, ClassId); }
        }
    }

    [DataContract]
    public enum ClassIdContract
    {
        [EnumMember] NoClass = 0,
        [EnumMember] OHS = 1,
        [EnumMember] Spear = 2,
        [EnumMember] THS = 3,
        [EnumMember] Crossbow = 4,
        [EnumMember] Bow = 5,
        [EnumMember] Balista = 6,
        [EnumMember] AtackMage = 7,
        [EnumMember] SupportMage = 8,
        [EnumMember] HealMage = 9,
    }

    [DataContract]
    public class CharacterFullInfo : CharacterBaseInfo
    {
        [DataMember]
        public uint AccId;
    }
    [DataContract]
    public class CharactersInfo
    {
        [DataMember]
        public int TotalCharacters;
        [DataMember]
        public int TotalOnlineCharacters;
        [DataMember]
        public List<CharacterBaseInfo> Characters;
    }
    [DataContract]
    public class AccountsInfo
    {
        [DataMember]
        public int TotalAccounts;
        [DataMember]
        public int TotalOnlineAccounts;
        [DataMember]
        public List<AccountBaseInfo> Accounts;
    }
    [DataContract]
    public class AccountFullInfo : AccountBaseInfo
    {
        [DataMember]
        public List<CharacterBaseInfo> Characters;
    }

    [DataContract]
    public class AccountBaseInfo
    {
        [DataMember]
        public string Login;
        [DataMember]
        public string LastIp;
        [DataMember]
        public string Status;
    }

    [DataContract]
    public enum UnlockWarehouseEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        WrongPass,
        [EnumMember]
        Error
    }
    [DataContract]
    public enum SetWarehousePassEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        PassCantBeEmpty,
        [EnumMember]
        WrongOldPass,
        [EnumMember]
        Error
    }
    [DataContract]
    public enum ResetStatsEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        NotEnoughtMoney,
        [EnumMember]
        Error
    }
    [DataContract]
    public enum LogoutEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        SomeOneAttakingYou,
        [EnumMember]
        YouMustLeaveWar,
        [EnumMember]
        Error
    }
    [DataContract]
    public enum RebornEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        YouMustReachAtLeast80Level,
        [EnumMember]
        YouMustLeaveWar,
        [EnumMember]
        YouMustPutOffCloses,
        [EnumMember]n,
        Error
    }
    [DataContract]
    public enum StartPkModeEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        YouAlreadyPk,
        [EnumMember]
        YouMustLeaveGroup,
        [EnumMember]
        YouMustLeaveClan,
        [EnumMember]
        YouMustLeaveWar,
        [EnumMember]
        Error
    }
    [DataContract]
    public enum ErrorTeleportationEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        WaitingForTeleportation,
        [EnumMember]
        SomeOneAttakingYou,
        [EnumMember]
        Error,
        [EnumMember]
        CantDoItOnWar
    }
    [DataContract]
    public enum ChangeProffessionEnum
    {
        [EnumMember]
        Ok,
        [EnumMember]
        YouAlreadyHaveChangedProffession,
        [EnumMember]
        YourLevelIsNotEnoght,
        [EnumMember]
        Error
    }
    [DataContract]
    public class AddStatStatus
    {
        [DataMember]
        public AddStatStatusEnum Status { get; set; }
        [DataMember]
        public string Message { get; set; }

        public AddStatStatus(AddStatStatusEnum status, string message)
        {
            Status = status;
            Message = message;
        }
    }
    public enum AddStatStatusEnum
    {
        [EnumMember]
        Ok,
         [EnumMember]
        Error
    }
    [DataContract]
    public enum Asda2StatType
    {
       [EnumMember]
        Strength,
         [EnumMember]
        Dexterity,
        [EnumMember]
        Stamina,
        [EnumMember]
        Luck,
        [EnumMember]
        Intelect,
        [EnumMember]
        Spirit
    }
    [DataContract]
    public class UpdateData
    {
        [DataMember]
        public int Online;
        [DataMember]
        public string Login;
        [DataMember]
    public string CurrentCharacterName;
    [DataMember]
    public int MaxCharacterHealth;
    [DataMember]
    public int CurCharacterHealth;
    [DataMember]
    public int MaxCharacterMana;
    [DataMember]
    public int CurCharacterMana;
    [DataMember]
    public byte CurCharacterLevel;
    [DataMember]
    public uint CurCharacerMoney;
    [DataMember]
    public byte CurCharacterMap;
    [DataMember]
    public short CurCharacterX;
    [DataMember]
    public short CurCharacterY;
    [DataMember]
    public int Agility;
    [DataMember]

    public int Luck;
    [DataMember]
    public int Spirit;
    [DataMember]
    public int Strenght;
    [DataMember]
    public int Stamina;
    [DataMember]
    public int Intellect;
    [DataMember]
        public int FreePoints;

        [DataMember] public int ResetsCount;
        [DataMember]
        public int FishingLevel;
        [DataMember]
        public short FactionId;
        [DataMember]
        public byte CraftLevel;
        [DataMember]
        public bool IsAdmin;
    }
    [DataContract]
    public enum RegisterStatus
    {
        [EnumMember]
        Ok,
        [EnumMember]
        DuplicateLogin,
         [EnumMember]
        BadPassword,
        [EnumMember]
        WrongCaptcha,
         [EnumMember]
        Error
    }
    [DataContract]
    public class PreRegisterData
    {
        [DataMember]
        public Bitmap Image;
    }
    [DataContract]
    public enum AuthorizeStatus
    {
         [EnumMember]
        Ok,
        [EnumMember]
        WrongLoginOrPass,
        [EnumMember]
        AlreadyConnected,
        [EnumMember]
        ServerIsBisy
    }


    [DataContract]
    public class FullAccountInfo : IAccount
    {
        /// <summary>
        /// ID of this account
        /// </summary>
        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsActive
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? StatusUntil
        {
            get;
            set;
        }

        /// <summary>
        /// E-mail address of this account
        /// </summary>
        [DataMember]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The name of the Account's RoleGroup
        /// </summary>
        [DataMember]
        public string RoleGroupName { get; set; }

        [DataMember]
        public byte[] LastIP { get; set; }

        [DataMember]
        public DateTime? LastLogin { get; set; }

        [DataMember]
        public int HighestCharLevel
        {
            get;
            set;
        }

    }

    /// <summary>
    /// Holds information about an account
    /// </summary>
    [DataContract]
    public class AccountInfo : IAccountInfo
    {
        /// <summary>
        /// ID of this account
        /// </summary>
        [DataMember]
        public int AccountId
        {
            get;
            set;
        }

        /// <summary>
        /// E-mail address of this account
        /// </summary>
        [DataMember]
        public string EmailAddress
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the Account's RoleGroup
        /// </summary>
        [DataMember]
        public string RoleGroupName
        {
            get;
            set;
        }

        [DataMember]
        public byte[] LastIP
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? LastLogin
        {
            get;
            set;
        }

        [DataMember]
        public int HighestCharLevel
        {
            get;
            set;
        }

    }

    public interface IAccountInfo
    {
        /// <summary>
        /// ID of this account
        /// </summary>
        int AccountId
        {
            get;
        }

        /// <summary>
        /// E-mail address of this account
        /// </summary>
        string EmailAddress
        {
            get;
        }


        /// <summary>
        /// The name of the Account's RoleGroup
        /// </summary>
        string RoleGroupName
        {
            get;
        }

        byte[] LastIP
        {
            get;
        }

        DateTime? LastLogin
        {
            get;
        }

        int HighestCharLevel
        {
            get;
        }

    }

    public interface IAccount : IAccountInfo
    {
        string Name
        {
            get;
        }

        bool IsActive
        {
            get;
        }

        DateTime? StatusUntil
        {
            get;
        }
    }
}