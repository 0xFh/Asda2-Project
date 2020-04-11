using System.Collections.Generic;
using System.IO;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Util
{
    public static class IOExtensions
    {
        public static Matrix ReadMatrix(this BinaryReader br)
        {
            return new Matrix(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Vector3 ReadWMOVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static List<int> ReadInt32List(this BinaryReader br)
        {
            int capacity = br.ReadInt32();
            List<int> intList = new List<int>(capacity);
            for (int index = 0; index < capacity; ++index)
                intList.Add(br.ReadInt32());
            return intList;
        }

        public static int[] ReadInt32Array(this BinaryReader br)
        {
            int length = br.ReadInt32();
            if (length == 0)
                return (int[]) null;
            int[] numArray = new int[length];
            for (int index = 0; index < length; ++index)
                numArray[index] = br.ReadInt32();
            return numArray;
        }

        public static List<Vector3> ReadVector3List(this BinaryReader br)
        {
            int capacity = br.ReadInt32();
            List<Vector3> vector3List = new List<Vector3>(capacity);
            for (int index = 0; index < capacity; ++index)
                vector3List.Add(br.ReadVector3());
            return vector3List;
        }

        public static Vector3[] ReadVector3Array(this BinaryReader br)
        {
            int length = br.ReadInt32();
            Vector3[] vector3Array = new Vector3[length];
            for (int index = 0; index < length; ++index)
                vector3Array[index] = br.ReadVector3();
            return vector3Array;
        }

        public static Rect ReadRect(this BinaryReader br)
        {
            return new Rect(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        /// <summary>
        /// Reads a C-style null-terminated string from the current stream.
        /// </summary>
        /// <param name="binReader">the extended <see cref="T:System.IO.BinaryReader" /> instance</param>
        /// <returns>the string being reader</returns>
        public static string ReadCString(this BinaryReader binReader)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte num;
            while ((num = binReader.ReadByte()) != (byte) 0)
                stringBuilder.Append((char) num);
            return stringBuilder.ToString();
        }

        public static byte PeekByte(this BinaryReader binReader)
        {
            byte num = binReader.ReadByte();
            --binReader.BaseStream.Position;
            return num;
        }

        public static string ReadFixedString(this BinaryReader br, int size)
        {
            byte[] bytes = br.ReadBytes(size);
            for (int count = 0; count < size; ++count)
            {
                if (bytes[count] == (byte) 0)
                    return Encoding.ASCII.GetString(bytes, 0, count);
            }

            return Encoding.ASCII.GetString(bytes);
        }

        public static bool HasData(this BinaryReader br)
        {
            return br.BaseStream.Position < br.BaseStream.Length;
        }

        public static void Write(this BinaryWriter writer, Vector3 vector3)
        {
            writer.Write(vector3.X);
            writer.Write(vector3.Y);
            writer.Write(vector3.Z);
        }

        public static void Write(this BinaryWriter writer, BoundingBox box)
        {
            writer.Write(box.Min);
            writer.Write(box.Max);
        }

        public static void Write(this BinaryWriter writer, Matrix mat)
        {
            writer.Write(mat.M11);
            writer.Write(mat.M12);
            writer.Write(mat.M13);
            writer.Write(mat.M14);
            writer.Write(mat.M21);
            writer.Write(mat.M22);
            writer.Write(mat.M23);
            writer.Write(mat.M24);
            writer.Write(mat.M31);
            writer.Write(mat.M32);
            writer.Write(mat.M33);
            writer.Write(mat.M34);
            writer.Write(mat.M41);
            writer.Write(mat.M42);
            writer.Write(mat.M43);
            writer.Write(mat.M44);
        }

        public static void Write(this BinaryWriter writer, ICollection<int> list)
        {
            if (list == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(list.Count);
                foreach (int num in (IEnumerable<int>) list)
                    writer.Write(num);
            }
        }

        public static void Write(this BinaryWriter writer, ICollection<Vector3> list)
        {
            if (list == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(list.Count);
                foreach (Vector3 vector3 in (IEnumerable<Vector3>) list)
                    writer.Write(vector3);
            }
        }

        public static void Write(this BinaryWriter writer, Rect rect)
        {
            writer.Write(rect.X);
            writer.Write(rect.Y);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
        }
    }
}