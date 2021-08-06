using System;
using System.Text.Json;

namespace TeamServer
{
    public static class Extensions
    {
        private static readonly JsonSerializerOptions Options = new() {PropertyNameCaseInsensitive = true};

        public static T Deserialize<T>(this byte[] bytes)
            => JsonSerializer.Deserialize<T>(bytes, Options);


            public static T Deserialize<T>(this string json)
            => JsonSerializer.Deserialize<T>(json, Options);

        public static byte[] Serialize<T>(this T data)
            => JsonSerializer.SerializeToUtf8Bytes(data, Options);

        public static string ConvertToShortGuid(this Guid guid)
            => guid.ToString().Replace("-", "")[..10];
    }
}