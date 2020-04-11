using System;

namespace WCell.Intercommunication
{
    public interface IAccountInfo
    {
        /// <summary>ID of this account</summary>
        int AccountId { get; }

        /// <summary>E-mail address of this account</summary>
        string EmailAddress { get; }

        /// <summary>The name of the Account's RoleGroup</summary>
        string RoleGroupName { get; }

        byte[] LastIP { get; }

        DateTime? LastLogin { get; }

        int HighestCharLevel { get; }
    }
}