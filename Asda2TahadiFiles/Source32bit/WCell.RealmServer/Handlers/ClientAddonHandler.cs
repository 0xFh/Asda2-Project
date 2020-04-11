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
      195,
      91,
      80,
      132,
      185,
      62,
      50,
      66,
      140,
      208,
      199,
      72,
      250,
      14,
      93,
      84,
      90,
      163,
      14,
      20,
      186,
      158,
      13,
      185,
      93,
      139,
      238,
      182,
      132,
      147,
      69,
      117,
      byte.MaxValue,
      49,
      254,
      47,
      100,
      63,
      61,
      109,
      7,
      217,
      68,
      155,
      64,
      133,
      89,
      52,
      78,
      16,
      225,
      231,
      67,
      105,
      239,
      124,
      22,
      252,
      180,
      237,
      27,
      149,
      40,
      168,
      35,
      118,
      81,
      49,
      87,
      48,
      43,
      121,
      8,
      80,
      16,
      28,
      74,
      26,
      44,
      200,
      139,
      143,
      5,
      45,
      34,
      61,
      219,
      90,
      36,
      122,
      15,
      19,
      80,
      55,
      143,
      90,
      204,
      158,
      4,
      68,
      14,
      135,
      1,
      212,
      163,
      21,
      148,
      22,
      52,
      198,
      194,
      195,
      251,
      73,
      254,
      225,
      249,
      218,
      140,
      80,
      60,
      190,
      44,
      187,
      87,
      237,
      70,
      185,
      173,
      139,
      198,
      223,
      14,
      214,
      15,
      190,
      128,
      179,
      139,
      30,
      119,
      207,
      173,
      34,
      207,
      183,
      75,
      207,
      251,
      240,
      107,
      17,
      69,
      45,
      122,
      129,
      24,
      242,
      146,
      126,
      152,
      86,
      93,
      94,
      105,
      114,
      10,
      13,
      3,
      10,
      133,
      162,
      133,
      156,
      203,
      251,
      86,
      110,
      143,
      68,
      187,
      143,
      2,
      34,
      104,
      99,
      151,
      188,
      133,
      186,
      168,
      247,
      181,
      64,
      104,
      60,
      119,
      134,
      111,
      75,
      215,
      136,
      202,
      138,
      215,
      206,
      54,
      240,
      69,
      110,
      213,
      100,
      121,
      15,
      23,
      252,
      100,
      221,
      16,
      111,
      243,
      245,
      224,
      166,
      195,
      251,
      27,
      140,
      41,
      239,
      142,
      229,
      52,
      203,
      209,
      42,
      206,
      121,
      195,
      154,
      13,
      54,
      234,
      1,
      224,
      170,
      145,
      32,
      84,
      240,
      114,
      216,
      30,
      199,
      137,
      210
    };

    private const uint BlizzardAddOnCRC = 1276933997;

    public static void SendAddOnInfoPacket(IRealmClient client)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ADDON_INFO))
      {
        if(client.Addons.Length > 0)
        {
          int num1;
          using(BinaryReader binReader = new BinaryReader(new MemoryStream(client.Addons)))
          {
            int num2 = binReader.ReadInt32();
            for(int index = 0; index < num2; ++index)
            {
              ClientAddOn addOn = ReadAddOn(binReader);
              WriteAddOnInfo(packet, addOn);
            }

            num1 = binReader.ReadInt32();
          }

          Console.WriteLine("CMSG ADDON Unk: " + num1);
        }

        packet.Write(0);
        for(int index = 0; index < 0; ++index)
        {
          packet.Write(0);
          packet.Write(new byte[16]);
          packet.Write(new byte[16]);
          packet.Write(0);
          packet.Write(0);
        }

        client.Send(packet, false);
      }

      client.Addons = null;
    }

    private static ClientAddOn ReadAddOn(BinaryReader binReader)
    {
      string str = binReader.ReadCString();
      if((binReader.BaseStream.Position + 9L) > binReader.BaseStream.Length)
      {
        return new ClientAddOn { Name = str };
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
      if(flag)
        packet.Write(BlizzardPublicKey);
      packet.Write(0);
      packet.Write(false);
    }

    private enum AddOnType
    {
      Enabled = 1,
      Blizzard = 2
    }
  }
}