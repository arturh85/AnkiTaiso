namespace ankitaiso.game_typing.domain;

using System;
using System.Threading.Tasks;
using ankitaiso.domain;
using Microsoft.Extensions.DependencyInjection;

public interface IDatabaseRepo : IDisposable {
  public Task InitDatabase();
}

/// <summary>
///   Database repository — stores pure typing game logic
/// </summary>
public class DatabaseRepo : IDatabaseRepo {
  private bool _disposedValue;

  private ServiceProvider _serviceProvider;
  public async Task InitDatabase() {
    var services = new ServiceCollection();
    services.AddDbContext<DatabaseContext>();
    _serviceProvider = services.BuildServiceProvider();
    await using var dbContext = GetContext();
    dbContext?.EnsureUp2date();
  }

  public DatabaseContext? GetContext() => _serviceProvider.GetService<DatabaseContext>();

  #region Internals

  protected void Dispose(bool disposing) {
    if (!_disposedValue) {
      if (disposing) {
        // Dispose managed objects.
        _serviceProvider.Dispose();
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
