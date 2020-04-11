using System;
using System.Security.Cryptography;
using WCell.Core.Network;
using WCell.Intercommunication.DataTypes;

namespace WCell.Core.Cryptography
{
    /// <summary>
    /// Handles writing the server's authentication proof and validating the client's proof.
    /// </summary>
    public class Authenticator
    {
        private readonly SecureRemotePassword m_srp;

        /// <summary>Default constructor.</summary>
        /// <param name="srp">the SRP instance for our current session</param>
        public Authenticator(SecureRemotePassword srp)
        {
            this.m_srp = srp;
        }

        /// <summary>The SRP instance we're using</summary>
        public SecureRemotePassword SRP
        {
            get { return this.m_srp; }
        }

        public byte[] ReconnectProof { get; set; }

        /// <summary>Writes the server's challenge.</summary>
        /// <param name="packet">the packet to write to</param>
        public void WriteServerChallenge(PrimitiveWriter packet)
        {
            packet.WriteBigInt(this.m_srp.PublicEphemeralValueB, 32);
            packet.WriteBigIntLength(this.m_srp.Generator, 1);
            packet.WriteBigIntLength(this.m_srp.Modulus, 32);
            packet.WriteBigInt(this.m_srp.Salt);
        }

        /// <summary>Checks if the client's proof matches our proof.</summary>
        /// <param name="packet">the packet to read from</param>
        /// <returns>true if the client proof matches; false otherwise</returns>
        public bool IsClientProofValid(PacketIn packet)
        {
            this.m_srp.PublicEphemeralValueA = packet.ReadBigInteger(32);
            BigInteger client_proof = packet.ReadBigInteger(20);
            packet.ReadBytes(20);
            byte num1 = packet.ReadByte();
            for (int index = 0; index < (int) num1; ++index)
            {
                packet.ReadUInt16();
                packet.ReadUInt32();
                packet.ReadBytes(4);
                packet.ReadBytes(20);
            }

            byte num2 = packet.ReadByte();
            if (((int) num2 & 1) != 0)
            {
                packet.ReadBytes(16);
                packet.ReadBytes(20);
            }

            if (((int) num2 & 2) != 0)
                packet.ReadBytes(20);
            if (((int) num2 & 4) != 0)
            {
                byte num3 = packet.ReadByte();
                packet.ReadBytes((int) num3);
            }

            return this.m_srp.IsClientProofValid(client_proof);
        }

        /// <summary>Writes the server's proof.</summary>
        /// <param name="packet">the packet to write to</param>
        public void WriteServerProof(PrimitiveWriter packet)
        {
            packet.WriteBigInt(this.m_srp.ServerSessionKeyProof, 20);
        }

        public void WriteReconnectChallenge(PrimitiveWriter packet)
        {
            this.ReconnectProof = new byte[16];
            new Random(Environment.TickCount).NextBytes(this.ReconnectProof);
            packet.Write(this.ReconnectProof);
            packet.Write(0L);
            packet.Write(0L);
        }

        public bool IsReconnectProofValid(PacketIn packet, AuthenticationInfo authInfo)
        {
            byte[] numArray1 = packet.ReadBytes(16);
            byte[] numArray2 = packet.ReadBytes(20);
            packet.ReadBytes(20);
            byte[] bytes = WCellConstants.DefaultEncoding.GetBytes(this.m_srp.Username);
            SHA1Managed shA1Managed = new SHA1Managed();
            shA1Managed.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            shA1Managed.TransformBlock(numArray1, 0, numArray1.Length, numArray1, 0);
            shA1Managed.TransformBlock(this.ReconnectProof, 0, this.ReconnectProof.Length, this.ReconnectProof, 0);
            shA1Managed.TransformBlock(authInfo.SessionKey, 0, authInfo.SessionKey.Length, authInfo.SessionKey, 0);
            byte[] numArray3 = shA1Managed.TransformFinalBlock(new byte[0], 0, 0);
            for (int index = 0; index < 20; ++index)
            {
                if ((int) numArray2[index] != (int) numArray3[index])
                    return false;
            }

            return true;
        }
    }
}