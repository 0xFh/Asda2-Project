using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WCell.Intercommunication.DataTypes
{
    [DataContract]
    public class UpdateRealmResponse
    {
        public void AddCommand(string commandStr)
        {
            if (this.Commands == null)
                this.Commands = new List<string>(3);
            this.Commands.Add(commandStr);
        }

        [DataMember] public List<string> Commands { get; set; }
    }
}