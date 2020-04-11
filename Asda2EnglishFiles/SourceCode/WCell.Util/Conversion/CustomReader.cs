using System.Data;

namespace WCell.Util.Conversion
{
    public class CustomReader : IFieldReader
    {
        private readonly IConverter m_Converter;

        public CustomReader(IConverter converter)
        {
            this.m_Converter = converter;
        }

        public IConverter Converter
        {
            get { return this.m_Converter; }
        }

        public object Read(IDataReader reader, int index)
        {
            return this.m_Converter.Convert(reader[index]);
        }
    }
}