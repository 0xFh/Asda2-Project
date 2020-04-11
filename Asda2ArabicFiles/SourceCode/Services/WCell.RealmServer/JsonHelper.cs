using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace WCell.RealmServer
{
    public class JsonHelper
    {
        /// <summary>
        ///     JSON Serialization
        /// </summary>
        public static string Serialize<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }

        /// <summary>
        ///     JSON Deserialization
        /// </summary>
        public static T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static byte[] SerializeToBson<T>(T t)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, t);
            }
            return ms.ToArray();
        }

        public static T Deserialize<T>(Byte[] bson)
        {
            var ms = new MemoryStream(bson);
            using (var reader = new BsonReader(ms))
            {
                var serializer = new JsonSerializer();

                var t = serializer.Deserialize<T>(reader);
                return t;
            }
        }
    }
}