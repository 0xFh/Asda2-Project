using System.Collections.Generic;

namespace WCell.Intercommunication.DataTypes
{
    public interface IBufferedCommandResponse
    {
        List<string> Replies { get; set; }
    }
}