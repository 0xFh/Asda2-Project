using Cell.Core;
using NLog;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using WCell.Constants;
using WCell.Core.Localization;
using WCell.Core.Network;

namespace WCell.Core
{
  /// <summary>
  /// Describes basic system information about a client,
  /// including architecture, OS, and locale.
  /// </summary>
  [Serializable]
  public class ClientInformation
  {
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private ClientLocale _locale = ClientLocale.English;
    private ClientType _clientInstallationType;
    private Constants.OperatingSystem _operatingSys;
    private ProcessorArchitecture _architecture;

    public ClientInformation()
    {
      _operatingSys = Constants.OperatingSystem.Win;
      _architecture = ProcessorArchitecture.x86;
      Locale = ClientLocale.English;
      TimeZone = 600U;
      IPAddress = new XmlIPAddress(System.Net.IPAddress.Loopback);
    }

    private ClientInformation(PacketIn packet)
    {
      try
      {
        ProtocolVersion = packet.ReadByte();
        ushort num = packet.ReadUInt16();
        if(packet.RemainingLength != num)
          Log.Warn(WCell_Core.Auth_Logon_with_invalid_length, num,
            packet.RemainingLength);
        _clientInstallationType = ClientTypeUtility.Lookup(packet.ReadFourCC());
        Version = new ClientVersion(packet.ReadBytes(5));
        Architecture = packet.ReadFourCC().TrimEnd(new char[1]);
        OS = packet.ReadFourCC().TrimEnd(new char[1]);
        _locale = ClientLocaleUtility.Lookup(packet.ReadFourCC());
        TimeZone = BitConverter.ToUInt32(packet.ReadBytes(4), 0);
        IPAddress = new XmlIPAddress(packet.ReadBytes(4));
        Log.Info(WCell_Core.ClientInformationFourCCs, (object) ProtocolVersion,
          (object) ClientInstallationType, (object) Version, (object) Architecture,
          (object) OS, (object) Locale, (object) TimeZone, (object) IPAddress);
      }
      catch
      {
      }
    }

    /// <summary>The game client version of the client.</summary>
    public ClientVersion Version { get; set; }

    /// <summary>The game client version of the client.</summary>
    public byte ProtocolVersion { get; set; }

    /// <summary>The type of client that is attempting to connect.</summary>
    public ClientType ClientInstallationType
    {
      get { return _clientInstallationType; }
      set { _clientInstallationType = value; }
    }

    /// <summary>The operating system of the client.</summary>
    public string OS
    {
      get { return Enum.GetName(typeof(Constants.OperatingSystem), _operatingSys); }
      set
      {
        try
        {
          _operatingSys =
            (Constants.OperatingSystem) Enum.Parse(typeof(Constants.OperatingSystem), value);
        }
        catch(ArgumentException ex)
        {
        }
      }
    }

    /// <summary>The CPU architecture of the client.</summary>
    public string Architecture
    {
      get { return Enum.GetName(typeof(ProcessorArchitecture), _architecture); }
      set
      {
        try
        {
          _architecture = (ProcessorArchitecture) Enum.Parse(typeof(ProcessorArchitecture), value);
        }
        catch(ArgumentException ex)
        {
        }
      }
    }

    /// <summary>The location and native language of the client.</summary>
    public ClientLocale Locale
    {
      get { return _locale; }
      set { _locale = value; }
    }

    /// <summary>The timezone of the client.</summary>
    public uint TimeZone { get; set; }

    /// <summary>
    /// The IP address of the client.
    /// Not really trustworthy.
    /// </summary>
    /// <remarks>This is serializable.</remarks>
    public XmlIPAddress IPAddress { get; set; }

    /// <summary>
    /// Generates a system information objet from the given packet.
    /// </summary>
    /// <param name="inPacket">contains the system information in a raw, serialized format</param>
    public static ClientInformation ReadFromPacket(PacketIn packet)
    {
      return new ClientInformation(packet);
    }

    /// <summary>
    /// Serializes a <see cref="T:WCell.Core.ClientInformation" /> object to a binary representation.
    /// </summary>
    /// <param name="clientInfo">the client information object</param>
    /// <returns>the binary representation of the <see cref="T:WCell.Core.ClientInformation" /> object</returns>
    public static byte[] Serialize(ClientInformation clientInfo)
    {
      byte[] array;
      using(MemoryStream memoryStream = new MemoryStream())
      {
        new BinaryFormatter().Serialize(memoryStream, clientInfo);
        array = memoryStream.ToArray();
      }

      return array;
    }

    /// <summary>
    /// Deserializes a <see cref="T:WCell.Core.ClientInformation" /> object from its binary representation.
    /// </summary>
    /// <param name="rawInfoData">the binary data for the <see cref="T:WCell.Core.ClientInformation" /> object</param>
    /// <returns>a <see cref="T:WCell.Core.ClientInformation" /> object</returns>
    public static ClientInformation Deserialize(byte[] rawInfoData)
    {
      ClientInformation clientInformation;
      using(MemoryStream memoryStream = new MemoryStream(rawInfoData))
        clientInformation = (ClientInformation) new BinaryFormatter().Deserialize(memoryStream);
      return clientInformation;
    }
  }
}