using System.IO;
using System.Security.Cryptography;

namespace WCell.Core.Cryptography
{
    /// <summary>
    /// Provides facilities for performing common-but-specific
    /// cryptographical operations
    /// </summary>
    public static class HashUtilities
    {
        /// <summary>
        /// Computes a hash from hash data brokers using the given
        /// hashing algorithm
        /// </summary>
        /// <param name="algorithm">the hashing algorithm to use</param>
        /// <param name="brokers">the data brokers to hash</param>
        /// <returns>the hash result of all the data brokers</returns>
        public static byte[] FinalizeHash(HashAlgorithm algorithm, params HashUtilities.HashDataBroker[] brokers)
        {
            MemoryStream memoryStream = new MemoryStream();
            foreach (HashUtilities.HashDataBroker broker in brokers)
                memoryStream.Write(broker.RawData, 0, broker.Length);
            memoryStream.Position = 0L;
            return algorithm.ComputeHash((Stream) memoryStream);
        }

        /// <summary>
        /// Computes a hash from hash data brokers using the given
        /// hash algorithm, and generates a BigInteger from it
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="brokers"></param>
        /// <returns></returns>
        public static BigInteger HashToBigInteger(HashAlgorithm algorithm,
            params HashUtilities.HashDataBroker[] brokers)
        {
            return new BigInteger(HashUtilities.FinalizeHash(algorithm, brokers));
        }

        /// <summary>
        /// Brokers various data types into their integral raw
        /// form for usage by other cryptographical functions
        /// </summary>
        public class HashDataBroker
        {
            internal byte[] RawData;

            /// <summary>Default constructor</summary>
            /// <param name="data">the data to broker</param>
            public HashDataBroker(byte[] data)
            {
                this.RawData = data;
            }

            internal int Length
            {
                get { return this.RawData.Length; }
            }

            /// <summary>Implicit operator for byte[]-&gt;HashDataBroker casts</summary>
            /// <param name="data">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashUtilities.HashDataBroker(byte[] data)
            {
                return new HashUtilities.HashDataBroker(data);
            }

            /// <summary>Implicit operator for string-&gt;HashDataBroker casts</summary>
            /// <param name="str">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashUtilities.HashDataBroker(string str)
            {
                return new HashUtilities.HashDataBroker(WCellConstants.DefaultEncoding.GetBytes(str));
            }

            /// <summary>
            /// Implicit operator for BigInteger-&gt;HashDataBroker casts
            /// </summary>
            /// <param name="integer">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashUtilities.HashDataBroker(BigInteger integer)
            {
                return new HashUtilities.HashDataBroker(integer.GetBytes());
            }

            /// <summary>Implicit operator for uint-&gt;HashDataBroker casts</summary>
            /// <param name="integer">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashUtilities.HashDataBroker(uint integer)
            {
                return new HashUtilities.HashDataBroker(new BigInteger((long) integer).GetBytes());
            }
        }
    }
}