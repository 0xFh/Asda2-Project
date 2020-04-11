using NLog;
using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace WCell.Util.Xml
{
    /// <summary>TODO: Allow case-insensitive node names</summary>
    public static class XmlUtil
    {
        /// <summary>We needs this for correct parsing.</summary>
        public static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-US");

        public static CultureInfo OrigCulture;

        /// <summary>
        /// Ensure a default culture, so float-comma and other values are parsed correctly on all systems
        /// </summary>
        public static void EnsureCulture()
        {
            if (Thread.CurrentThread.CurrentCulture == XmlUtil.DefaultCulture)
                return;
            XmlUtil.OrigCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = XmlUtil.DefaultCulture;
        }

        /// <summary>Reset system-culture after parsing</summary>
        public static void ResetCulture()
        {
            if (XmlUtil.OrigCulture == null)
                return;
            Thread.CurrentThread.CurrentCulture = XmlUtil.OrigCulture;
        }

        public static string ReadString(this XmlNode node, string name)
        {
            return node[name].InnerText;
        }

        public static bool ReadBool(this XmlNode node, string name)
        {
            string innerText = node[name].InnerText;
            return innerText == "1" || innerText.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }

        public static int ReadInt(this XmlNode node, string name)
        {
            return node.ReadInt(name, 0);
        }

        public static int ReadInt(this XmlNode node, string name, int defaultVal)
        {
            int result;
            if (int.TryParse(node[name].InnerText, out result))
                return result;
            return defaultVal;
        }

        public static uint ReadUInt(this XmlNode node, string name)
        {
            return node.ReadUInt(name, 0U);
        }

        public static uint ReadUInt(this XmlNode node, string name, uint defaultVal)
        {
            uint result;
            if (!uint.TryParse(node[name].InnerText, out result))
                result = defaultVal;
            return result;
        }

        public static float ReadFloat(this XmlNode node, string name)
        {
            return node.ReadFloat(name, 0.0f);
        }

        public static float ReadFloat(this XmlNode node, string name, float defaultVal)
        {
            float result;
            if (!float.TryParse(node[name].InnerText, out result))
                result = defaultVal;
            return result;
        }

        public static E ReadEnum<E>(this XmlNode node, string name)
        {
            return (E) Enum.Parse(typeof(E), node[name].InnerText);
        }

        public static void SkipEmptyNodes(this XmlReader reader)
        {
            while (reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment)
                reader.ReadInnerXml();
        }

        public static void WriteCollection(this XmlWriter writer, IEnumerable col, string itemName)
        {
            foreach (object obj in col)
            {
                writer.WriteStartElement(itemName);
                if (obj == null)
                    LogManager.GetCurrentClassLogger().Warn("Invalid null-element in Collection: " + itemName);
                else if (obj is IXmlSerializable)
                    ((IXmlSerializable) obj).WriteXml(writer);
                else
                    writer.WriteString(obj.ToString());
                writer.WriteEndElement();
            }
        }
    }
}