using System.IO;

namespace WCell.Core.WDB
{
    public abstract class WDBRecordConverter<TEntry>
    {
        public abstract TEntry Convert(BinaryReader binReader);
    }
}