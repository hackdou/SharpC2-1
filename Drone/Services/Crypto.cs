using System;
using System.Linq;
using System.Security.Cryptography;

using Drone.Interfaces;
using Drone.Utilities;

namespace Drone.Services;

public class Crypto : ICrypto
{
    public (byte[] iv, byte[] data, byte[] checksum) EncryptObject<T>(T obj)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = Key;
        aes.GenerateIV();

        using var transform = aes.CreateEncryptor();
        
        var raw = obj.Serialize();
        var enc = transform.TransformFinalBlock(raw, 0, raw.Length);
        var checksum = ComputeHmac(enc);

        return (aes.IV, enc, checksum);
    }

    public T DecryptObject<T>(byte[] iv, byte[] data, byte[] checksum)
    {
        if (!ComputeHmac(data).SequenceEqual(checksum))
            throw new CryptoException("Invalid Checksum");
        
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = Key;
        aes.IV = iv;

        using var transform = aes.CreateDecryptor();
        var dec = transform.TransformFinalBlock(data, 0, data.Length);

        return dec.Deserialize<T>();
    }

    private static byte[] ComputeHmac(byte[] data)
    {
        using var hmac = new HMACSHA256(Key);
        return hmac.ComputeHash(data);
    }

    private static byte[] Key => Convert.FromBase64String("TfiAGr88Ia1PHiFHxTVMTf5/qXzhgN2nnn4TvsXYUQo=");
}

public class CryptoException : Exception
{
    public CryptoException() { }
    public CryptoException(string message) : base(message) { }
    public CryptoException(string message, Exception inner) : base(message, inner) { }
}