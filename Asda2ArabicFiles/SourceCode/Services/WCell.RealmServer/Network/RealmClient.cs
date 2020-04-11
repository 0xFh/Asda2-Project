/*************************************************************************
 *
 *   file		: IRealmClient.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 02:51:11 +0100 (lø, 30 jan 2010) $

 *   revision		: $Rev: 1233 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cell.Core;
using NLog;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Core.Network;
using WCell.RealmServer.Auth.Accounts;
using WCell.RealmServer.Entities;
using WCell.Util.NLog;

namespace WCell.RealmServer.Network
{
    /// <summary>
    /// Represents a client connected to the realm server
    /// </summary>
    public sealed class RealmClient : ClientBase, IRealmClient
    {

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private byte[] m_sessionKey;

        public static readonly List<IRealmClient> EmptyArray = new List<IRealmClient>();

        /// <summary>
        /// The server this client is connected to.
        /// </summary>
        public new RealmServer Server
        {
            get { return (RealmServer)_server; }
        }

        public bool IsGameServerConnection { get; set; }

        /// <summary>
        /// The <see cref="ClientInformation">system information</see> for this client.
        /// </summary>
        public ClientInformation Info { get; set; }

        /// <summary>
        /// The compressed addon data sent by the client.
        /// </summary>
        public byte[] Addons { get; set; }

        /// <summary>
        /// The account on this session.
        /// </summary>
        public RealmAccount Account { get; set; }

        /// <summary>
        /// The <see cref="Character" /> that the client is currently playing.
        /// </summary>
        public Character ActiveCharacter { get; set; }

        /// <summary>
        /// Whether or not this client is currently logging out.
        /// </summary>
        public bool IsOffline { get; set; }

        /// <summary>
        /// Whether or not communication with this client is encrypted.
        /// </summary>
        public bool IsEncrypted
        {
            get { return m_sessionKey != null; }
        }

        /// <summary>
        /// The local system uptime of the client.
        /// </summary>
        public uint ClientTime { get; set; }

        /// <summary>
        /// Connection latency between client and server.
        /// </summary>
        public int Latency { get; set; }

        /// <summary>
        /// The amount of time skipped by the client.
        /// </summary>
        /// <remarks>Deals with the the way we calculate movement delay.</remarks>
        public uint OutOfSyncDelay { get; set; }

        public uint LastClientMoveTime { get; set; }

        /// <summary>
        /// The client tick count.
        /// </summary>
        /// <remarks>It is set by opcodes 912/913, and seems to be a client ping sequence that is
        /// local to the map, and thus it resets to 0 on a map change.  Real usage isn't known.</remarks>
        public uint TickCount { get; set; }

        /// <summary>
        /// The client seed sent by the client during re-authentication.
        /// </summary>
        public uint ClientSeed { get; set; }

        /// <summary>
        /// The authentication message digest received from the client during re-authentication.
        /// </summary>
        public BigInteger ClientDigest { get; set; }

        /// <summary>
        /// Create an realm client for a given server.
        /// </summary>
        /// <param name="server">reference to the parent RealmServer</param>
        public RealmClient(RealmServer server)
            : base(server)
        {
        }

        public bool KnownClientVersion;
        /// <summary>
        /// Pass recieved data into the packet buffer and try to parse.
        /// </summary>
        /// <param name="_remainingLength">number of bytes waiting to be read</param>
        /// <returns>false, if there is a part of a packet still remaining</returns>
        protected override bool OnReceive()
        {
            while (true)
            {

                var packetOfset = _bufferSegment.Offset;

                var packetLength = BitConverter.ToUInt16(_bufferSegment.Buffer.Array, packetOfset + 1);
                if (packetLength == 0)
                    throw new ObjectDisposedException("none");
                if (packetLength > 4096)
                {
                    LogUtil.WarnException("{0} send packet with lenght {1}. HACKER! Remaining length {2}", AccountName, packetLength, _remainingLength);
                    _remainingLength = 0;
                    Disconnect(false);
                    throw new InvalidOperationException("Wrong data from client.");
                }
                if (_remainingLength < packetLength)
                {
                    return true;
                }
                if (!KnownClientVersion)
                {
                    var data = new byte[2];
                    data[0] = _bufferSegment.Buffer.Array[packetOfset + 3];
                    data[1] = _bufferSegment.Buffer.Array[packetOfset + 4];
                    Asda2CryptHelper.XorData(data, 0, 2, Locale.En, Locale.UnEncoded);
                    if (data[1] == 0)
                    {
                        Locale = Locale.En;
                        //eng client
                    }
                    else if (data[1] == 228)
                        Locale = Locale.Ru;
                    else if (data[1] == 116)
                        Locale = Locale.Ar;
                    else
                    {
                        //unknown client
                    }
                    KnownClientVersion = true;
                }
                Asda2CryptHelper.XorData(_bufferSegment.Buffer.Array, packetOfset + 3, packetLength - 4, Locale, Locale.UnEncoded);
                var bs = _bufferSegment;
                var packet = new RealmPacketIn(bs, 7, packetLength - 8, IsGameServerConnection);
                //Console.WriteLine(@"S <- C: {0}", packet.PacketId);

                RealmPacketMgr.Instance.HandlePacket(this, packet);
                _remainingLength -= packetLength;

                var curBs = _bufferSegment;
                _bufferSegment = Buffers.CheckOut();
                if (_remainingLength > 0)
                {
                    //log.Info("remaining length = " + _remainingLength);
                    Array.Copy(curBs.Buffer.Array, curBs.Offset + packetLength, _bufferSegment.Buffer.Array,
                               _bufferSegment.Offset, _remainingLength);
                }
                else
                {
                    return true;
                }
            }
        }

        public Locale Locale { get; set; }


        /// <summary>
        /// Sends the given bytes representing a full packet, to the Client
        /// </summary>
        /// <param name="packet"></param>
        public override void Send(byte[] packet, int offset, int count)
        {
            if (IsOffline)
                return;

            base.Send(packet, offset, count);
        }

        public void Send(RealmPacketOut packet, bool addEndOfPacket = false)
        {
            packet.FinalizeAsda(addEndOfPacket, Locale);
            try
            {
                Send(packet.BufferSegment, packet.TotalLength);
            }
            catch (SocketException)
            {
                Disconnect(false);
            }
        }

        public override string ToString()
        {
            var infoStr = new StringBuilder();

            infoStr.Append(AddrTemp);
            infoStr.Append(" - Account: ");
            infoStr.Append(AccountName);
            if (ActiveCharacter != null)
            {
                infoStr.Append(" - Char: ");
                infoStr.Append(ActiveCharacter.Name);
            }

            return infoStr.ToString();
        }


        public Account AuthAccount { get; set; }

        public void Disconnect(bool disconnectSocketWithDelay)
        {
            if (!IsConnected)
                return;
            //RealmServer.Instance.aUnregisterAccount(Account);
            _server.DisconnectClient(this, true, disconnectSocketWithDelay);
        }

        /// <summary>
        /// The session key for the latest session of this account.
        /// </summary>
        public byte[] SessionKey
        {
            get { return m_sessionKey; }
            set
            {
                m_sessionKey = value;
                m_packetCrypt = new PacketCrypt(value);
            }
        }

        public string AccountName { get; set; }

        public string Password { get; set; }

        #region Packet Encryption/Decryption

        private PacketCrypt m_packetCrypt;
        int encrypt;
        int decryptSeq, decryptUntil = -1;

        /// <summary>
        /// Encrypts the byte array
        /// </summary>
        /// <param name="data">The raw packet data to encrypt</param>
        private void Encrypt(byte[] data, int offset)
        {
            if (Interlocked.Exchange(ref encrypt, 1) == 1)
                log.Error("Encrypt Error");

            m_packetCrypt.Encrypt(data, offset, 4);
            Interlocked.Exchange(ref encrypt, 0);
        }

        //private int GetContentInfo(byte[] inputData, int dataStartOffset, out int packetLength, out RealmServerOpCode opcode)
        //{
        //}

        private byte GetDecryptedByte(byte[] inputData, int baseOffset, int offset)
        {
            if (Interlocked.Exchange(ref decryptSeq, 1) == 1)
                log.Error("Decrypt Error");

            var dataStartOffset = baseOffset + offset;
            if (decryptUntil < offset)
            {
                m_packetCrypt.Decrypt(inputData, dataStartOffset, 1);
            }

            Interlocked.Exchange(ref decryptSeq, 0);

            return inputData[dataStartOffset];
        }

        private int GetDecryptedOpcode(byte[] inputData, int baseOffset, int offset)
        {
            var dataStartOffset = baseOffset + offset;
            if (decryptUntil < offset + 4)
            {
                //if (decryptUntil > offset)		// must not happen
                //{
                //    m_packetCrypt.Decrypt(inputData, baseOffset + decryptUntil, (offset - decryptUntil + 4));
                //}
                m_packetCrypt.Decrypt(inputData, dataStartOffset, 4);
            }
            return BitConverter.ToInt32(inputData, dataStartOffset);
        }

        #endregion
    }
}