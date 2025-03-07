namespace ankitaiso.domain;

using System.Linq;
using data.model;
using Godot;
using Microsoft.EntityFrameworkCore;

public class DatabaseContext : DbContext {
  private const int VERSION = 1;

  public DbSet<DatabaseVersion> Version { get; set; }
  public DbSet<TypingGameRun> Runs { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
    optionsBuilder.UseSqlite($"Data Source={ProjectSettings.GlobalizePath("user://database.db")}");

  public void EnsureUp2date() {
    Database.EnsureCreated();
    if (!Version.Any()) {
      Version.Add(new DatabaseVersion() { Version = VERSION });
      SaveChanges();
    }
    else {
      var version = Version.ToList().First();
      if (version.Version != VERSION) {
        Database.EnsureDeleted();
        Database.EnsureCreated();
      }
    }
  }
}
