using SQLite;

namespace TeamServer.Storage;

[Table("tasks")]
public class DroneTaskRecordDao
{
    [PrimaryKey]
    [Column("task_id")]
    public string TaskId { get; set; }
    
    [Indexed]
    [Column("drone_id")]
    public string DroneId { get; set; }
    
    [Column("drone_function")]
    public string DroneFunction { get; set; }

    [Column("alias")]
    public string CommandAlias { get; set; }

    [Column("parameters")]
    public string Parameters { get; set; }  // can't store as string[]

    [Column("artefact_path")]
    public string ArtefactPath { get; set; }

    [Column("artefact")]
    public byte[] Artefact { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("status")]
    public int Status { get; set; }
    
    [Column("end_time")]
    public DateTime EndTime { get; set; }
    
    [Column("result")]
    public byte[] Result { get; set; }
}