namespace kyoukaitansa.game_typing.domain;

using System;
using Chickensoft.Collections;
using GameDemo;
using Godot;

public interface IGameTypingRepo : IDisposable {
  /// <summary>Event invoked whenever the player clears an enemy.</summary>
  event Action? EnemyCleared;

  /// <summary>Number of errors by the player.</summary>
  IAutoProp<int> NumErrors { get; }

  /// <summary>Inform the game that a jumpshroom was used.</summary>
  void OnSpawnEnemy();

  /// <summary>Pauses the game and releases the mouse.</summary>
  void Pause();

  /// <summary>Resumes the game and recaptures the mouse.</summary>
  void Resume();
}

/// <summary>
///   GameTyping repository — stores pure typing game logic
/// </summary>
public class GameTypingRepo : IGameTypingRepo {
  public event Action? EnemyCleared;
  public IAutoProp<int> NumErrors { get; }
  private bool _disposedValue;

  public void OnSpawnEnemy() => throw new NotImplementedException();

  public void Pause() => throw new NotImplementedException();

  public void Resume() => throw new NotImplementedException();

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
