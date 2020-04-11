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
    private SRPParameters m_srpParams = SRPParameters.Defaults;
    private readonly BigInteger m_secretEphemeralValueA = RandomNumber();

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

    public SecureRemotePassword(bool isServer, SRPParameters parameters)
    {
      m_srpParams = parameters;
      m_isServer = isServer;
    }

    public SecureRemotePassword(bool isServer)
      : this(isServer, SRPParameters.Defaults)
    {
    }

    public SecureRemotePassword(string username, BigInteger credentials, bool isServer,
      SRPParameters parameters)
    {
      if(!parameters.CaseSensitive)
        username = username.ToUpper();
      m_srpParams = parameters;
      m_isServer = isServer;
      Username = username;
      Credentials = credentials;
    }

    public SecureRemotePassword(string username, BigInteger credentials, bool isServer)
      : this(username, credentials, isServer, SRPParameters.Defaults)
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
      SRPParameters parameters)
    {
      if(!parameters.CaseSensitive)
        username = username.ToUpper();
      m_srpParams = parameters;
      m_isServer = true;
      Username = username;
      Verifier = verifier;
      m_salt = salt;
    }

    public SecureRemotePassword(string username, BigInteger verifier, BigInteger salt)
      : this(username, verifier, salt, SRPParameters.Defaults)
    {
    }

    public SRPParameters Parameters
    {
      get { return m_srpParams; }
      set { m_srpParams = value; }
    }

    /// <summary>
    /// Are we the server? This should be set before calculation commences.
    /// </summary>
    public bool IsServer
    {
      get { return m_isServer; }
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
        return Hash(
          (HashUtilities.HashDataBroker) (Hash((HashUtilities.HashDataBroker) Modulus) ^
                                          Hash((HashUtilities.HashDataBroker) Generator)),
          (HashUtilities.HashDataBroker) Hash((HashUtilities.HashDataBroker) Username),
          (HashUtilities.HashDataBroker) Salt, (HashUtilities.HashDataBroker) PublicEphemeralValueA,
          (HashUtilities.HashDataBroker) PublicEphemeralValueB,
          (HashUtilities.HashDataBroker) SessionKey);
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
        return Hash((HashUtilities.HashDataBroker) PublicEphemeralValueA,
          (HashUtilities.HashDataBroker) ClientSessionKeyProof,
          (HashUtilities.HashDataBroker) SessionKey);
      }
    }

    /// <summary>Generate a random number of maximal size</summary>
    /// <returns></returns>
    public static BigInteger RandomNumber()
    {
      return RandomNumber(32U);
    }

    public BigInteger Modulus
    {
      get { return Parameters.Modulus; }
    }

    public BigInteger Generator
    {
      get { return Parameters.Generator; }
    }

    /// <summary>
    /// 'k' in the spec. In SRP-6a k = H(N, g). Older versions have k = 3.
    /// </summary>
    public BigInteger Multiplier
    {
      get
      {
        if(Parameters.AlgorithmVersion == SRPParameters.SRPVersion.SRP6)
          return (BigInteger) 3;
        return Hash((HashUtilities.HashDataBroker) Modulus,
          (HashUtilities.HashDataBroker) Generator);
      }
    }

    public BigInteger Credentials { get; set; }

    /// <summary>
    /// Salt for credentials hash. You can bind this to the users'
    /// account or use the automatically generated random salt.
    /// </summary>
    public BigInteger Salt
    {
      set { m_salt = value; }
      get
      {
        if(m_salt == null)
        {
          if(!IsServer)
            throw new Exception("Unknown salt! This should be set by the server.");
          m_salt = RandomNumber();
        }

        return m_salt;
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
        if(m_credentialsHash == null)
          m_credentialsHash = Hash((HashUtilities.HashDataBroker) Salt,
            (HashUtilities.HashDataBroker) Credentials);
        return m_credentialsHash;
      }
    }

    /// <summary>
    /// 'A' in the spec. A = g^a, generated by client and sent to the server
    /// </summary>
    public BigInteger PublicEphemeralValueA
    {
      get
      {
        if(!IsServer && m_publicEphemeralValueA == null)
          m_publicEphemeralValueA = Generator.ModPow(m_secretEphemeralValueA, Modulus);
        return m_publicEphemeralValueA;
      }
      set
      {
        if(!IsServer)
          throw new Exception("Attempt by SRP client to set A. This is generated.");
        m_publicEphemeralValueA = value;
        m_publicEphemeralValueA %= Modulus;
        if(m_publicEphemeralValueA < 0)
          m_publicEphemeralValueA += Modulus;
        if(m_publicEphemeralValueA == 0)
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
        if(IsServer && m_publicEphemeralValueB == null)
        {
          m_secretEphemeralValueB = RandomNumber();
          m_publicEphemeralValueB = Multiplier * Verifier +
                                    Generator.ModPow(m_secretEphemeralValueB, Modulus);
          m_publicEphemeralValueB %= Modulus;
          if(m_publicEphemeralValueB < 0)
            m_publicEphemeralValueB += Modulus;
        }

        return m_publicEphemeralValueB;
      }
      set
      {
        if(IsServer)
          throw new Exception("Attempt by SRP server to set B. This is generated.");
        m_publicEphemeralValueB = value;
        m_publicEphemeralValueB %= Modulus;
        if(m_publicEphemeralValueB < 0)
          m_publicEphemeralValueB += Modulus;
        if(m_publicEphemeralValueB == 0)
          throw new InvalidDataException("B cannot be 0 mod N!");
      }
    }

    /// <summary>u in the spec. Generated by both server and client.</summary>
    public BigInteger ScramblingParameter
    {
      get
      {
        return Hash((HashUtilities.HashDataBroker) PublicEphemeralValueA,
          (HashUtilities.HashDataBroker) PublicEphemeralValueB);
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
        if(m_sessionKey == null)
        {
          BigInteger bigInteger;
          if(IsServer)
          {
            if(m_publicEphemeralValueA == null)
              return null;
            bigInteger =
              (Verifier.ModPow(ScramblingParameter, Modulus) * PublicEphemeralValueA %
               Modulus).ModPow(m_secretEphemeralValueB, Modulus);
          }
          else
          {
            bigInteger =
              (PublicEphemeralValueB -
               Multiplier * Generator.ModPow(CredentialsHash, Modulus))
              .ModPow(m_secretEphemeralValueA + ScramblingParameter * CredentialsHash,
                Modulus);
            if(bigInteger < 0)
              bigInteger += Modulus;
          }

          m_sessionKey = bigInteger;
        }

        return m_sessionKey;
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
        if(verifier == null)
          verifier = Generator.ModPow(CredentialsHash, Modulus);
        if(verifier < 0)
          verifier += Modulus;
        return verifier;
      }
      set { verifier = value; }
    }

    public BigInteger SessionKey
    {
      get
      {
        byte[] bytes1 = SessionKeyRaw.GetBytes(32);
        byte[] numArray = new byte[16];
        for(int index = 0; index < numArray.Length; ++index)
          numArray[index] = bytes1[2 * index];
        byte[] bytes2 = Hash((HashUtilities.HashDataBroker) numArray).GetBytes(20);
        for(int index = 0; index < numArray.Length; ++index)
          numArray[index] = bytes1[2 * index + 1];
        byte[] bytes3 = Hash((HashUtilities.HashDataBroker) numArray).GetBytes(20);
        byte[] inData = new byte[40];
        for(int index = 0; index < inData.Length; ++index)
          inData[index] = index % 2 == 0 ? bytes2[index / 2] : bytes3[index / 2];
        return new BigInteger(inData);
      }
    }

    public BigInteger Hash(params HashUtilities.HashDataBroker[] brokers)
    {
      return HashUtilities.HashToBigInteger(SRPParameters.Hash, brokers);
    }

    /// <summary>Generate a random number of a specified size</summary>
    /// <param name="size">Maximum size in bytes of the random number</param>
    /// <returns></returns>
    public static BigInteger RandomNumber(uint size)
    {
      byte[] numArray = new byte[size];
      SRPParameters.RandomGenerator.GetBytes(numArray);
      if(numArray[0] == 0)
        numArray[0] = 1;
      return new BigInteger(numArray);
    }

    public bool IsClientProofValid(BigInteger client_proof)
    {
      return ClientSessionKeyProof == client_proof;
    }

    public bool IsServerProofValid(BigInteger server_proof)
    {
      return server_proof == ServerSessionKeyProof;
    }

    public string InternalsToString()
    {
      string str1 = string.Format("SRP {0} Internals:\n", IsServer ? "server" : "client") +
                    string.Format("G      = {0}\n", Generator.ToHexString()) +
                    string.Format("K      = {0}\n", Multiplier.ToHexString()) +
                    string.Format("N      = {0}\n", Modulus.ToHexString()) +
                    string.Format("I      = '{0}'\n", Credentials) +
                    string.Format("Hash(I)= {0}\n",
                      Hash((HashUtilities.HashDataBroker) Credentials).ToHexString()) +
                    string.Format("X      = {0}\n", CredentialsHash.ToHexString()) +
                    string.Format("V      = {0}\n", Verifier.ToHexString());
      if(m_salt != null)
        str1 += string.Format("Salt   = {0}\n", Salt.ToHexString());
      if(null != m_publicEphemeralValueA && null != m_publicEphemeralValueB)
        str1 = str1 + string.Format("u      = {0}\n", ScramblingParameter.ToHexString()) +
               string.Format("h(A)   = {0}\n",
                 Hash((HashUtilities.HashDataBroker) PublicEphemeralValueA)
                   .ToHexString()) + string.Format("h(B)   = {0}\n",
                 Hash((HashUtilities.HashDataBroker) PublicEphemeralValueB.GetBytes())
                   .ToHexString());
      if(!IsServer || PublicEphemeralValueA != null)
        str1 += string.Format("A      = {0}\n", PublicEphemeralValueA.ToHexString());
      if(IsServer || PublicEphemeralValueB != null)
      {
        string str2 = str1 + string.Format("B      = {0}\n", PublicEphemeralValueB.ToHexString());
        BigInteger bigInteger1 = Multiplier * Generator.ModPow(CredentialsHash, Modulus);
        string str3 = str2 + string.Format("kg^x   = {0}\n", bigInteger1.ToHexString());
        BigInteger bigInteger2 = PublicEphemeralValueB - bigInteger1 % Modulus;
        if(bigInteger2 < 0)
          bigInteger2 += Modulus;
        str1 = str3 + string.Format("B-kg^x = {0}\n", bigInteger2.ToHexString());
      }

      string str4;
      try
      {
        str4 = str1 + string.Format("S.key  = {0}\n", SessionKey.ToHexString());
      }
      catch
      {
        str4 = str1 + "S.key  = empty\n";
      }

      return str4;
    }

    public static void Test()
    {
      SRPParameters srpParameters = new SRPParameters();
      BigInteger bigInteger = HashUtilities.HashToBigInteger(SRPParameters.Hash,
        (HashUtilities.HashDataBroker) "USER:PASSWORD");
      SecureRemotePassword secureRemotePassword1 =
        new SecureRemotePassword("USER", bigInteger, true, SRPParameters.Defaults);
      SecureRemotePassword secureRemotePassword2 = new SecureRemotePassword("USER", bigInteger, false,
        SRPParameters.Defaults);
      Console.WriteLine("Client sending A = {0}",
        secureRemotePassword2.PublicEphemeralValueA.ToHexString());
      secureRemotePassword1.PublicEphemeralValueA = secureRemotePassword2.PublicEphemeralValueA;
      Console.WriteLine("Server sending salt = {0}", secureRemotePassword1.Salt.ToHexString());
      Console.WriteLine("Server sending B = {0}",
        secureRemotePassword1.PublicEphemeralValueB.ToHexString());
      secureRemotePassword2.Salt = secureRemotePassword1.Salt;
      secureRemotePassword2.PublicEphemeralValueB = secureRemotePassword1.PublicEphemeralValueB;
      Console.WriteLine("Server's session key = {0}", secureRemotePassword1.SessionKey.ToHexString());
      Console.WriteLine("Client's session key = {0}", secureRemotePassword2.SessionKey.ToHexString());
      Console.WriteLine("\nServer key == client key {0}",
        secureRemotePassword1.SessionKey == secureRemotePassword2.SessionKey);
      Console.WriteLine("Client proof valid: {0}",
        secureRemotePassword1.IsClientProofValid(secureRemotePassword2.ClientSessionKeyProof));
      Console.WriteLine("Server proof valid: {0}",
        secureRemotePassword2.IsServerProofValid(secureRemotePassword1.ServerSessionKeyProof));
    }

    /// <summary>
    /// Generates a hash for an account's credentials (username:password) based on the SRP hashing method,
    /// </summary>
    /// <param name="username">the username</param>
    /// <param name="password">the password</param>
    /// <returns>a byte array of the resulting hash</returns>
    public static byte[] GenerateCredentialsHash(string username, string password)
    {
      byte[] hash = SRPParameters.Hash.ComputeHash(
        WCellConstants.DefaultEncoding.GetBytes(string.Format("{0}:{1}", username.ToUpper(),
          password.ToUpper())));
      if(hash.Length > 20)
        throw new CryptographicException("SHA-1 hash too long - " + hash.Length +
                                         " bytes, should be 20!");
      return hash;
    }

    public SecureRemotePassword(SerializationInfo info, StreamingContext context)
    {
      SerializationInfo = info;
    }

    public SerializationInfo SerializationInfo { get; set; }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if(SerializationInfo == null)
        return;
      foreach(SerializationEntry serializationEntry in SerializationInfo)
        info.AddValue(serializationEntry.Name, serializationEntry.Value);
    }

    [Serializable]
    public class SRPParameters
    {
      /// <summary>Random number generator for this instance.</summary>
      public static RandomNumberGenerator
        RandomGenerator = new RNGCryptoServiceProvider();

      /// <summary>Hashing function for the instance.</summary>
      /// <remarks>MD5 or other SHA hashes are usable, though SHA1 is more standard for SRP.</remarks>
      [NonSerialized]public static readonly HashAlgorithm Hash = new SHA1Managed();

      /// <summary>
      /// 'g' in the spec. This number must be a generator in the finite field Modulus.
      /// </summary>
      private static readonly BigInteger s_generator = new BigInteger(7L);

      private static readonly BigInteger s_modulus =
        new BigInteger("B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89", 16);

      /// <summary>Version of this instance.</summary>
      public SRPVersion AlgorithmVersion =
        SRPVersion.SRP6;

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
        get { return s_modulus; }
      }

      public BigInteger Generator
      {
        get { return s_generator; }
      }

      public static SRPParameters Defaults
      {
        get { return new SRPParameters(); }
      }

      /// <summary>
      /// Algorithm version. Consult specification for differences.
      /// </summary>
      public enum SRPVersion
      {
        SRP6,
        SRP6a
      }
    }
  }
}