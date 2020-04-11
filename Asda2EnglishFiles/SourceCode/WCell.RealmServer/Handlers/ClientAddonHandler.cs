using NLog;
using System;
using System.IO;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class ClientAddonHandler
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static readonly byte[] BlizzardPublicKey = new byte[256]
        {
            (byte) 195,
            (byte) 91,
            (byte) 80,
            (byte) 132,
            (byte) 185,
            (byte) 62,
            (byte) 50,
            (byte) 66,
            (byte) 140,
            (byte) 208,
            (byte) 199,
            (byte) 72,
            (byte) 250,
            (byte) 14,
            (byte) 93,
            (byte) 84,
            (byte) 90,
            (byte) 163,
            (byte) 14,
            (byte) 20,
            (byte) 186,
            (byte) 158,
            (byte) 13,
            (byte) 185,
            (byte) 93,
            (byte) 139,
            (byte) 238,
            (byte) 182,
            (byte) 132,
            (byte) 147,
            (byte) 69,
            (byte) 117,
            byte.MaxValue,
            (byte) 49,
            (byte) 254,
            (byte) 47,
            (byte) 100,
            (byte) 63,
            (byte) 61,
            (byte) 109,
            (byte) 7,
            (byte) 217,
            (byte) 68,
            (byte) 155,
            (byte) 64,
            (byte) 133,
            (byte) 89,
            (byte) 52,
            (byte) 78,
            (byte) 16,
            (byte) 225,
            (byte) 231,
            (byte) 67,
            (byte) 105,
            (byte) 239,
            (byte) 124,
            (byte) 22,
            (byte) 252,
            (byte) 180,
            (byte) 237,
            (byte) 27,
            (byte) 149,
            (byte) 40,
            (byte) 168,
            (byte) 35,
            (byte) 118,
            (byte) 81,
            (byte) 49,
            (byte) 87,
            (byte) 48,
            (byte) 43,
            (byte) 121,
            (byte) 8,
            (byte) 80,
            (byte) 16,
            (byte) 28,
            (byte) 74,
            (byte) 26,
            (byte) 44,
            (byte) 200,
            (byte) 139,
            (byte) 143,
            (byte) 5,
            (byte) 45,
            (byte) 34,
            (byte) 61,
            (byte) 219,
            (byte) 90,
            (byte) 36,
            (byte) 122,
            (byte) 15,
            (byte) 19,
            (byte) 80,
            (byte) 55,
            (byte) 143,
            (byte) 90,
            (byte) 204,
            (byte) 158,
            (byte) 4,
            (byte) 68,
            (byte) 14,
            (byte) 135,
            (byte) 1,
            (byte) 212,
            (byte) 163,
            (byte) 21,
            (byte) 148,
            (byte) 22,
            (byte) 52,
            (byte) 198,
            (byte) 194,
            (byte) 195,
            (byte) 251,
            (byte) 73,
            (byte) 254,
            (byte) 225,
            (byte) 249,
            (byte) 218,
            (byte) 140,
            (byte) 80,
            (byte) 60,
            (byte) 190,
            (byte) 44,
            (byte) 187,
            (byte) 87,
            (byte) 237,
            (byte) 70,
            (byte) 185,
            (byte) 173,
            (byte) 139,
            (byte) 198,
            (byte) 223,
            (byte) 14,
            (byte) 214,
            (byte) 15,
            (byte) 190,
            (byte) 128,
            (byte) 179,
            (byte) 139,
            (byte) 30,
            (byte) 119,
            (byte) 207,
            (byte) 173,
            (byte) 34,
            (byte) 207,
            (byte) 183,
            (byte) 75,
            (byte) 207,
            (byte) 251,
            (byte) 240,
            (byte) 107,
            (byte) 17,
            (byte) 69,
            (byte) 45,
            (byte) 122,
            (byte) 129,
            (byte) 24,
            (byte) 242,
            (byte) 146,
            (byte) 126,
            (byte) 152,
            (byte) 86,
            (byte) 93,
            (byte) 94,
            (byte) 105,
            (byte) 114,
            (byte) 10,
            (byte) 13,
            (byte) 3,
            (byte) 10,
            (byte) 133,
            (byte) 162,
            (byte) 133,
            (byte) 156,
            (byte) 203,
            (byte) 251,
            (byte) 86,
            (byte) 110,
            (byte) 143,
            (byte) 68,
            (byte) 187,
            (byte) 143,
            (byte) 2,
            (byte) 34,
            (byte) 104,
            (byte) 99,
            (byte) 151,
            (byte) 188,
            (byte) 133,
            (byte) 186,
            (byte) 168,
            (byte) 247,
            (byte) 181,
            (byte) 64,
            (byte) 104,
            (byte) 60,
            (byte) 119,
            (byte) 134,
            (byte) 111,
            (byte) 75,
            (byte) 215,
            (byte) 136,
            (byte) 202,
            (byte) 138,
            (byte) 215,
            (byte) 206,
            (byte) 54,
            (byte) 240,
            (byte) 69,
            (byte) 110,
            (byte) 213,
            (byte) 100,
            (byte) 121,
            (byte) 15,
            (byte) 23,
            (byte) 252,
            (byte) 100,
            (byte) 221,
            (byte) 16,
            (byte) 111,
            (byte) 243,
            (byte) 245,
            (byte) 224,
            (byte) 166,
            (byte) 195,
            (byte) 251,
            (byte) 27,
            (byte) 140,
            (byte) 41,
            (byte) 239,
            (byte) 142,
            (byte) 229,
            (byte) 52,
            (byte) 203,
            (byte) 209,
            (byte) 42,
            (byte) 206,
            (byte) 121,
            (byte) 195,
            (byte) 154,
            (byte) 13,
            (byte) 54,
            (byte) 234,
            (byte) 1,
            (byte) 224,
            (byte) 170,
            (byte) 145,
            (byte) 32,
            (byte) 84,
            (byte) 240,
            (byte) 114,
            (byte) 216,
            (byte) 30,
            (byte) 199,
            (byte) 137,
            (byte) 210
        };

        private const uint BlizzardAddOnCRC = 1276933997;

        public static void SendAddOnInfoPacket(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ADDON_INFO))
            {
                if (client.Addons.Length > 0)
                {
                    int num1;
                    using (BinaryReader binReader = new BinaryReader((Stream) new MemoryStream(client.Addons)))
                    {
                        int num2 = binReader.ReadInt32();
                        for (int index = 0; index < num2; ++index)
                        {
                            ClientAddOn addOn = ClientAddonHandler.ReadAddOn(binReader);
                            ClientAddonHandler.WriteAddOnInfo(packet, addOn);
                        }

                        num1 = binReader.ReadInt32();
                    }

                    Console.WriteLine("CMSG ADDON Unk: " + (object) num1);
                }

                packet.Write(0);
                for (int index = 0; index < 0; ++index)
                {
                    packet.Write(0);
                    packet.Write(new byte[16]);
                    packet.Write(new byte[16]);
                    packet.Write(0);
                    packet.Write(0);
                }

                client.Send(packet, false);
            }

            client.Addons = (byte[]) null;
        }

        private static ClientAddOn ReadAddOn(BinaryReader binReader)
        {
            string str = binReader.ReadCString();
            if ((binReader.BaseStream.Position + 9L) > binReader.BaseStream.Length)
            {
                return new ClientAddOn {Name = str};
            }

            return new ClientAddOn
            {
                Name = str,
                HasSignature = binReader.ReadByte(),
                AddOnCRC = binReader.ReadUInt32(),
                ExtraCRC = binReader.ReadUInt32()
            };
        }

        private static void WriteAddOnInfo(RealmPacketOut packet, ClientAddOn addOn)
        {
            packet.Write((byte) 2);
            packet.Write(true);
            bool flag = addOn.AddOnCRC != 1276933997U;
            packet.Write(flag);
            if (flag)
                packet.Write(ClientAddonHandler.BlizzardPublicKey);
            packet.Write(0);
            packet.Write(false);
        }

        private enum AddOnType
        {
            Enabled = 1,
            Blizzard = 2,
        }
    }
}