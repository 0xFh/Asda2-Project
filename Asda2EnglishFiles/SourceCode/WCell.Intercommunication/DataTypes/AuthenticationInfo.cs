using System.Runtime.Serialization;

namespace WCell.Intercommunication.DataTypes
{
    /// <summary>Holds authentication information</summary>
    [DataContract]
    public class AuthenticationInfo
    {
        /// <summary>Session key used for the session</summary>
        [DataMember] public byte[] SessionKey;

        /// <summary>Salt used for the session</summary>
        [DataMember] public byte[] Salt;

        /// <summary>Verifier used for the session</summary>
        [DataMember] public byte[] Verifier;

        /// <summary>System information of the client</summary>
        [DataMember] public byte[] SystemInformation;
    }
}