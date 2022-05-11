using SQLite;

namespace TeamServer.Storage;

[Table("crypto")]
public class CryptoDao
{
    [Column("key")]
    public byte[] Key { get; set; }
}