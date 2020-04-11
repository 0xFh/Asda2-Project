using System;
using System.Net;
using System.Xml.Serialization;

namespace Cell.Core
{
    /// <summary>
    /// This class provides a wrapper for <see cref="T:System.Net.IPAddress" /> that can be serialized with XML.
    /// </summary>
    /// <seealso cref="N:System.Xml.Serialization" />
    /// <seealso cref="T:System.Net.IPAddress" />
    [Serializable]
    public class XmlIPAddress
    {
        /// <summary>
        /// The <see cref="P:Cell.Core.XmlIPAddress.IPAddress" />.
        /// </summary>
        private IPAddress _ipAddress = new IPAddress(16777343L);

        /// <summary>
        /// Gets/Sets a string representation of a <see cref="T:System.Net.IPAddress" />.
        /// </summary>
        public string Address
        {
            get { return this._ipAddress.ToString(); }
            set
            {
                IPAddress address;
                if (!IPAddress.TryParse(value, out address))
                    return;
                this._ipAddress = address;
            }
        }

        /// <summary>
        /// Gets/Sets the internal <see cref="T:System.Net.IPAddress" />.
        /// </summary>
        [XmlIgnore]
        public IPAddress IPAddress
        {
            get { return this._ipAddress; }
            set { this._ipAddress = value; }
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="T:Cell.Core.XmlIPAddress" /> class.
        /// </summary>
        public XmlIPAddress()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cell.Core.XmlIPAddress" /> class with the address specified as a <see cref="T:System.Byte" /> array.
        /// </summary>
        /// <param name="address">The byte array value of the IP address.</param>
        /// <exception cref="T:System.ArgumentNullException">address is null.</exception>
        public XmlIPAddress(byte[] address)
        {
            this._ipAddress = new IPAddress(address);
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="T:Cell.Core.XmlIPAddress" /> class with the specified address and scope.
        /// </summary>
        /// <param name="address">The byte array value of the IP address.</param>
        /// <param name="scopeid">The long value of the scope identifier.</param>
        /// <exception cref="T:System.ArgumentNullException">address is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// scopeid &lt; 0 or
        /// scopeid &gt; 0x00000000FFFFFFF
        /// </exception>
        public XmlIPAddress(byte[] address, long scopeId)
        {
            this._ipAddress = new IPAddress(address, scopeId);
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="T:Cell.Core.XmlIPAddress" /> class with the address specified as a <see cref="T:System.Int64" />.
        /// </summary>
        /// <param name="newAddress">The long value of the IP address.</param>
        /// <remarks>For example, the value 0x2414188f in big endian format would be the IP address "143.24.20.36".+ </remarks>
        public XmlIPAddress(long newAddress)
        {
            this._ipAddress = new IPAddress(newAddress);
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="T:Cell.Core.XmlIPAddress" /> class with the address specified as a <see cref="T:System.Net.IPAddress" />.
        /// </summary>
        /// <param name="newAddress">The new <see cref="P:Cell.Core.XmlIPAddress.IPAddress" />.</param>
        public XmlIPAddress(IPAddress newAddress)
        {
            this._ipAddress = newAddress;
        }

        /// <summary>
        /// Converts the <see cref="T:Cell.Core.XmlIPAddress" /> into a string.
        /// </summary>
        /// <returns>A string representation of the internal <see cref="P:Cell.Core.XmlIPAddress.IPAddress" />.</returns>
        public override string ToString()
        {
            return this._ipAddress.ToString();
        }

        /// <summary>Gets a hash code for the object.</summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return this._ipAddress.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            XmlIPAddress xmlIpAddress = obj as XmlIPAddress;
            if (xmlIpAddress != null)
                return xmlIpAddress.IPAddress.Equals((object) this.IPAddress);
            return false;
        }
    }
}