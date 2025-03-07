namespace ankitaiso.data.model;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Table("TypingGameRuns")]
[Index(nameof(Title))]
public class TypingGameRun {
  [Key] public int Id { get; set; }

  [Column(TypeName = "varchar(200)")]
  [Required]
  public string Title { get; set; } = string.Empty;

  public DateTime Start { get; set; }
  public DateTime End { get; set; }

  public int HitSuccess { get; set; }
  public int HitFailures { get; set; }
  public bool IsComplete { get; set; }
}
