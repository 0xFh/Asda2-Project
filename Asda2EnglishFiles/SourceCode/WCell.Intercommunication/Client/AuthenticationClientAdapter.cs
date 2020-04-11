using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WCell.Intercommunication.DataTypes;

namespace WCell.Intercommunication.Client
{
    [GeneratedCode("System.ServiceModel", "3.0.0.0")]
    [DebuggerStepThrough]
    public class AuthenticationClientAdapter : ClientBase<IWCellIntercomService>, IWCellIntercomService
    {
        public event Action<Exception> Error;

        public AuthenticationClientAdapter()
        {
        }

        public AuthenticationClientAdapter(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        public AuthenticationClientAdapter(string endpointConfigurationName, string remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        public AuthenticationClientAdapter(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        public AuthenticationClientAdapter(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        private void OnError(Exception e)
        {
            Action<Exception> error = this.Error;
            if (error == null)
                return;
            error(e);
        }

        public BufferedCommandResponse ExecuteCommand(string cmd)
        {
            try
            {
                return this.Channel.ExecuteCommand(cmd);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (BufferedCommandResponse) null;
            }
        }

        public AuthorizeStatus Authorize(string login, string password)
        {
            try
            {
                return this.Channel.Authorize(login, password);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return AuthorizeStatus.ServerIsBisy;
            }
        }

        public PreRegisterData PreRegister()
        {
            try
            {
                return this.Channel.PreRegister();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (PreRegisterData) null;
            }
        }

        public RegisterStatus Register(string login, string password, string email, int captcha)
        {
            try
            {
                return this.Channel.Register(login, password, email, captcha);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return RegisterStatus.Error;
            }
        }

        public UpdateData Update()
        {
            try
            {
                return this.Channel.Update();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (UpdateData) null;
            }
        }

        public AddStatStatus AddStat(Asda2StatType type, uint amount)
        {
            try
            {
                return this.Channel.AddStat(type, amount);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return new AddStatStatus(AddStatStatusEnum.Error, ex.Message);
            }
        }

        public ChangeProffessionEnum ChangeProffession(byte c)
        {
            try
            {
                return this.Channel.ChangeProffession(c);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return ChangeProffessionEnum.Error;
            }
        }

        public ErrorTeleportationEnum ErrorTeleportation()
        {
            try
            {
                return this.Channel.ErrorTeleportation();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return ErrorTeleportationEnum.Error;
            }
        }

        public StartPkModeEnum StartPkMode()
        {
            try
            {
                return this.Channel.StartPkMode();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return StartPkModeEnum.Error;
            }
        }

        public RebornEnum Reborn()
        {
            try
            {
                return this.Channel.Reborn();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return RebornEnum.Error;
            }
        }

        public LogoutEnum LogOut()
        {
            try
            {
                return this.Channel.LogOut();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return LogoutEnum.Error;
            }
        }

        public ResetStatsEnum ResetStats()
        {
            try
            {
                return this.Channel.ResetStats();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return ResetStatsEnum.Error;
            }
        }

        public SetWarehousePassEnum SetWarehousePassword(string oldPass, string newPass)
        {
            try
            {
                return this.Channel.SetWarehousePassword(oldPass, newPass);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return SetWarehousePassEnum.Error;
            }
        }

        public UnlockWarehouseEnum UnlockWarehouse(string pass)
        {
            try
            {
                return this.Channel.UnlockWarehouse(pass);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return UnlockWarehouseEnum.Error;
            }
        }

        public CharactersInfo GetCharacters(int pageSize, int page, bool isOnline, string nameFilter)
        {
            try
            {
                return this.Channel.GetCharacters(pageSize, page, isOnline, nameFilter);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (CharactersInfo) null;
            }
        }

        public CharacterFullInfo GetCharacter(uint characterId)
        {
            try
            {
                return this.Channel.GetCharacter(characterId);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (CharacterFullInfo) null;
            }
        }

        public CharacterFullInfo BanCharacter(uint characterId, DateTime until)
        {
            try
            {
                return this.Channel.BanCharacter(characterId, until);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (CharacterFullInfo) null;
            }
        }

        public CharacterFullInfo UnBanCharacter(uint characterId)
        {
            try
            {
                return this.Channel.UnBanCharacter(characterId);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (CharacterFullInfo) null;
            }
        }

        public CharacterFullInfo KickCharacter(uint characterId, string reason)
        {
            try
            {
                return this.Channel.KickCharacter(characterId, reason);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (CharacterFullInfo) null;
            }
        }

        public AccountsInfo GetAccounts(int pageSize, int page, bool isOnline, string nameFilter)
        {
            try
            {
                return this.Channel.GetAccounts(pageSize, page, isOnline, nameFilter);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (AccountsInfo) null;
            }
        }

        public AccountFullInfo BanAccount(string name, DateTime until)
        {
            try
            {
                return this.Channel.BanAccount(name, until);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (AccountFullInfo) null;
            }
        }

        public AccountFullInfo UnBanAccount(string name)
        {
            try
            {
                return this.Channel.UnBanAccount(name);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (AccountFullInfo) null;
            }
        }

        public AccountFullInfo LogoffAccount(string name)
        {
            try
            {
                return this.Channel.LogoffAccount(name);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (AccountFullInfo) null;
            }
        }

        public CharacterFullInfo SetCharacterLevel(uint characterId, byte level)
        {
            try
            {
                return this.Channel.SetCharacterLevel(characterId, level);
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return (CharacterFullInfo) null;
            }
        }

        public ResetStatsEnum ResetSkills()
        {
            try
            {
                return this.Channel.ResetSkills();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return ResetStatsEnum.Error;
            }
        }

        public ResetStatsEnum ResetFaction()
        {
            try
            {
                return this.Channel.ResetFaction();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return ResetStatsEnum.Error;
            }
        }

        public bool TriggerExpBlock()
        {
            try
            {
                return this.Channel.TriggerExpBlock();
            }
            catch (Exception ex)
            {
                this.OnError(ex);
                return false;
            }
        }
    }
}