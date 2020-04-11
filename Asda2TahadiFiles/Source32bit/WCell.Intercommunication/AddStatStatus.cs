using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
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
}