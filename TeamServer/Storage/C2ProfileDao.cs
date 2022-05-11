using SQLite;

namespace TeamServer.Storage;

[Table("profiles")]
public class C2ProfileDao
{
    [PrimaryKey]
    [Column("name")]
    public string Name { get; set; }

    [Column("yaml")]
    public string Yaml { get; set; }
}