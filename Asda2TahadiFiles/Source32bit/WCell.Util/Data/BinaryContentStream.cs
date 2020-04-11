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
      m_Def = def;
      InitPersistors();
    }

    private void InitPersistors()
    {
      m_fields = new IDataField[m_Def.Fields.Values.Count];
      m_persistors = new IBinaryPersistor[m_fields.Length];
      int index = 0;
      if(m_Def.DependingField != null)
      {
        m_persistors[0] = BinaryPersistors.GetPersistor(m_Def.DependingField);
        m_fields[0] = m_Def.DependingField;
        ++index;
      }

      foreach(IDataField field in m_Def.Fields.Values)
      {
        if(field != m_Def.DependingField)
        {
          IBinaryPersistor persistor = BinaryPersistors.GetPersistor(field);
          m_persistors[index] = persistor;
          m_fields[index] = field;
          ++index;
        }
      }
    }

    public void WriteAll(string filename, IEnumerable holders)
    {
      WriteAll(new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write)),
        holders);
    }

    public void WriteAll(BinaryWriter writer, IEnumerable holders)
    {
      long position = writer.BaseStream.Position;
      writer.BaseStream.Position += 4L;
      int num = 0;
      foreach(object holder in holders)
      {
        if(holder != null)
        {
          ++num;
          Write(writer, (IDataHolder) holder);
        }
      }

      writer.BaseStream.Position = position;
      writer.Write(num);
    }

    private void Write(BinaryWriter writer, IDataHolder holder)
    {
      for(int index = 0; index < m_persistors.Length; ++index)
      {
        IBinaryPersistor persistor = m_persistors[index];
        try
        {
          object obj = m_fields[index].Accessor.Get(holder);
          persistor.Write(writer, obj);
        }
        catch(Exception ex)
        {
          throw new DataHolderException(ex,
            "Failed to write DataHolder \"{0}\" (Persistor #{1} {2} for: {3}).", (object) holder, (object) index,
            (object) persistor, (object) m_fields[index]);
        }
      }
    }

    internal void LoadAll(BinaryReader reader, List<Action> initors)
    {
      int num = reader.ReadInt32();
      for(int index = 0; index < num; ++index)
      {
        IDataHolder dataHolder = Read(reader);
        initors.Add(dataHolder.FinalizeDataHolder);
      }
    }

    public IDataHolder Read(BinaryReader reader)
    {
      object firstValue = m_persistors[0].Read(reader);
      IDataHolder holder = (IDataHolder) m_Def.CreateHolder(firstValue);
      m_fields[0].Accessor.Set(holder, firstValue);
      for(int index = 1; index < m_persistors.Length; ++index)
      {
        IBinaryPersistor persistor = m_persistors[index];
        try
        {
          object obj = persistor.Read(reader);
          m_fields[index].Accessor.Set(holder, obj);
        }
        catch(Exception ex)
        {
          throw new DataHolderException(ex,
            "Failed to read DataHolder \"{0}\" (Persistor #{1} {2} for: {3}).", (object) holder, (object) index,
            (object) persistor, (object) m_fields[index]);
        }
      }

      return holder;
    }
  }
}