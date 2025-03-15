namespace ankitaiso.domain;

using System;
using System.Threading.Tasks;
using data;
using data.model;
using game_typing;
using Microsoft.Extensions.DependencyInjection;
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
  public async Task InitDatabase() {
    var services = new ServiceCollection();
    services.AddDbContext<DatabaseContext>();
    _serviceProvider = services.BuildServiceProvider();
    await using var dbContext = GetContext();
    dbContext?.EnsureUp2date();
  }


  public Task StoreRun(GameTypingSystem system, Scenario scenario) {
    var run = new TypingGameRun {
      Title = scenario.Title,
      Start = system.GetStart().GetValueOrDefault(),
      End = system.GetEnd().GetValueOrDefault(),
      HitSuccess = system.StatisticTotalSuccess,
      HitFailures = system.StatisticTotalError,
      IsComplete = system.GetEnd() != null
    };

    var context = GetContext();
    if (context == null) {
      throw new GameException("missing db context");
    }
    context.Runs.Add(run);
    return Task.CompletedTask;
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
