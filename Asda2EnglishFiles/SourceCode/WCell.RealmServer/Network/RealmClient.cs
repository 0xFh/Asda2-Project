using Cell.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.Util.NLog;

namespace WCell.RealmServer.Network
{
    /// <summary>Represents a client connected to the realm server</summary>
    public sealed class RealmClient : ClientBase, IRealmClient, IClient, IDisposable, IPacketReceiver
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static readonly List<IRealmClient> EmptyArray = new List<IRealmClient>();
        private int decryptUntil = -1;
        private byte[] m_sessionKey;
        public bool KnownClientVersion;
        private PacketCrypt m_packetCrypt;
        private int encrypt;
        private int decryptSeq;

        /// <summary>The server this client is connected to.</summary>
        public WCell.RealmServer.RealmServer Server
        {
            get { return (WCell.RealmServer.RealmServer) this._server; }
        }

        public bool IsGameServerConnection { get; set; }

        /// <summary>
        /// The <see cref="T:WCell.Core.ClientInformation">system information</see> for this client.
        /// </summary>
        public ClientInformation Info { get; set; }

        /// <summary>The compressed addon data sent by the client.</summary>
        public byte[] Addons { get; set; }

        /// <summary>The account on this session.</summary>
        public RealmAccount Account { get; set; }

        /// <summary>
        /// The <see cref="T:WCell.RealmServer.Entities.Character" /> that the client is currently playing.
        /// </summary>
        public Character ActiveCharacter { get; set; }

        /// <summary>Whether or not this client is currently logging out.</summary>
        public bool IsOffline { get; set; }

        /// <summary>
        /// Whether or not communication with this client is encrypted.
        /// </summary>
        public bool IsEncrypted
        {
            get { return this.m_sessionKey != null; }
        }

        /// <summary>The local system uptime of the client.</summary>
        public uint ClientTime { get; set; }

        /// <summary>Connection latency between client and server.</summary>
        public int Latency { get; set; }

        /// <summary>The amount of time skipped by the client.</summary>
        /// <remarks>Deals with the the way we calculate movement delay.</remarks>
        public uint OutOfSyncDelay { get; set; }

        public uint LastClientMoveTime { get; set; }

        /// <summary>The client tick count.</summary>
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

        /// <summary>Create an realm client for a given server.</summary>
        /// <param name="server">reference to the parent RealmServer</param>
        public RealmClient(WCell.RealmServer.RealmServer server)
            : base((ServerBase) server)
        {
        }

        /// <summary>
        /// Pass recieved data into the packet buffer and try to parse.
        /// </summary>
        /// <param name="_remainingLength">number of bytes waiting to be read</param>
        /// <returns>false, if there is a part of a packet still remaining</returns>
        protected override bool OnReceive()
        {
            ushort uint16;
            while (true)
            {
                int offset = this._bufferSegment.Offset;
                uint16 = BitConverter.ToUInt16(this._bufferSegment.Buffer.Array, offset + 1);
                if (uint16 != (ushort) 0)
                {
                    if (uint16 <= (ushort) 4096)
                    {
                        if (this._remainingLength >= (int) uint16)
                        {
                            if (!this.KnownClientVersion)
                            {
                                byte[] buffer = new byte[2]
                                {
                                    this._bufferSegment.Buffer.Array[offset + 3],
                                    this._bufferSegment.Buffer.Array[offset + 4]
                                };
                                Asda2CryptHelper.XorData(buffer, 0, 2L, Locale.Start, Locale.Any);
                                if (buffer[1] == (byte) 0)
                                    this.Locale = Locale.Start;
                                else if (buffer[1] == (byte) 228)
                                    this.Locale = Locale.Ru;
                                this.KnownClientVersion = true;
                            }

                            Asda2CryptHelper.XorData(this._bufferSegment.Buffer.Array, offset + 3,
                                (long) ((int) uint16 - 4), this.Locale, Locale.Any);
                            RealmPacketIn packet = new RealmPacketIn(this._bufferSegment, 7, (int) uint16 - 8,
                                this.IsGameServerConnection);
                            RealmPacketMgr.Instance.HandlePacket((IRealmClient) this, packet);
                            this._remainingLength -= (int) uint16;
                            BufferSegment bufferSegment = this._bufferSegment;
                            this._bufferSegment = ClientBase.Buffers.CheckOut();
                            if (this._remainingLength > 0)
                                Array.Copy((Array) bufferSegment.Buffer.Array, bufferSegment.Offset + (int) uint16,
                                    (Array) this._bufferSegment.Buffer.Array, this._bufferSegment.Offset,
                                    this._remainingLength);
                            else
                                goto label_14;
                        }
                        else
                            goto label_5;
                    }
                    else
                        goto label_3;
                }
                else
                    break;
            }

            throw new ObjectDisposedException("none");
            label_3:
            LogUtil.WarnException("{0} send packet with lenght {1}. HACKER! Remaining length {2}", new object[3]
            {
                (object) this.AccountName,
                (object) uint16,
                (object) this._remainingLength
            });
            this._remainingLength = 0;
            this.Disconnect(false);
            throw new InvalidOperationException("Wrong data from client.");
            label_5:
            return true;
            label_14:
            return true;
        }

        public Locale Locale { get; set; }

        /// <summary>
        /// Sends the given bytes representing a full packet, to the Client
        /// </summary>
        /// <param name="packet"></param>
        public override void Send(byte[] packet, int offset, int count)
        {
            if (this.IsOffline)
                return;
            base.Send(packet, offset, count);
        }

        public void Send(RealmPacketOut packet, bool addEndOfPacket = false)
        {
            packet.FinalizeAsda(addEndOfPacket, this.Locale);
            try
            {
                this.Send(packet.BufferSegment, packet.TotalLength);
            }
            catch (SocketException ex)
            {
                this.Disconnect(false);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.AddrTemp);
            stringBuilder.Append(" - Account: ");
            stringBuilder.Append(this.AccountName);
            if (this.ActiveCharacter != null)
            {
                stringBuilder.Append(" - Char: ");
                stringBuilder.Append(this.ActiveCharacter.Name);
            }

            return stringBuilder.ToString();
        }

        public WCell.RealmServer.Auth.Accounts.Account AuthAccount { get; set; }

        public void Disconnect(bool disconnectSocketWithDelay)
        {
            if (!this.IsConnected)
                return;
            this._server.DisconnectClient((IClient) this, true, disconnectSocketWithDelay);
        }

        /// <summary>
        /// The session key for the latest session of this account.
        /// </summary>
        public byte[] SessionKey
        {
            get { return this.m_sessionKey; }
            set
            {
                this.m_sessionKey = value;
                this.m_packetCrypt = new PacketCrypt(value);
            }
        }

        public string AccountName { get; set; }

        public string Password { get; set; }

        /// <summary>Encrypts the byte array</summary>
        /// <param name="data">The raw packet data to encrypt</param>
        private void Encrypt(byte[] data, int offset)
        {
            if (Interlocked.Exchange(ref this.encrypt, 1) == 1)
                RealmClient.log.Error("Encrypt Error");
            this.m_packetCrypt.Encrypt(data, offset, 4);
            Interlocked.Exchange(ref this.encrypt, 0);
        }

        private byte GetDecryptedByte(byte[] inputData, int baseOffset, int offset)
        {
            if (Interlocked.Exchange(ref this.decryptSeq, 1) == 1)
                RealmClient.log.Error("Decrypt Error");
            int start = baseOffset + offset;
            if (this.decryptUntil < offset)
                this.m_packetCrypt.Decrypt(inputData, start, 1);
            Interlocked.Exchange(ref this.decryptSeq, 0);
            return inputData[start];
        }

        private int GetDecryptedOpcode(byte[] inputData, int baseOffset, int offset)
        {
            int num = baseOffset + offset;
            if (this.decryptUntil < offset + 4)
                this.m_packetCrypt.Decrypt(inputData, num, 4);
            return BitConverter.ToInt32(inputData, num);
        }
    }
}