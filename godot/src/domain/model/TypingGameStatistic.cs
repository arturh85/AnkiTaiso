namespace ankitaiso.domain.model;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("TypingGameStatistic")]
[Index(nameof(Id))]
public class TypingGameStatistic {
  [Key] public int Id { get; set; }
  public int RunId { get; set; }
  [ForeignKey(nameof(RunId))]
  public virtual TypingGameRun? Run { get; set; }

  public string Character { get; set; } = string.Empty;
  public int HitSuccess { get; set; }
  public int HitFailures { get; set; }
}
