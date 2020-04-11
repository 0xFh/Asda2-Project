/*************************************************************************
 *
 *   file		: ClientAdapter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-17 08:08:03 +0800 (Sun, 17 Aug 2008) $
 
 *   revision		: $Rev: 598 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using WCell.Intercommunication.DataTypes;

namespace WCell.Intercommunication.Client
{
	[DebuggerStepThrough()]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	public class AuthenticationClientAdapter : ClientBase<IWCellIntercomService>, IWCellIntercomService
	{
		public event Action<Exception> Error;

		public AuthenticationClientAdapter()
		{
		}

		public AuthenticationClientAdapter(string endpointConfigurationName)
			:
				base(endpointConfigurationName)
		{
		}

		public AuthenticationClientAdapter(string endpointConfigurationName, string remoteAddress)
			:
				base(endpointConfigurationName, remoteAddress)
		{
		}

		public AuthenticationClientAdapter(string endpointConfigurationName, EndpointAddress remoteAddress)
			:
				base(endpointConfigurationName, remoteAddress)
		{
		}

		public AuthenticationClientAdapter(Binding binding, EndpointAddress remoteAddress)
			:
				base(binding, remoteAddress)
		{
		}

		private void OnError(Exception e)
		{
			var evt = Error;
			if (evt != null)
			{
				evt(e);
			}
		}
		public BufferedCommandResponse ExecuteCommand(string cmd)
		{
			try
			{
				return Channel.ExecuteCommand(cmd);
			}
			catch (Exception e)
			{
				OnError(e);
				return null;
			}
		}

	    public AuthorizeStatus Authorize(string login, string password)
	    {
            try
            {
                return Channel.Authorize(login, password);
            }
            catch (Exception e)
            {
                OnError(e);
                return AuthorizeStatus.ServerIsBisy;
            }
	    }

	    public PreRegisterData PreRegister()
	    {
            try
            {
                return Channel.PreRegister();
            }
            catch (Exception e)
            {
                OnError(e);
                return null;
            }
	    }

	    public RegisterStatus Register(string login, string password, string email, int captcha)
	    {
            try
            {
                return Channel.Register(login,password,email,captcha);
            }
            catch (Exception e)
            {
                OnError(e);
                return RegisterStatus.Error;
            }
	    }

	    public UpdateData Update()
	    {
            try
            {
                return Channel.Update();
            }
            catch (Exception e)
            {
                OnError(e);
                return null;
            }
	    }

	    public AddStatStatus AddStat(Asda2StatType type, uint amount)
	    {
            try
            {
                return Channel.AddStat(type, amount);
            }
            catch (Exception e)
            {
                OnError(e);
                return new AddStatStatus(AddStatStatusEnum.Error, e.Message);
            }
	    }

	    public ChangeProffessionEnum ChangeProffession(byte c)
	    {
            try
            {
                return Channel.ChangeProffession(c);
            }
            catch (Exception e)
            {
                OnError(e);
                return ChangeProffessionEnum.Error;
            }
	    }

	    public ErrorTeleportationEnum ErrorTeleportation()
	    {
            try
            {
                return Channel.ErrorTeleportation();
            }
            catch (Exception e)
            {
                OnError(e);
                return ErrorTeleportationEnum.Error;
            }
	    }

	    public StartPkModeEnum StartPkMode()
	    {
            try
            {
                return Channel.StartPkMode();
            }
            catch (Exception e)
            {
                OnError(e);
                return StartPkModeEnum.Error;
            }
	    }

	    public RebornEnum Reborn()
	    {
            try
            {
                return Channel.Reborn();
            }
            catch (Exception e)
            {
                OnError(e);
                return RebornEnum.Error;
            }
	    }

	    public LogoutEnum LogOut()
	    {
            try
            {
                return Channel.LogOut();
            }
            catch (Exception e)
            {
                OnError(e);
                return LogoutEnum.Error;
            }
	    }

	    public ResetStatsEnum ResetStats()
	    {
            try
            {
                return Channel.ResetStats();
            }
            catch (Exception e)
            {
                OnError(e);
                return ResetStatsEnum.Error;
            }
	    }

	    public SetWarehousePassEnum SetWarehousePassword(string oldPass, string newPass)
	    {
            try
            {
                return Channel.SetWarehousePassword(oldPass,newPass);
            }
            catch (Exception e)
            {
                OnError(e);
                return SetWarehousePassEnum.Error;
            }
	    }

	    public UnlockWarehouseEnum UnlockWarehouse(string pass)
	    {
            try
            {
                return Channel.UnlockWarehouse(pass);
            }
            catch (Exception e)
            {
                OnError(e);
                return UnlockWarehouseEnum.Error;
            }
	    }

	    public CharactersInfo GetCharacters(int pageSize, int page, bool isOnline, string nameFilter)
	    {
	        try
	        {
	            return Channel.GetCharacters(pageSize, page, isOnline, nameFilter);
	        }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

	    public CharacterFullInfo GetCharacter(uint characterId)
	    {
            try
            {
                return Channel.GetCharacter(characterId);
            }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

	    public CharacterFullInfo BanCharacter(uint characterId, DateTime until)
	    {
            try
            {
                return Channel.BanCharacter(characterId, until);
            }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

	    public CharacterFullInfo UnBanCharacter(uint characterId)
	    {
            try
            {
                return Channel.UnBanCharacter(characterId);
            }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

	    public CharacterFullInfo KickCharacter(uint characterId, string reason)
	    {
            try
            {
                return Channel.KickCharacter(characterId, reason);
            }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

	    public AccountsInfo GetAccounts(int pageSize, int page, bool isOnline, string nameFilter)
	    {
            try
            {
                return Channel.GetAccounts(pageSize, page, isOnline, nameFilter);
            }
            catch (Exception e)
            {
                OnError(e);
                return null;
            }
	    }

	    public AccountFullInfo BanAccount(string name, DateTime until)
	    {
            try
            {
                return Channel.BanAccount(name, until);
            }
            catch (Exception e)
            {
                OnError(e);
                return null;
            }
	    }

	    public AccountFullInfo UnBanAccount(string name)
	    {
            try
            {
                return Channel.UnBanAccount(name);
            }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

	    public AccountFullInfo LogoffAccount(string name)
	    {
            try
            {
                return Channel.LogoffAccount(name);
            }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

        public CharacterFullInfo SetCharacterLevel(uint characterId, byte level)
	    {
            try
            {
                return Channel.SetCharacterLevel(characterId, level);
            }
            catch (Exception e)
            {
                OnError(e); return null;
            }
	    }

	    public ResetStatsEnum ResetSkills()
	    {
            try
            {
                return Channel.ResetSkills();
            }
            catch (Exception e)
            {
                OnError(e); return ResetStatsEnum.Error;
            }
	    }

	    public ResetStatsEnum ResetFaction()
	    {
            try
            {
                return Channel.ResetFaction();
            }
            catch (Exception e)
            {
                OnError(e); return ResetStatsEnum.Error;
            }
	    }

	    public bool TriggerExpBlock()
	    {
            try
            {
                return Channel.TriggerExpBlock();
            }
            catch (Exception e)
            {
                OnError(e); return false;
            }
	    }
	}
}