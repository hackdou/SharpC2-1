using dnlib.DotNet;

using ProtoBuf;

namespace TeamServer.Utilities;

public static class Extensions
{
    public static (byte[] iv, byte[] data, byte[] checksum) FromByteArray(this byte[] bytes)
    {
        // static sizes
        // iv 16 bytes
        // hmac 32 bytes
        // data n bytes
        
        var iv = bytes[..16];
        var checksum = bytes[16..(16 + 32)];
        var data = bytes[(16 + 32)..];

        return (iv, data, checksum);
    }
    
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
    
    public static TypeDef GetTypeDef(this ModuleDef module, string name)
    {
        return module.Types.Single(t => t.Name.String.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public static MethodDef GetMethodDef(this TypeDef type, string name)
    {
        return type.Methods.Single(m => m.Name.String.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}