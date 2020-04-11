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
            (byte) 194,
            (byte) 179,
            (byte) 114,
            (byte) 60,
            (byte) 198,
            (byte) 174,
            (byte) 217,
            (byte) 181,
            (byte) 52,
            (byte) 60,
            (byte) 83,
            (byte) 238,
            (byte) 47,
            (byte) 67,
            (byte) 103,
            (byte) 206
        };

        /// <summary>
        /// This is the key the client uses to decrypt server packets
        /// This is also the key the server uses to encrypt the packets
        /// </summary>
        private static readonly byte[] ServerEncryptionKey = new byte[16]
        {
            (byte) 204,
            (byte) 152,
            (byte) 174,
            (byte) 4,
            (byte) 232,
            (byte) 151,
            (byte) 234,
            (byte) 202,
            (byte) 18,
            (byte) 221,
            (byte) 192,
            (byte) 147,
            (byte) 66,
            (byte) 145,
            (byte) 83,
            (byte) 87
        };

        private static readonly HMACSHA1 s_decryptClientDataHMAC = new HMACSHA1(PacketCrypt.ServerDecryptionKey);
        private static readonly HMACSHA1 s_encryptServerDataHMAC = new HMACSHA1(PacketCrypt.ServerEncryptionKey);

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
            byte[] hash = PacketCrypt.s_encryptServerDataHMAC.ComputeHash(sessionKey);
            this.decryptClientData = new ARC4(PacketCrypt.s_decryptClientDataHMAC.ComputeHash(sessionKey));
            this.encryptServerData = new ARC4(hash);
            byte[] buffer1 = new byte[1024];
            this.encryptServerData.Process(buffer1, 0, buffer1.Length);
            byte[] buffer2 = new byte[1024];
            this.decryptClientData.Process(buffer2, 0, buffer2.Length);
        }

        public void Decrypt(byte[] data, int start, int count)
        {
            this.decryptClientData.Process(data, start, count);
        }

        public void Encrypt(byte[] data, int start, int count)
        {
            this.encryptServerData.Process(data, start, count);
        }
    }
}