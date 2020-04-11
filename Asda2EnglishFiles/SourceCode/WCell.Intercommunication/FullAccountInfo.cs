using System;
using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class FullAccountInfo : IAccount, IAccountInfo
    {
        /// <summary>ID of this account</summary>
        [DataMember]
        public int AccountId { get; set; }

        [DataMember] public string Name { get; set; }

        [DataMember] public bool IsActive { get; set; }

        [DataMember] public DateTime? StatusUntil { get; set; }

        /// <summary>E-mail address of this account</summary>
        [DataMember]
        public string EmailAddress { get; set; }

        /// <summary>The name of the Account's RoleGroup</summary>
        [DataMember]
        public string RoleGroupName { get; set; }

        [DataMember] public byte[] LastIP { get; set; }

        [DataMember] public DateTime? LastLogin { get; set; }

        [DataMember] public int HighestCharLevel { get; set; }
    }
}