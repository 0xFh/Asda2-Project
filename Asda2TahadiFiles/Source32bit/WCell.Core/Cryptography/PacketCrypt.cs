using System.Security.Cryptography;

namespace WCell.Core.Cryptography
{
  public class PacketCrypt
  {
    /// <summary>
    /// This is the key the client uses to encrypt its packets
    /// This is also the key the server uses to decrypt the packets
    /// </summary>
    private static readonly byte[] ServerDecryptionKey = new byte[16]
    {
      194,
      179,
      114,
      60,
      198,
      174,
      217,
      181,
      52,
      60,
      83,
      238,
      47,
      67,
      103,
      206
    };

    /// <summary>
    /// This is the key the client uses to decrypt server packets
    /// This is also the key the server uses to encrypt the packets
    /// </summary>
    private static readonly byte[] ServerEncryptionKey = new byte[16]
    {
      204,
      152,
      174,
      4,
      232,
      151,
      234,
      202,
      18,
      221,
      192,
      147,
      66,
      145,
      83,
      87
    };

    private static readonly HMACSHA1 s_decryptClientDataHMAC = new HMACSHA1(ServerDecryptionKey);
    private static readonly HMACSHA1 s_encryptServerDataHMAC = new HMACSHA1(ServerEncryptionKey);

    /// <summary>
    /// The amount of bytes to drop from the stream initially.
    /// 
    /// This is to resist the FMS attack.
    /// </summary>
    public const int DropN = 1024;

    /// <summary>Encrypts data sent to the client</summary>
    private readonly ARC4 encryptServerData;

    /// <summary>Decrypts data sent from the client</summary>
    private readonly ARC4 decryptClientData;

    public PacketCrypt(byte[] sessionKey)
    {
      byte[] hash = s_encryptServerDataHMAC.ComputeHash(sessionKey);
      decryptClientData = new ARC4(s_decryptClientDataHMAC.ComputeHash(sessionKey));
      encryptServerData = new ARC4(hash);
      byte[] buffer1 = new byte[1024];
      encryptServerData.Process(buffer1, 0, buffer1.Length);
      byte[] buffer2 = new byte[1024];
      decryptClientData.Process(buffer2, 0, buffer2.Length);
    }

    public void Decrypt(byte[] data, int start, int count)
    {
      decryptClientData.Process(data, start, count);
    }

    public void Encrypt(byte[] data, int start, int count)
    {
      encryptServerData.Process(data, start, count);
    }
  }
}