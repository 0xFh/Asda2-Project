using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class AccountsInfo
    {
        [DataMember] public int TotalAccounts;
        [DataMember] public int TotalOnlineAccounts;
        [DataMember] public List<AccountBaseInfo> Accounts;
    }
}