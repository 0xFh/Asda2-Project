using System;
using System.Globalization;

namespace WCell.Util.Graphics
{
    public struct HalfSingle : IPackedVector<ushort>, IPackedVector, IEquatable<HalfSingle>
    {
        private ushort packedValue;

        public HalfSingle(float value)
        {
            this.packedValue = HalfUtils.Pack(value);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            this.packedValue = HalfUtils.Pack(vector.X);
        }

        public float ToSingle()
        {
            return HalfUtils.Unpack(this.packedValue);
        }

        Vector4 IPackedVector.ToVector4()
        {
            return new Vector4(this.ToSingle(), 0.0f, 0.0f, 1f);
        }

        public ushort PackedValue
        {
            get { return this.packedValue; }
            set { this.packedValue = value; }
        }

        public override string ToString()
        {
            return this.ToSingle().ToString((IFormatProvider) CultureInfo.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return this.packedValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is HalfSingle && this.Equals((HalfSingle) obj);
        }

        public bool Equals(HalfSingle other)
        {
            return this.packedValue.Equals(other.packedValue);
        }

        public static bool operator ==(HalfSingle a, HalfSingle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HalfSingle a, HalfSingle b)
        {
            return !a.Equals(b);
        }
    }
}