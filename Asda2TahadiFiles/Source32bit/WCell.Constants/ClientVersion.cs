using System;
using System.Runtime.Serialization;

namespace WCell.Constants
{
  /// <summary>Defines a version, i.e. 2.0.10.3424</summary>
  [DataContract]
  [Serializable]
  public struct ClientVersion
  {
    /// <summary>The major version</summary>
    [DataMember]public byte Major;

    /// <summary>The minor version</summary>
    [DataMember]public byte Minor;

    /// <summary>The revision</summary>
    [DataMember]public byte Revision;

    /// <summary>The build</summary>
    [DataMember]public ushort Build;

    /// <summary>Default constructor</summary>
    /// <param name="build">the build of this version</param>
    public ClientVersion(ushort build)
    {
      this = new ClientVersion(byte.MaxValue, byte.MaxValue, byte.MaxValue, build);
    }

    /// <summary>Default constructor.</summary>
    /// <param name="majorVersion">the major part of this version</param>
    /// <param name="minorVersion">the minor part of this version</param>
    /// <param name="revision">the revision of this version</param>
    public ClientVersion(byte majorVersion, byte minorVersion, byte revision)
    {
      this = new ClientVersion(majorVersion, minorVersion, revision, ushort.MaxValue);
    }

    /// <summary>Default constructor.</summary>
    /// <param name="majorVersion">the major part of this version</param>
    /// <param name="minorVersion">the minor part of this version</param>
    /// <param name="revision">the revision of this version</param>
    /// <param name="build">the build of this version</param>
    public ClientVersion(byte majorVersion, byte minorVersion, byte revision, ushort build)
    {
      Major = majorVersion;
      Minor = minorVersion;
      Revision = revision;
      Build = build;
    }

    /// <summary>Default constructor.</summary>
    /// <param name="rawVersion"></param>
    public ClientVersion(byte[] rawVersion)
    {
      Major = rawVersion[0];
      Minor = rawVersion[1];
      Revision = rawVersion[2];
      Build = BitConverter.ToUInt16(rawVersion, 3);
    }

    public ClientVersion(string version)
    {
      try
      {
        string[] strArray = version.Split('.');
        Major = byte.Parse(strArray[0]);
        Minor = byte.Parse(strArray[1]);
        Revision = byte.Parse(strArray[2]);
        Build = ushort.Parse(strArray[3]);
      }
      catch(Exception ex)
      {
        throw new Exception(string.Format("Invalid ClientVersion: " + version), ex);
      }
    }

    /// <summary>
    /// Test if two versions are equivalent.
    /// 
    /// I'm accepting either the major, minor and revision numbers to be the same
    /// or the build numbers to be the same.
    /// </summary>
    /// <param name="obj">object to compare to</param>
    /// <returns>true if the versions are the same; false otherwise</returns>
    public override bool Equals(object obj)
    {
      if(obj == null || GetType() != obj.GetType())
        return false;
      ClientVersion clientVersion = (ClientVersion) obj;
      return Major == clientVersion.Major && Minor == clientVersion.Minor &&
             Revision == clientVersion.Revision || Build == clientVersion.Build;
    }

    /// <summary>Operator overload for == (equality)</summary>
    /// <param name="v1">the first version to compare</param>
    /// <param name="v2">the second version to compare</param>
    /// <returns>true if both versions are equal; false otherwise</returns>
    public static bool operator ==(ClientVersion v1, ClientVersion v2)
    {
      return v1.Equals(v2);
    }

    /// <summary>Operator overload for != (inequality)</summary>
    /// <param name="v1">the first version to compare</param>
    /// <param name="v2">the second version to compare</param>
    /// <returns>true if both versions are equal; false otherwise</returns>
    public static bool operator !=(ClientVersion v1, ClientVersion v2)
    {
      return !(v1 == v2);
    }

    /// <summary>Operator overload for &gt; (greater than)</summary>
    /// <param name="v1">the first version to compare</param>
    /// <param name="v2">the second version to compare</param>
    /// <returns>true if the first version is greater than the second; false otherwise</returns>
    public static bool operator >(ClientVersion v1, ClientVersion v2)
    {
      return v2 < v1;
    }

    /// <summary>Operator overload for &lt; (less than)</summary>
    /// <param name="v1">the first version to compare</param>
    /// <param name="v2">the second version to compare</param>
    /// <returns>true if the first version is less than the second; false otherwise</returns>
    public static bool operator <(ClientVersion v1, ClientVersion v2)
    {
      return v1.CompareTo(v2) < 0;
    }

    /// <summary>
    /// Operator overload for &lt;= (greater than or equal to)
    /// </summary>
    /// <param name="v1">the first version to compare</param>
    /// <param name="v2">the second version to compare</param>
    /// <returns>true if the first version is greater than or equal to the second; false otherwise</returns>
    public static bool operator >=(ClientVersion v1, ClientVersion v2)
    {
      return v2 <= v1;
    }

    /// <summary>Operator overload for &lt;= (less than or equal to)</summary>
    /// <param name="v1">the first version to compare</param>
    /// <param name="v2">the second version to compare</param>
    /// <returns>true if the first version is less than or equal to the second; false otherwise</returns>
    public static bool operator <=(ClientVersion v1, ClientVersion v2)
    {
      return v1.CompareTo(v2) <= 0;
    }

    /// <summary>Compares a version to this ersion</summary>
    /// <param name="value">the version to compare</param>
    /// <returns>-1 if the version is less than this, 0 is it is equal, and -1 if it is greater than this</returns>
    public int CompareTo(ClientVersion value)
    {
      if(Major != value.Major)
        return (int) Major > (int) value.Major ? 1 : -1;
      if(Minor != value.Minor)
        return (int) Minor > (int) value.Minor ? 1 : -1;
      if(Revision != value.Revision)
        return (int) Revision > (int) value.Revision ? 1 : -1;
      if(Build == value.Build)
        return 0;
      return (int) Build > (int) value.Build ? 1 : -1;
    }

    public bool IsSupported(ClientVersion version)
    {
      return Major == version.Major && Minor == version.Minor &&
             Revision == version.Revision;
    }

    /// <summary>Gets the hash code for this version</summary>
    /// <returns>an integer hashcode, (almost) unique to this version</returns>
    public override int GetHashCode()
    {
      return Major << 28 | Minor << 24 | Revision << 16 | Build;
    }

    public string BasicString
    {
      get
      {
        if(Major == byte.MaxValue)
          return string.Format("Build {0}", Build);
        return string.Format("{0}.{1}.{2}", Major, Minor, Revision);
      }
    }

    /// <summary>Returns the string representation for this version</summary>
    /// <returns>the string representing this version</returns>
    public override string ToString()
    {
      if(Major == byte.MaxValue)
        return string.Format("Build {0}", Build);
      if(Build == ushort.MaxValue)
        return string.Format("{0}.{1}.{2}", Major, Minor, Revision);
      return string.Format("{0}.{1}.{2}.{3}", (object) Major, (object) Minor, (object) Revision,
        (object) Build);
    }
  }
}