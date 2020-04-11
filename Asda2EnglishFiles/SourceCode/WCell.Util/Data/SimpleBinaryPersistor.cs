using System.Collections.Generic;
using System.IO;

namespace WCell.Util.Data
{
    public abstract class SimpleBinaryPersistor : ISimpleBinaryPersistor, IBinaryPersistor
    {
        public abstract int BinaryLength { get; }

        public abstract void Write(BinaryWriter writer, object obj);

        public abstract object Read(BinaryReader reader);

        public int InitPersistor(List<IGetterSetter> stringPersistors)
        {
            return this.BinaryLength;
        }
    }
}