using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Drone
{
    public static class Extensions
    {
        public static byte[] Serialize<T>(this T data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using var ms = new MemoryStream();
            serializer.WriteObject(ms, data);
            return ms.ToArray();
        }

        public static T Deserialize<T>(this byte[] data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using var ms = new MemoryStream(data);
            return (T) serializer.ReadObject(ms);
        }

        public static string ToShortGuid(this Guid guid)
        {
            return guid.ToString().Replace("-", "").Substring(0, 10);
        }
    }
}