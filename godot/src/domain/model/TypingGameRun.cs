namespace ankitaiso.domain.model;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using data.model;
using Microsoft.EntityFrameworkCore;

[Table("TypingGameRuns")]
[Index(nameof(Title))]
public class TypingGameRun {
  [Key] public int Id { get; set; }

  [Column(TypeName = "varchar(200)")]
  [Required]
  public string Title { get; set; } = string.Empty;

  public DateTimeOffset Start { get; set; }
  public DateTimeOffset End { get; set; }

  public int HitSuccess { get; set; }
  public int HitFailures { get; set; }
  public bool IsComplete { get; set; }

  public List<TypingGameStatistic> Statistics { get; set; } = [];
}
