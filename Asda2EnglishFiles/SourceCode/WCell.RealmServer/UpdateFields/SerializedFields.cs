using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.UpdateFields
{
    [Serializable]
    public class SerializedFields
    {
        private CompoundType[] m_values;

        private SerializedFields()
        {
        }

        public CompoundType[] Values
        {
            get { return this.m_values; }
            set { this.m_values = value; }
        }

        public void SetValues(ObjectBase obj)
        {
            this.m_values = obj.UpdateValues;
        }

        public static byte[] GetSerializedFields(ObjectBase obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            SerializedFields serializedFields = new SerializedFields();
            serializedFields.SetValues(obj);
            binaryFormatter.Serialize((Stream) memoryStream, (object) serializedFields);
            return memoryStream.ToArray();
        }

        public static CompoundType[] GetDeserializedFields(byte[] serializedFields)
        {
            return ((SerializedFields) new BinaryFormatter().Deserialize((Stream) new MemoryStream(serializedFields)))
                .Values;
        }
    }
}