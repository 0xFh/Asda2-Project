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
			Replies = new List<string>(3);
		}

		public BufferedCommandResponse(params string[] replies)
		{
			Replies = replies.ToList();
		}

		[DataMember]
		public List<string> Replies
		{
			get;
			set;
		}
	}
    public interface IBufferedCommandResponse
    {
        List<string> Replies
        {
            get;
            set;
        }
    }
}