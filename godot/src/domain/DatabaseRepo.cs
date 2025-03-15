namespace ankitaiso.domain;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using data;
using game_typing;
using Microsoft.Extensions.DependencyInjection;
using model;
using utils;

public interface IDatabaseRepo : IDisposable {
  public Task InitDatabase();
  public Task StoreRun(GameTypingSystem system, Scenario scenario);
}

/// <summary>
///   Database repository — stores pure typing game logic
/// </summary>
public class DatabaseRepo : IDatabaseRepo {
  private bool _disposedValue;

  private ServiceProvider? _serviceProvider;
  public Task InitDatabase() {
    var services = new ServiceCollection();
    services.AddDbContext<DatabaseContext>();
    _serviceProvider = services.BuildServiceProvider();
    var dbContext = GetContext();
    dbContext?.EnsureUp2date();
    return Task.CompletedTask;
  }


  public async Task StoreRun(GameTypingSystem system, Scenario scenario) {
    var stats = new List<TypingGameStatistic>();

    var dbContext = GetContext();
    foreach (var (c, charStat) in system.StatisticByChar) {

      var stat = new TypingGameStatistic {
        Character = c.ToString(),
        HitSuccess = charStat.SuccessCount,
        HitFailures = charStat.FailCount
      };
      dbContext?.Statistics.Add(stat);
      stats.Add(stat);
    }

    var run = new TypingGameRun {
      Title = scenario.Title,
      Start = system.GetStart().GetValueOrDefault(),
      End = system.GetEnd().GetValueOrDefault(),
      HitSuccess = system.StatisticTotalSuccess,
      HitFailures = system.StatisticTotalError,
      IsComplete = system.GetEnd() != null,
      Statistics = stats
    };

    if (dbContext == null) {
      throw new GameException("missing db context");
    }
    dbContext.Runs.Add(run);
    await dbContext.SaveChangesAsync(true);
  }

  public DatabaseContext? GetContext() => _serviceProvider?.GetService<DatabaseContext>();

  #region Internals

  protected void Dispose(bool disposing) {
    if (!_disposedValue) {
      if (disposing) {
        // Dispose managed objects.
        _serviceProvider?.Dispose();
      }

      _disposedValue = true;
    }
  }

  public void Dispose() {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  #endregion Internals
}
