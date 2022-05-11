using SQLite;

namespace TeamServer.Storage;

[Table("http_handlers")]
public class HttpHandlerDao
{
    [PrimaryKey, Indexed]
    [Column("name")]
    public string Name { get; set; }
    
    [Column("bind_port")]
    public int BindPort { get; set; }
    
    [Column("connect_address")]
    public string ConnectAddress { get; set; }
    
    [Column("connect_port")]
    public int ConnectPort { get; set; }

    [Column("profile")]
    public string Profile { get; set; }
}