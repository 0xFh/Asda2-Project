using System;
using System.Collections.Generic;
using System.Text;

namespace WCell.Core
{
  public class ByteBuffer
  {
    private const int MAX_LENGTH = 1024;
    private byte[] _data;
    private int _index;
    private int _length;
    private int _maxlength;

    public ByteBuffer()
    {
      _maxlength = 1024;
      _data = new byte[_maxlength];
      _length = _maxlength;
      _index = 0;
    }

    public ByteBuffer(int len)
    {
      if(len > _maxlength)
        _maxlength = len;
      _data = new byte[_maxlength];
      _length = len;
      _index = 0;
    }

    public ByteBuffer(byte[] buff)
    {
      _length = buff.Length;
      if(_length > _maxlength)
        _maxlength = _length;
      _data = new byte[_maxlength];
      _index = 0;
      buff.CopyTo(_data, 0);
    }

    public int Length()
    {
      return _length;
    }

    public void Resize(int len)
    {
      if(len <= _maxlength)
        _length = len;
      if(len <= _maxlength)
        return;
      byte[] numArray = new byte[_length];
      _data.CopyTo(numArray, 0);
      _maxlength = len;
      _data = new byte[_maxlength];
      numArray.CopyTo(_data, 0);
      _length = _maxlength;
    }

    public void WriteBytes(byte[] data)
    {
      for(int index = 0; index < data.Length; ++index)
        _data[_index + index] = data[index];
      _index += data.Length;
    }

    public void ResetIndex()
    {
      _index = 0;
    }

    public void ClearData()
    {
      for(int index = 0; index < _maxlength; ++index)
        _data[index] = 0;
    }

    public ushort ReadUInt16()
    {
      if(_length < _index + 2)
        return 0;
      ushort uint16 = BitConverter.ToUInt16(_data, _index);
      _index += 2;
      return uint16;
    }

    public uint ReadUInt32()
    {
      if(_length < _index + 4)
        return 0;
      uint uint32 = BitConverter.ToUInt32(_data, _index);
      _index += 4;
      return uint32;
    }

    public ulong ReadUInt64()
    {
      if(_length < _index + 8)
        return 0;
      ulong uint64 = BitConverter.ToUInt64(_data, _index);
      _index += 8;
      return uint64;
    }

    public short ReadInt16()
    {
      if(_length < _index + 2)
        return 0;
      short int16 = BitConverter.ToInt16(_data, _index);
      _index += 2;
      return int16;
    }

    public int ReadInt32()
    {
      if(_length < _index + 4)
        return 0;
      int int32 = BitConverter.ToInt32(_data, _index);
      _index += 4;
      return int32;
    }

    public long ReadInt64()
    {
      if(_length < _index + 8)
        return 0;
      long int64 = BitConverter.ToInt64(_data, _index);
      _index += 8;
      return int64;
    }

    public double ReadDouble()
    {
      if(_length < _index + 8)
        return 0.0;
      double num = BitConverter.ToDouble(_data, _index);
      _index += 8;
      return num;
    }

    public char ReadChar()
    {
      if(_length < _index + 2)
        return char.MinValue;
      char ch = BitConverter.ToChar(_data, _index);
      _index += 2;
      return ch;
    }

    public byte ReadByte()
    {
      if(_length < _index + 1)
        return 0;
      byte num = _data[_index];
      ++_index;
      return num;
    }

    public string ReadString()
    {
      try
      {
        string str = "";
        for(char ch = ReadChar(); ch != char.MinValue; ch = ReadChar())
          str += (string) (object) ch;
        return str;
      }
      catch
      {
        return "";
      }
    }

    public string ReadAsciiString()
    {
      try
      {
        List<byte> byteList = new List<byte>();
        for(byte index = ReadByte(); index != (byte) 0; index = ReadByte())
          byteList.Add(index);
        return Encoding.ASCII.GetString(byteList.ToArray());
      }
      catch
      {
        return "";
      }
    }

    public void WriteUInt16(ushort val)
    {
      if(_length < _index + 2)
        return;
      byte[] numArray = new byte[2];
      byte[] bytes = BitConverter.GetBytes(val);
      _data[_index] = bytes[0];
      _data[_index + 1] = bytes[1];
      _index += 2;
    }

    public void WriteUInt32(uint val)
    {
      if(_length < _index + 4)
        return;
      byte[] numArray = new byte[4];
      byte[] bytes = BitConverter.GetBytes(val);
      _data[_index] = bytes[0];
      _data[_index + 1] = bytes[1];
      _data[_index + 2] = bytes[2];
      _data[_index + 3] = bytes[3];
      _index += 4;
    }

    public void WriteUInt64(ulong val)
    {
      if(_length < _index + 8)
        return;
      byte[] numArray = new byte[8];
      byte[] bytes = BitConverter.GetBytes(val);
      _data[_index] = bytes[0];
      _data[_index + 1] = bytes[1];
      _data[_index + 2] = bytes[2];
      _data[_index + 3] = bytes[3];
      _data[_index + 4] = bytes[4];
      _data[_index + 5] = bytes[5];
      _data[_index + 6] = bytes[6];
      _data[_index + 7] = bytes[7];
      _index += 8;
    }

    public void WriteInt16(short val)
    {
      if(_length < _index + 2)
        return;
      byte[] numArray = new byte[2];
      byte[] bytes = BitConverter.GetBytes(val);
      _data[_index] = bytes[0];
      _data[_index + 1] = bytes[1];
      _index += 2;
    }

    public void WriteInt32(int val)
    {
      if(_length < _index + 4)
        return;
      byte[] numArray = new byte[4];
      byte[] bytes = BitConverter.GetBytes(val);
      _data[_index] = bytes[0];
      _data[_index + 1] = bytes[1];
      _data[_index + 2] = bytes[2];
      _data[_index + 3] = bytes[3];
      _index += 4;
    }

    public void WriteInt64(long val)
    {
      if(_length < _index + 8)
        return;
      byte[] numArray = new byte[8];
      byte[] bytes = BitConverter.GetBytes(val);
      _data[_index] = bytes[0];
      _data[_index + 1] = bytes[1];
      _data[_index + 2] = bytes[2];
      _data[_index + 3] = bytes[3];
      _data[_index + 4] = bytes[4];
      _data[_index + 5] = bytes[5];
      _data[_index + 6] = bytes[6];
      _data[_index + 7] = bytes[7];
      _index += 8;
    }

    public void WriteDouble(double val)
    {
      if(_length < _index + 8)
        return;
      byte[] numArray = new byte[8];
      byte[] bytes = BitConverter.GetBytes(val);
      _data[_index] = bytes[0];
      _data[_index + 1] = bytes[1];
      _data[_index + 2] = bytes[2];
      _data[_index + 3] = bytes[3];
      _data[_index + 4] = bytes[4];
      _data[_index + 5] = bytes[5];
      _data[_index + 6] = bytes[6];
      _data[_index + 7] = bytes[7];
      _index += 8;
    }

    public void WriteByte(byte val)
    {
      if(_length < _index + 1)
        return;
      _data[_index] = val;
      ++_index;
    }

    public void WriteString(string text)
    {
      if(_length < _index + text.Length * 2 + 2)
        return;
      foreach(byte val in Encoding.Unicode.GetBytes(text))
        WriteByte(val);
      WriteByte(0);
      WriteByte(0);
    }

    public void SetByte(byte b)
    {
      if(_length < _index + 1)
        return;
      ++_index;
      _data[_index] = b;
    }

    public byte GetByte(int ind)
    {
      if(_length >= ind)
        return _data[ind];
      return 0;
    }

    public void SetByte(byte b, int ind)
    {
      if(_length < ind)
        return;
      _data[ind] = b;
    }

    public int GetIndex()
    {
      return _index;
    }

    public void SetIndex(int ind)
    {
      _index = ind;
    }

    public byte[] Get_ByteArray()
    {
      byte[] numArray = new byte[_length];
      for(int index = 0; index < _length; ++index)
        numArray[index] = _data[index];
      return numArray;
    }

    public byte[] Get_UsefullDataByteArray()
    {
      byte[] numArray = new byte[_index];
      for(int index = 0; index < _index; ++index)
        numArray[index] = _data[index];
      return numArray;
    }
  }
}