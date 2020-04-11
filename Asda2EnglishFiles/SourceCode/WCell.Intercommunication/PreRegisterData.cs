using System.Drawing;
using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class PreRegisterData
    {
        [DataMember] public Bitmap Image;
    }
}