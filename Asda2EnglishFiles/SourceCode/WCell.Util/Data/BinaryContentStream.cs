using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WCell.Util.Data
{
    public class BinaryContentStream
    {
        private readonly DataHolderDefinition m_Def;
        private IBinaryPersistor[] m_persistors;
        private IDataField[] m_fields;

        public BinaryContentStream(DataHolderDefinition def)
        {
            this.m_Def = def;
            this.InitPersistors();
        }

        private void InitPersistors()
        {
            this.m_fields = new IDataField[this.m_Def.Fields.Values.Count];
            this.m_persistors = new IBinaryPersistor[this.m_fields.Length];
            int index = 0;
            if (this.m_Def.DependingField != null)
            {
                this.m_persistors[0] = BinaryPersistors.GetPersistor((IDataField) this.m_Def.DependingField);
                this.m_fields[0] = (IDataField) this.m_Def.DependingField;
                ++index;
            }

            foreach (IDataField field in this.m_Def.Fields.Values)
            {
                if (field != this.m_Def.DependingField)
                {
                    IBinaryPersistor persistor = BinaryPersistors.GetPersistor(field);
                    this.m_persistors[index] = persistor;
                    this.m_fields[index] = field;
                    ++index;
                }
            }
        }

        public void WriteAll(string filename, IEnumerable holders)
        {
            this.WriteAll(new BinaryWriter((Stream) new FileStream(filename, FileMode.Create, FileAccess.Write)),
                holders);
        }

        public void WriteAll(BinaryWriter writer, IEnumerable holders)
        {
            long position = writer.BaseStream.Position;
            writer.BaseStream.Position += 4L;
            int num = 0;
            foreach (object holder in holders)
            {
                if (holder != null)
                {
                    ++num;
                    this.Write(writer, (IDataHolder) holder);
                }
            }

            writer.BaseStream.Position = position;
            writer.Write(num);
        }

        private void Write(BinaryWriter writer, IDataHolder holder)
        {
            for (int index = 0; index < this.m_persistors.Length; ++index)
            {
                IBinaryPersistor persistor = this.m_persistors[index];
                try
                {
                    object obj = this.m_fields[index].Accessor.Get((object) holder);
                    persistor.Write(writer, obj);
                }
                catch (Exception ex)
                {
                    throw new DataHolderException(ex,
                        "Failed to write DataHolder \"{0}\" (Persistor #{1} {2} for: {3}).", new object[4]
                        {
                            (object) holder,
                            (object) index,
                            (object) persistor,
                            (object) this.m_fields[index]
                        });
                }
            }
        }

        internal void LoadAll(BinaryReader reader, List<Action> initors)
        {
            int num = reader.ReadInt32();
            for (int index = 0; index < num; ++index)
            {
                IDataHolder dataHolder = this.Read(reader);
                initors.Add(new Action(dataHolder.FinalizeDataHolder));
            }
        }

        public IDataHolder Read(BinaryReader reader)
        {
            object firstValue = this.m_persistors[0].Read(reader);
            IDataHolder holder = (IDataHolder) this.m_Def.CreateHolder(firstValue);
            this.m_fields[0].Accessor.Set((object) holder, firstValue);
            for (int index = 1; index < this.m_persistors.Length; ++index)
            {
                IBinaryPersistor persistor = this.m_persistors[index];
                try
                {
                    object obj = persistor.Read(reader);
                    this.m_fields[index].Accessor.Set((object) holder, obj);
                }
                catch (Exception ex)
                {
                    throw new DataHolderException(ex,
                        "Failed to read DataHolder \"{0}\" (Persistor #{1} {2} for: {3}).", new object[4]
                        {
                            (object) holder,
                            (object) index,
                            (object) persistor,
                            (object) this.m_fields[index]
                        });
                }
            }

            return holder;
        }
    }
}