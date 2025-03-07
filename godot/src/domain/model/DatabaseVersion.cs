namespace ankitaiso.data.model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("DatabaseVersions")]
public class DatabaseVersion {

  [Key] public int Id { get; set; }
  public int Version { get; set; }
}
