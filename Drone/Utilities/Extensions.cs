using System;
using System.IO;

using ProtoBuf;

namespace Drone.Utilities;

public static class Extensions
{
    public static byte[] Serialize<T>(this T obj)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, obj);
        return ms.ToArray();
    }

    public static T Deserialize<T>(this byte[] data)
    {
        using var ms = new MemoryStream(data);
        return Serializer.Deserialize<T>(ms);
    }
    
    public static string ConvertToShortGuid(this Guid guid)
    {
        return guid.ToString().Replace("-", "").Substring(0, 10);
    }

    public static byte[] ToByteArray(this (byte[] iv, byte[] data, byte[]checksum) data)
    {
        var buffer = new byte[data.iv.Length + data.data.Length + data.checksum.Length];

        // iv
        Buffer.BlockCopy(data.iv, 0, buffer, 0, data.iv.Length);

        // hmac
        Buffer.BlockCopy(data.checksum, 0, buffer, data.iv.Length, data.checksum.Length);

        // data
        Buffer.BlockCopy(data.data, 0, buffer, data.iv.Length + data.checksum.Length, data.data.Length);

        return buffer;
    }
}