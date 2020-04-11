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
            this._maxlength = 1024;
            this._data = new byte[this._maxlength];
            this._length = this._maxlength;
            this._index = 0;
        }

        public ByteBuffer(int len)
        {
            if (len > this._maxlength)
                this._maxlength = len;
            this._data = new byte[this._maxlength];
            this._length = len;
            this._index = 0;
        }

        public ByteBuffer(byte[] buff)
        {
            this._length = buff.Length;
            if (this._length > this._maxlength)
                this._maxlength = this._length;
            this._data = new byte[this._maxlength];
            this._index = 0;
            buff.CopyTo((Array) this._data, 0);
        }

        public int Length()
        {
            return this._length;
        }

        public void Resize(int len)
        {
            if (len <= this._maxlength)
                this._length = len;
            if (len <= this._maxlength)
                return;
            byte[] numArray = new byte[this._length];
            this._data.CopyTo((Array) numArray, 0);
            this._maxlength = len;
            this._data = new byte[this._maxlength];
            numArray.CopyTo((Array) this._data, 0);
            this._length = this._maxlength;
        }

        public void WriteBytes(byte[] data)
        {
            for (int index = 0; index < data.Length; ++index)
                this._data[this._index + index] = data[index];
            this._index += data.Length;
        }

        public void ResetIndex()
        {
            this._index = 0;
        }

        public void ClearData()
        {
            for (int index = 0; index < this._maxlength; ++index)
                this._data[index] = (byte) 0;
        }

        public ushort ReadUInt16()
        {
            if (this._length < this._index + 2)
                return 0;
            ushort uint16 = BitConverter.ToUInt16(this._data, this._index);
            this._index += 2;
            return uint16;
        }

        public uint ReadUInt32()
        {
            if (this._length < this._index + 4)
                return 0;
            uint uint32 = BitConverter.ToUInt32(this._data, this._index);
            this._index += 4;
            return uint32;
        }

        public ulong ReadUInt64()
        {
            if (this._length < this._index + 8)
                return 0;
            ulong uint64 = BitConverter.ToUInt64(this._data, this._index);
            this._index += 8;
            return uint64;
        }

        public short ReadInt16()
        {
            if (this._length < this._index + 2)
                return 0;
            short int16 = BitConverter.ToInt16(this._data, this._index);
            this._index += 2;
            return int16;
        }

        public int ReadInt32()
        {
            if (this._length < this._index + 4)
                return 0;
            int int32 = BitConverter.ToInt32(this._data, this._index);
            this._index += 4;
            return int32;
        }

        public long ReadInt64()
        {
            if (this._length < this._index + 8)
                return 0;
            long int64 = BitConverter.ToInt64(this._data, this._index);
            this._index += 8;
            return int64;
        }

        public double ReadDouble()
        {
            if (this._length < this._index + 8)
                return 0.0;
            double num = BitConverter.ToDouble(this._data, this._index);
            this._index += 8;
            return num;
        }

        public char ReadChar()
        {
            if (this._length < this._index + 2)
                return char.MinValue;
            char ch = BitConverter.ToChar(this._data, this._index);
            this._index += 2;
            return ch;
        }

        public byte ReadByte()
        {
            if (this._length < this._index + 1)
                return 0;
            byte num = this._data[this._index];
            ++this._index;
            return num;
        }

        public string ReadString()
        {
            try
            {
                string str = "";
                for (char ch = this.ReadChar(); ch != char.MinValue; ch = this.ReadChar())
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
                for (byte index = this.ReadByte(); index != (byte) 0; index = this.ReadByte())
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
            if (this._length < this._index + 2)
                return;
            byte[] numArray = new byte[2];
            byte[] bytes = BitConverter.GetBytes(val);
            this._data[this._index] = bytes[0];
            this._data[this._index + 1] = bytes[1];
            this._index += 2;
        }

        public void WriteUInt32(uint val)
        {
            if (this._length < this._index + 4)
                return;
            byte[] numArray = new byte[4];
            byte[] bytes = BitConverter.GetBytes(val);
            this._data[this._index] = bytes[0];
            this._data[this._index + 1] = bytes[1];
            this._data[this._index + 2] = bytes[2];
            this._data[this._index + 3] = bytes[3];
            this._index += 4;
        }

        public void WriteUInt64(ulong val)
        {
            if (this._length < this._index + 8)
                return;
            byte[] numArray = new byte[8];
            byte[] bytes = BitConverter.GetBytes(val);
            this._data[this._index] = bytes[0];
            this._data[this._index + 1] = bytes[1];
            this._data[this._index + 2] = bytes[2];
            this._data[this._index + 3] = bytes[3];
            this._data[this._index + 4] = bytes[4];
            this._data[this._index + 5] = bytes[5];
            this._data[this._index + 6] = bytes[6];
            this._data[this._index + 7] = bytes[7];
            this._index += 8;
        }

        public void WriteInt16(short val)
        {
            if (this._length < this._index + 2)
                return;
            byte[] numArray = new byte[2];
            byte[] bytes = BitConverter.GetBytes(val);
            this._data[this._index] = bytes[0];
            this._data[this._index + 1] = bytes[1];
            this._index += 2;
        }

        public void WriteInt32(int val)
        {
            if (this._length < this._index + 4)
                return;
            byte[] numArray = new byte[4];
            byte[] bytes = BitConverter.GetBytes(val);
            this._data[this._index] = bytes[0];
            this._data[this._index + 1] = bytes[1];
            this._data[this._index + 2] = bytes[2];
            this._data[this._index + 3] = bytes[3];
            this._index += 4;
        }

        public void WriteInt64(long val)
        {
            if (this._length < this._index + 8)
                return;
            byte[] numArray = new byte[8];
            byte[] bytes = BitConverter.GetBytes(val);
            this._data[this._index] = bytes[0];
            this._data[this._index + 1] = bytes[1];
            this._data[this._index + 2] = bytes[2];
            this._data[this._index + 3] = bytes[3];
            this._data[this._index + 4] = bytes[4];
            this._data[this._index + 5] = bytes[5];
            this._data[this._index + 6] = bytes[6];
            this._data[this._index + 7] = bytes[7];
            this._index += 8;
        }

        public void WriteDouble(double val)
        {
            if (this._length < this._index + 8)
                return;
            byte[] numArray = new byte[8];
            byte[] bytes = BitConverter.GetBytes(val);
            this._data[this._index] = bytes[0];
            this._data[this._index + 1] = bytes[1];
            this._data[this._index + 2] = bytes[2];
            this._data[this._index + 3] = bytes[3];
            this._data[this._index + 4] = bytes[4];
            this._data[this._index + 5] = bytes[5];
            this._data[this._index + 6] = bytes[6];
            this._data[this._index + 7] = bytes[7];
            this._index += 8;
        }

        public void WriteByte(byte val)
        {
            if (this._length < this._index + 1)
                return;
            this._data[this._index] = val;
            ++this._index;
        }

        public void WriteString(string text)
        {
            if (this._length < this._index + text.Length * 2 + 2)
                return;
            foreach (byte val in Encoding.Unicode.GetBytes(text))
                this.WriteByte(val);
            this.WriteByte((byte) 0);
            this.WriteByte((byte) 0);
        }

        public void SetByte(byte b)
        {
            if (this._length < this._index + 1)
                return;
            ++this._index;
            this._data[this._index] = b;
        }

        public byte GetByte(int ind)
        {
            if (this._length >= ind)
                return this._data[ind];
            return 0;
        }

        public void SetByte(byte b, int ind)
        {
            if (this._length < ind)
                return;
            this._data[ind] = b;
        }

        public int GetIndex()
        {
            return this._index;
        }

        public void SetIndex(int ind)
        {
            this._index = ind;
        }

        public byte[] Get_ByteArray()
        {
            byte[] numArray = new byte[this._length];
            for (int index = 0; index < this._length; ++index)
                numArray[index] = this._data[index];
            return numArray;
        }

        public byte[] Get_UsefullDataByteArray()
        {
            byte[] numArray = new byte[this._index];
            for (int index = 0; index < this._index; ++index)
                numArray[index] = this._data[index];
            return numArray;
        }
    }
}