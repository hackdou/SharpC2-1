using SQLite;

namespace TeamServer.Storage;

[Table("payloads")]
public class PayloadDao
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("md5")]
    public string MdHash { get; set; }
    
    [Column("sha256")]
    public string ShaHash { get; set; }

    [Column("format")]
    public string Format { get; set; }
    
    [Column("generated")]
    public DateTime Generated { get; set; }
    
    [Column("source")]
    public string Source { get; set; }
}