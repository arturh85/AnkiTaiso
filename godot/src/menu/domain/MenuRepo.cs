namespace ankitaiso.menu.domain;

using System;
using Godot;

public interface IMenuRepo : IDisposable {
  void SetActiveScenarioId(StringName scenarioId);
  StringName? GetActiveScenarioId();
}

/// <summary>
///   Menu repository — stores pure typing game logic
/// </summary>
public class MenuRepo : IMenuRepo {
  private bool _disposedValue;
  private StringName? _scenarioId;
  public void SetActiveScenarioId(StringName scenarioId) => _scenarioId = scenarioId;

  public StringName? GetActiveScenarioId() => _scenarioId;

  #region Internals

  protected void Dispose(bool disposing) {
    if (!_disposedValue) {
      if (disposing) {
        // Dispose managed objects.
        // _isMouseCaptured.OnCompleted();
        // _isMouseCaptured.Dispose();
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
