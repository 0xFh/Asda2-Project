using System;
using System.CodeDom.Compiler;
using System.ServiceModel;
using WCell.Intercommunication.DataTypes;

namespace WCell.Intercommunication
{
    [ServiceContract(ConfigurationName = "IAuthenticationService")]
    [GeneratedCode("System.ServiceModel", "3.0.0.0")]
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
        CharacterFullInfo BanCharacter(uint characterId, DateTime until);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/UnBanCharacter",
            ReplyAction = "http://www.wcell.org/IServerIPC/UnBanCharacterResponse")]
        CharacterFullInfo UnBanCharacter(uint characterId);

        [OperationContract(Action = "http://www.wcell.org/IServerIPC/KickCharacter",
            ReplyAction = "http://www.wcell.org/IServerIPC/KickCharacterResponse")]
        CharacterFullInfo KickCharacter(uint characterId, string reason);

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
}