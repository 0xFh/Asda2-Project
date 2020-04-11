using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace WCell.Intercommunication.DataTypes
{
    [DataContract]
    public class BufferedCommandResponse : IBufferedCommandResponse
    {
        public BufferedCommandResponse()
        {
            this.Replies = new List<string>(3);
        }

        public BufferedCommandResponse(params string[] replies)
        {
            this.Replies = ((IEnumerable<string>) replies).ToList<string>();
        }

        [DataMember] public List<string> Replies { get; set; }
    }
}