using Cell.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using WCell.Constants;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.Util.Graphics;

namespace WCell.RealmServer
{
    /// <summary>Class for all type extension methods.</summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds all elements from the collection to the hash set.
        /// </summary>
        /// <typeparam name="T">the type of the elements</typeparam>
        /// <param name="set">the hash set to add to\being extended</param>
        /// <param name="elements">the elements to add</param>
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> elements)
        {
            foreach (T element in elements)
                set.Add(element);
        }

        public static bool ImplementsType(this Type targetType, Type baseType)
        {
            return baseType.IsAssignableFrom(targetType);
        }

        public static float TotalMin(this DamageInfo[] damages)
        {
            return ((IEnumerable<DamageInfo>) damages).Sum<DamageInfo>((Func<DamageInfo, float>) (dmg => dmg.Minimum));
        }

        public static float TotalMax(this DamageInfo[] damages)
        {
            return ((IEnumerable<DamageInfo>) damages).Sum<DamageInfo>((Func<DamageInfo, float>) (dmg => dmg.Maximum));
        }

        public static DamageSchoolMask AllSchools(this DamageInfo[] damages)
        {
            return ((IEnumerable<DamageInfo>) damages).Aggregate<DamageInfo, DamageSchoolMask>(DamageSchoolMask.None,
                (Func<DamageSchoolMask, DamageInfo, DamageSchoolMask>) ((current, dmg) => current | dmg.School));
        }

        public static bool IsValid(this IWorldLocation location)
        {
            if ((double) location.Position.X != 0.0 && location.Map != null)
                return location.Phase != 0U;
            return false;
        }

        public static void AddChecked(this XElement element, string name, int value)
        {
            if (value == 0)
                return;
            element.Add((object) new XElement((XName) name, (object) value));
        }

        public static void AddChecked(this XElement element, string name, float value)
        {
            if ((double) value == 0.0)
                return;
            element.Add((object) new XElement((XName) name, (object) value));
        }

        public static int ReadInt32(this XElement element, string name)
        {
            XElement xelement = element.Element((XName) name);
            if (xelement != null)
                return int.Parse(xelement.Value);
            return 0;
        }

        public static uint ReadUInt32(this XElement element, string name)
        {
            XElement xelement = element.Element((XName) name);
            if (xelement == null)
                return 0;
            uint result;
            if (!uint.TryParse(xelement.Value, out result))
                result = (uint) int.Parse(xelement.Value);
            return result;
        }

        public static uint ReadUInt32(this XElement element)
        {
            uint result;
            uint.TryParse(element.Value, out result);
            return result;
        }

        public static T ReadEnum<T>(this XElement element, string name)
        {
            return (T) Enum.Parse(typeof(T), element.ReadString(name));
        }

        public static bool ReadBoolean(this XElement element, string name)
        {
            XElement xelement = element.Element((XName) name);
            if (xelement == null)
                return false;
            string str = xelement.Value;
            if (!(str == "1"))
                return str.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            return true;
        }

        public static string ReadString(this XElement element, string name)
        {
            XElement xelement = element.Element((XName) name);
            if (xelement != null)
                return xelement.Value;
            return string.Empty;
        }

        public static float ReadFloat(this XElement element, string name)
        {
            XElement xelement = element.Element((XName) name);
            if (xelement != null)
                return float.Parse(xelement.Value);
            return 0.0f;
        }

        public static float ReadFloat(this XElement element, string name, float defaultValue)
        {
            XElement xelement = element.Element((XName) name);
            if (xelement != null)
                return float.Parse(xelement.Value);
            return defaultValue;
        }

        public static Vector3 ReadLocation(this XElement node, string xyzPrefix, bool upperCase)
        {
            return new Vector3(node.ReadFloat(xyzPrefix + (upperCase ? "X" : "x")),
                node.ReadFloat(xyzPrefix + (upperCase ? "Y" : "y")),
                node.ReadFloat(xyzPrefix + (upperCase ? "Z" : "z")));
        }

        public static XmlNode Add(this XmlNode el, string name, Vector3 pos)
        {
            string str = ((double) pos.X).ToString() + "," + (object) pos.Y + "," + (object) pos.Z;
            return el.Add(name, (object) str, new object[0]);
        }

        public static XmlNode Add(this XmlNode el, string name, Vector4 pos)
        {
            string str = ((double) pos.X).ToString() + "," + (object) pos.Y + "," + (object) pos.Z + "," +
                         (object) pos.W;
            return el.Add(name, (object) str, new object[0]);
        }

        public static XmlNode Add(this XmlNode el, string name, object value, params object[] args)
        {
            XmlDocument xmlDocument = el.OwnerDocument ?? el as XmlDocument;
            XmlNode xmlNode = el.AppendChild((XmlNode) xmlDocument.CreateElement(name));
            xmlNode.AppendChild((XmlNode) xmlDocument.CreateTextNode(string.Format(value.ToString(), args)));
            return xmlNode;
        }

        public static XmlNode AddAttr(this XmlNode el, string name, object value, params object[] args)
        {
            XmlDocument xmlDocument = el.OwnerDocument ?? el as XmlDocument;
            XmlAttribute xmlAttribute = el.Attributes.Append(xmlDocument.CreateAttribute(name));
            xmlAttribute.AppendChild((XmlNode) xmlDocument.CreateTextNode(string.Format(value.ToString(), args)));
            return (XmlNode) xmlAttribute;
        }

        public static XmlElement Add(this XmlNode el, string name)
        {
            XmlDocument xmlDocument = el.OwnerDocument ?? el as XmlDocument;
            return el.AppendChild((XmlNode) xmlDocument.CreateElement(name)) as XmlElement;
        }

        public static int GetAttrInt32(this XElement element, string name)
        {
            return int.Parse(element.Attribute((XName) name).Value);
        }

        public static uint GetAttrUInt32(this XElement element, string name)
        {
            uint result;
            if (!uint.TryParse(element.Attribute((XName) name).Value, out result))
                result = (uint) int.Parse(element.Attribute((XName) name).Value);
            return result;
        }

        public static Vector3 GetLocation(this byte[] bytes, uint index)
        {
            return new Vector3(bytes.GetFloat(index), bytes.GetFloat(index + 1U), bytes.GetFloat(index + 2U));
        }

        public static float GetDist(this IHasPosition pos, IHasPosition pos2)
        {
            return pos.Position.GetDistance(pos2.Position);
        }

        public static float GetDistSq(this IHasPosition pos, IHasPosition pos2)
        {
            return pos.Position.DistanceSquared(pos2.Position);
        }

        public static float GetDistSq(this IHasPosition pos, Vector3 pos2)
        {
            return pos.Position.DistanceSquared(pos2);
        }
    }
}