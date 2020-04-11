using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace WCell.Core.Cryptography
{
    /// <summary>
    /// This is an implementation of the SRP algorithm documented here:
    /// 
    /// http://srp.stanford.edu/design.html
    /// 
    /// 
    /// Example code (though usually data is copied over the wire):
    /// 	WCell.Cryptography.SRP server = new WCell.Cryptography.SRP(true, "USER", "PASSWORD");
    /// 	WCell.Cryptography.SRP client = new WCell.Cryptography.SRP(false, "USER", "PASSWORD");
    /// 
    /// 	server.PublicEphemeralValueA = client.PublicEphemeralValueA;
    /// 	client.Salt = server.Salt;
    /// 	client.PublicEphemeralValueB = server.PublicEphemeralValueB;
    /// 
    /// 	Console.WriteLine("Server's session key = {0}", server.SessionKey.ToHexString());
    /// 	Console.WriteLine("Client's session key = {0}", client.SessionKey.ToHexString());
    /// 
    /// 	Console.WriteLine("\nServer key == client key {0}", server.SessionKey == client.SessionKey);
    /// 
    /// 	Console.WriteLine("Client proof valid: {0}", client.ClientSessionKeyProof == server.ClientSessionKeyProof);
    /// 	Console.WriteLine("Server proof valid: {0}", client.ServerSessionKeyProof == server.ServerSessionKeyProof);
    /// </summary>
    [GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
    [Serializable]
    public class SecureRemotePassword : ISerializable
    {
        private SecureRemotePassword.SRPParameters m_srpParams = SecureRemotePassword.SRPParameters.Defaults;
        private readonly BigInteger m_secretEphemeralValueA = SecureRemotePassword.RandomNumber();

        /// <summary>The required minimum length of a password</summary>
        public const int MinPassLength = 3;

        /// <summary>The required maximum length of a password</summary>
        public const int MaxPassLength = 16;

        private readonly bool m_isServer;
        private BigInteger m_credentialsHash;
        private BigInteger m_salt;
        private BigInteger m_sessionKey;
        private BigInteger m_publicEphemeralValueA;
        private BigInteger m_publicEphemeralValueB;
        private BigInteger m_secretEphemeralValueB;
        private BigInteger verifier;

        public SecureRemotePassword(bool isServer, SecureRemotePassword.SRPParameters parameters)
        {
            this.m_srpParams = parameters;
            this.m_isServer = isServer;
        }

        public SecureRemotePassword(bool isServer)
            : this(isServer, SecureRemotePassword.SRPParameters.Defaults)
        {
        }

        public SecureRemotePassword(string username, BigInteger credentials, bool isServer,
            SecureRemotePassword.SRPParameters parameters)
        {
            if (!parameters.CaseSensitive)
                username = username.ToUpper();
            this.m_srpParams = parameters;
            this.m_isServer = isServer;
            this.Username = username;
            this.Credentials = credentials;
        }

        public SecureRemotePassword(string username, BigInteger credentials, bool isServer)
            : this(username, credentials, isServer, SecureRemotePassword.SRPParameters.Defaults)
        {
        }

        /// <summary>
        /// Make an SRP for user authentication. You use something like this when your
        /// verifier and salt are stored in a database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="verifier"></param>
        /// <param name="salt"></param>
        /// <param name="parameters"></param>
        public SecureRemotePassword(string username, BigInteger verifier, BigInteger salt,
            SecureRemotePassword.SRPParameters parameters)
        {
            if (!parameters.CaseSensitive)
                username = username.ToUpper();
            this.m_srpParams = parameters;
            this.m_isServer = true;
            this.Username = username;
            this.Verifier = verifier;
            this.m_salt = salt;
        }

        public SecureRemotePassword(string username, BigInteger verifier, BigInteger salt)
            : this(username, verifier, salt, SecureRemotePassword.SRPParameters.Defaults)
        {
        }

        public SecureRemotePassword.SRPParameters Parameters
        {
            get { return this.m_srpParams; }
            set { this.m_srpParams = value; }
        }

        /// <summary>
        /// Are we the server? This should be set before calculation commences.
        /// </summary>
        public bool IsServer
        {
            get { return this.m_isServer; }
        }

        /// <summary>
        /// Correct username. This should be set before calculations happen.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Referred to as 'M' in the documentation. This is used for authentication.
        /// 
        /// The client sends this value to the server and the server calculates it locally to verify it.
        /// The same then happens with ServerSessionKeyProof. Note ClientSessionKeyProof should come first.
        /// </summary>
        public BigInteger ClientSessionKeyProof
        {
            get
            {
                return this.Hash(
                    (HashUtilities.HashDataBroker) (this.Hash((HashUtilities.HashDataBroker) this.Modulus) ^
                                                    this.Hash((HashUtilities.HashDataBroker) this.Generator)),
                    (HashUtilities.HashDataBroker) this.Hash((HashUtilities.HashDataBroker) this.Username),
                    (HashUtilities.HashDataBroker) this.Salt, (HashUtilities.HashDataBroker) this.PublicEphemeralValueA,
                    (HashUtilities.HashDataBroker) this.PublicEphemeralValueB,
                    (HashUtilities.HashDataBroker) this.SessionKey);
            }
        }

        /// <summary>
        /// The server sends this to the client as proof it has the same session key as the client.
        /// The client will calculate this locally to verify.
        /// </summary>
        public BigInteger ServerSessionKeyProof
        {
            get
            {
                return this.Hash((HashUtilities.HashDataBroker) this.PublicEphemeralValueA,
                    (HashUtilities.HashDataBroker) this.ClientSessionKeyProof,
                    (HashUtilities.HashDataBroker) this.SessionKey);
            }
        }

        /// <summary>Generate a random number of maximal size</summary>
        /// <returns></returns>
        public static BigInteger RandomNumber()
        {
            return SecureRemotePassword.RandomNumber(32U);
        }

        public BigInteger Modulus
        {
            get { return this.Parameters.Modulus; }
        }

        public BigInteger Generator
        {
            get { return this.Parameters.Generator; }
        }

        /// <summary>
        /// 'k' in the spec. In SRP-6a k = H(N, g). Older versions have k = 3.
        /// </summary>
        public BigInteger Multiplier
        {
            get
            {
                if (this.Parameters.AlgorithmVersion == SecureRemotePassword.SRPParameters.SRPVersion.SRP6)
                    return (BigInteger) 3;
                return this.Hash((HashUtilities.HashDataBroker) this.Modulus,
                    (HashUtilities.HashDataBroker) this.Generator);
            }
        }

        public BigInteger Credentials { get; set; }

        /// <summary>
        /// Salt for credentials hash. You can bind this to the users'
        /// account or use the automatically generated random salt.
        /// </summary>
        public BigInteger Salt
        {
            set { this.m_salt = value; }
            get
            {
                if (this.m_salt == (BigInteger) null)
                {
                    if (!this.IsServer)
                        throw new Exception("Unknown salt! This should be set by the server.");
                    this.m_salt = SecureRemotePassword.RandomNumber();
                }

                return this.m_salt;
            }
        }

        /// <summary>
        /// 'x' in the spec. Note that this is slightly different - the spec says
        /// x = H(s, p) whereas here x = H(s, H(I, p)), which is the implementation in the demo.
        /// </summary>
        public BigInteger CredentialsHash
        {
            get
            {
                if (this.m_credentialsHash == (BigInteger) null)
                    this.m_credentialsHash = this.Hash((HashUtilities.HashDataBroker) this.Salt,
                        (HashUtilities.HashDataBroker) this.Credentials);
                return this.m_credentialsHash;
            }
        }

        /// <summary>
        /// 'A' in the spec. A = g^a, generated by client and sent to the server
        /// </summary>
        public BigInteger PublicEphemeralValueA
        {
            get
            {
                if (!this.IsServer && this.m_publicEphemeralValueA == (BigInteger) null)
                    this.m_publicEphemeralValueA = this.Generator.ModPow(this.m_secretEphemeralValueA, this.Modulus);
                return this.m_publicEphemeralValueA;
            }
            set
            {
                if (!this.IsServer)
                    throw new Exception("Attempt by SRP client to set A. This is generated.");
                this.m_publicEphemeralValueA = value;
                this.m_publicEphemeralValueA %= this.Modulus;
                if (this.m_publicEphemeralValueA < 0)
                    this.m_publicEphemeralValueA += this.Modulus;
                if (this.m_publicEphemeralValueA == 0)
                    throw new InvalidDataException("A cannot be 0 mod N!");
            }
        }

        /// <summary>
        /// 'B' in the spec. B = kv + g^b, generated by the server and sent to the client
        /// </summary>
        public BigInteger PublicEphemeralValueB
        {
            get
            {
                if (this.IsServer && this.m_publicEphemeralValueB == (BigInteger) null)
                {
                    this.m_secretEphemeralValueB = SecureRemotePassword.RandomNumber();
                    this.m_publicEphemeralValueB = this.Multiplier * this.Verifier +
                                                   this.Generator.ModPow(this.m_secretEphemeralValueB, this.Modulus);
                    this.m_publicEphemeralValueB %= this.Modulus;
                    if (this.m_publicEphemeralValueB < 0)
                        this.m_publicEphemeralValueB += this.Modulus;
                }

                return this.m_publicEphemeralValueB;
            }
            set
            {
                if (this.IsServer)
                    throw new Exception("Attempt by SRP server to set B. This is generated.");
                this.m_publicEphemeralValueB = value;
                this.m_publicEphemeralValueB %= this.Modulus;
                if (this.m_publicEphemeralValueB < 0)
                    this.m_publicEphemeralValueB += this.Modulus;
                if (this.m_publicEphemeralValueB == 0)
                    throw new InvalidDataException("B cannot be 0 mod N!");
            }
        }

        /// <summary>u in the spec. Generated by both server and client.</summary>
        public BigInteger ScramblingParameter
        {
            get
            {
                return this.Hash((HashUtilities.HashDataBroker) this.PublicEphemeralValueA,
                    (HashUtilities.HashDataBroker) this.PublicEphemeralValueB);
            }
        }

        /// <summary>
        /// This is the session key used for encryption later.
        /// 'K' in the spec. Note that this is different to 'k' (Multiplier)
        /// </summary>
        public BigInteger SessionKeyRaw
        {
            get
            {
                if (this.m_sessionKey == (BigInteger) null)
                {
                    BigInteger bigInteger;
                    if (this.IsServer)
                    {
                        if (this.m_publicEphemeralValueA == (BigInteger) null)
                            return (BigInteger) null;
                        bigInteger =
                            (this.Verifier.ModPow(this.ScramblingParameter, this.Modulus) * this.PublicEphemeralValueA %
                             this.Modulus).ModPow(this.m_secretEphemeralValueB, this.Modulus);
                    }
                    else
                    {
                        bigInteger =
                            (this.PublicEphemeralValueB -
                             this.Multiplier * this.Generator.ModPow(this.CredentialsHash, this.Modulus))
                            .ModPow(this.m_secretEphemeralValueA + this.ScramblingParameter * this.CredentialsHash,
                                this.Modulus);
                        if (bigInteger < 0)
                            bigInteger += this.Modulus;
                    }

                    this.m_sessionKey = bigInteger;
                }

                return this.m_sessionKey;
            }
        }

        /// <summary>
        /// V in the spec.
        /// v = g^x (mod N)
        /// 
        /// This only makes sense for servers.
        /// </summary>
        public BigInteger Verifier
        {
            get
            {
                if (this.verifier == (BigInteger) null)
                    this.verifier = this.Generator.ModPow(this.CredentialsHash, this.Modulus);
                if (this.verifier < 0)
                    this.verifier += this.Modulus;
                return this.verifier;
            }
            set { this.verifier = value; }
        }

        public BigInteger SessionKey
        {
            get
            {
                byte[] bytes1 = this.SessionKeyRaw.GetBytes(32);
                byte[] numArray = new byte[16];
                for (int index = 0; index < numArray.Length; ++index)
                    numArray[index] = bytes1[2 * index];
                byte[] bytes2 = this.Hash((HashUtilities.HashDataBroker) numArray).GetBytes(20);
                for (int index = 0; index < numArray.Length; ++index)
                    numArray[index] = bytes1[2 * index + 1];
                byte[] bytes3 = this.Hash((HashUtilities.HashDataBroker) numArray).GetBytes(20);
                byte[] inData = new byte[40];
                for (int index = 0; index < inData.Length; ++index)
                    inData[index] = index % 2 == 0 ? bytes2[index / 2] : bytes3[index / 2];
                return new BigInteger(inData);
            }
        }

        public BigInteger Hash(params HashUtilities.HashDataBroker[] brokers)
        {
            return HashUtilities.HashToBigInteger(SecureRemotePassword.SRPParameters.Hash, brokers);
        }

        /// <summary>Generate a random number of a specified size</summary>
        /// <param name="size">Maximum size in bytes of the random number</param>
        /// <returns></returns>
        public static BigInteger RandomNumber(uint size)
        {
            byte[] numArray = new byte[size];
            SecureRemotePassword.SRPParameters.RandomGenerator.GetBytes(numArray);
            if (numArray[0] == (byte) 0)
                numArray[0] = (byte) 1;
            return new BigInteger(numArray);
        }

        public bool IsClientProofValid(BigInteger client_proof)
        {
            return this.ClientSessionKeyProof == client_proof;
        }

        public bool IsServerProofValid(BigInteger server_proof)
        {
            return server_proof == this.ServerSessionKeyProof;
        }

        public string InternalsToString()
        {
            string str1 = string.Format("SRP {0} Internals:\n", this.IsServer ? (object) "server" : (object) "client") +
                          string.Format("G      = {0}\n", (object) this.Generator.ToHexString()) +
                          string.Format("K      = {0}\n", (object) this.Multiplier.ToHexString()) +
                          string.Format("N      = {0}\n", (object) this.Modulus.ToHexString()) +
                          string.Format("I      = '{0}'\n", (object) this.Credentials) +
                          string.Format("Hash(I)= {0}\n",
                              (object) this.Hash((HashUtilities.HashDataBroker) this.Credentials).ToHexString()) +
                          string.Format("X      = {0}\n", (object) this.CredentialsHash.ToHexString()) +
                          string.Format("V      = {0}\n", (object) this.Verifier.ToHexString());
            if (this.m_salt != (BigInteger) null)
                str1 += string.Format("Salt   = {0}\n", (object) this.Salt.ToHexString());
            if ((BigInteger) null != this.m_publicEphemeralValueA && (BigInteger) null != this.m_publicEphemeralValueB)
                str1 = str1 + string.Format("u      = {0}\n", (object) this.ScramblingParameter.ToHexString()) +
                       string.Format("h(A)   = {0}\n",
                           (object) this.Hash((HashUtilities.HashDataBroker) this.PublicEphemeralValueA)
                               .ToHexString()) + string.Format("h(B)   = {0}\n",
                           (object) this.Hash((HashUtilities.HashDataBroker) this.PublicEphemeralValueB.GetBytes())
                               .ToHexString());
            if (!this.IsServer || this.PublicEphemeralValueA != (BigInteger) null)
                str1 += string.Format("A      = {0}\n", (object) this.PublicEphemeralValueA.ToHexString());
            if (this.IsServer || this.PublicEphemeralValueB != (BigInteger) null)
            {
                string str2 = str1 + string.Format("B      = {0}\n", (object) this.PublicEphemeralValueB.ToHexString());
                BigInteger bigInteger1 = this.Multiplier * this.Generator.ModPow(this.CredentialsHash, this.Modulus);
                string str3 = str2 + string.Format("kg^x   = {0}\n", (object) bigInteger1.ToHexString());
                BigInteger bigInteger2 = this.PublicEphemeralValueB - bigInteger1 % this.Modulus;
                if (bigInteger2 < 0)
                    bigInteger2 += this.Modulus;
                str1 = str3 + string.Format("B-kg^x = {0}\n", (object) bigInteger2.ToHexString());
            }

            string str4;
            try
            {
                str4 = str1 + string.Format("S.key  = {0}\n", (object) this.SessionKey.ToHexString());
            }
            catch
            {
                str4 = str1 + "S.key  = empty\n";
            }

            return str4;
        }

        public static void Test()
        {
            SecureRemotePassword.SRPParameters srpParameters = new SecureRemotePassword.SRPParameters();
            BigInteger bigInteger = HashUtilities.HashToBigInteger(SecureRemotePassword.SRPParameters.Hash,
                (HashUtilities.HashDataBroker) "USER:PASSWORD");
            SecureRemotePassword secureRemotePassword1 =
                new SecureRemotePassword("USER", bigInteger, true, SecureRemotePassword.SRPParameters.Defaults);
            SecureRemotePassword secureRemotePassword2 = new SecureRemotePassword("USER", bigInteger, false,
                SecureRemotePassword.SRPParameters.Defaults);
            Console.WriteLine("Client sending A = {0}",
                (object) secureRemotePassword2.PublicEphemeralValueA.ToHexString());
            secureRemotePassword1.PublicEphemeralValueA = secureRemotePassword2.PublicEphemeralValueA;
            Console.WriteLine("Server sending salt = {0}", (object) secureRemotePassword1.Salt.ToHexString());
            Console.WriteLine("Server sending B = {0}",
                (object) secureRemotePassword1.PublicEphemeralValueB.ToHexString());
            secureRemotePassword2.Salt = secureRemotePassword1.Salt;
            secureRemotePassword2.PublicEphemeralValueB = secureRemotePassword1.PublicEphemeralValueB;
            Console.WriteLine("Server's session key = {0}", (object) secureRemotePassword1.SessionKey.ToHexString());
            Console.WriteLine("Client's session key = {0}", (object) secureRemotePassword2.SessionKey.ToHexString());
            Console.WriteLine("\nServer key == client key {0}",
                (object) (secureRemotePassword1.SessionKey == secureRemotePassword2.SessionKey));
            Console.WriteLine("Client proof valid: {0}",
                (object) secureRemotePassword1.IsClientProofValid(secureRemotePassword2.ClientSessionKeyProof));
            Console.WriteLine("Server proof valid: {0}",
                (object) secureRemotePassword2.IsServerProofValid(secureRemotePassword1.ServerSessionKeyProof));
        }

        /// <summary>
        /// Generates a hash for an account's credentials (username:password) based on the SRP hashing method,
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="password">the password</param>
        /// <returns>a byte array of the resulting hash</returns>
        public static byte[] GenerateCredentialsHash(string username, string password)
        {
            byte[] hash = SecureRemotePassword.SRPParameters.Hash.ComputeHash(
                WCellConstants.DefaultEncoding.GetBytes(string.Format("{0}:{1}", (object) username.ToUpper(),
                    (object) password.ToUpper())));
            if (hash.Length > 20)
                throw new CryptographicException("SHA-1 hash too long - " + (object) hash.Length +
                                                 " bytes, should be 20!");
            return hash;
        }

        public SecureRemotePassword(SerializationInfo info, StreamingContext context)
        {
            this.SerializationInfo = info;
        }

        public SerializationInfo SerializationInfo { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.SerializationInfo == null)
                return;
            foreach (SerializationEntry serializationEntry in this.SerializationInfo)
                info.AddValue(serializationEntry.Name, serializationEntry.Value);
        }

        [Serializable]
        public class SRPParameters
        {
            /// <summary>Random number generator for this instance.</summary>
            public static RandomNumberGenerator
                RandomGenerator = (RandomNumberGenerator) new RNGCryptoServiceProvider();

            /// <summary>Hashing function for the instance.</summary>
            /// <remarks>MD5 or other SHA hashes are usable, though SHA1 is more standard for SRP.</remarks>
            [NonSerialized] public static readonly HashAlgorithm Hash = (HashAlgorithm) new SHA1Managed();

            /// <summary>
            /// 'g' in the spec. This number must be a generator in the finite field Modulus.
            /// </summary>
            private static readonly BigInteger s_generator = new BigInteger(7L);

            private static readonly BigInteger s_modulus =
                new BigInteger("B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89", 16);

            /// <summary>Version of this instance.</summary>
            public SecureRemotePassword.SRPParameters.SRPVersion AlgorithmVersion =
                SecureRemotePassword.SRPParameters.SRPVersion.SRP6;

            public bool CaseSensitive = false;

            /// <summary>Maximum length of crypto keys in bytes.</summary>
            /// <remarks>You might get unlucky and have much shorter keys - this should be checked and keys recalculated.</remarks>
            public const uint KeyLength = 32;

            /// <summary>
            /// All operations are mod this number. It should be a large prime.
            /// </summary>
            /// <remarks>Referred to as 'N' in the spec.</remarks>
            /// <remarks>Defaults to 128 bits.</remarks>
            public BigInteger Modulus
            {
                get { return SecureRemotePassword.SRPParameters.s_modulus; }
            }

            public BigInteger Generator
            {
                get { return SecureRemotePassword.SRPParameters.s_generator; }
            }

            public static SecureRemotePassword.SRPParameters Defaults
            {
                get { return new SecureRemotePassword.SRPParameters(); }
            }

            /// <summary>
            /// Algorithm version. Consult specification for differences.
            /// </summary>
            public enum SRPVersion
            {
                SRP6,
                SRP6a,
            }
        }
    }
}