namespace ankitaiso.game.domain;

using System;
using Chickensoft.Collections;
using Godot;

public interface IGameRepo : IDisposable {
  /// <summary>Event invoked when the game ends.</summary>
  event Action<GameOverReason>? Ended;

  /// <summary>Mouse captured status.</summary>
  IAutoProp<bool> IsMouseCaptured { get; }

  /// <summary>Pause status.</summary>
  IAutoProp<bool> IsPaused { get; }

  /// <summary>Player's position in global coordinates.</summary>
  IAutoProp<Vector3> PlayerGlobalPosition { get; }

  /// <summary>Camera's global transform basis.</summary>
  IAutoProp<Basis> CameraBasis { get; }

  /// <summary>Camera's global forward direction vector.</summary>
  Vector3 GlobalCameraDirection { get; }

  /// <summary>Inform the game that the game ended.</summary>
  /// <param name="reason">Game over reason.</param>
  void OnGameEnded(GameOverReason reason);

  /// <summary>Pauses the game and releases the mouse.</summary>
  void Pause();

  /// <summary>Resumes the game and recaptures the mouse.</summary>
  void Resume();

  /// <summary>Tells the game that the player jumped.</summary>
  void OnJump();

  /// <summary>Changes whether the mouse is captured or not.</summary>
  /// <param name="isMouseCaptured">
  ///   Whether or not the mouse is captured.
  /// </param>
  void SetIsMouseCaptured(bool isMouseCaptured);

  /// <summary>Sets the camera's global transform basis.</summary>
  /// <param name="cameraBasis">Camera global transform basis.</param>
  void SetCameraBasis(Basis cameraBasis);

  /// <summary>Sets the player's global position.</summary>
  /// <param name="playerGlobalPosition">
  ///   Player's global position in world
  ///   coordinates.
  /// </param>
  void SetPlayerGlobalPosition(Vector3 playerGlobalPosition);
}

/// <summary>
///   Game repository — stores pure game logic that's not directly related to the
///   game node's overall view.
/// </summary>
public class GameRepo : IGameRepo {
  public IAutoProp<bool> IsMouseCaptured => _isMouseCaptured;
  private readonly AutoProp<bool> _isMouseCaptured;
  public IAutoProp<bool> IsPaused => _isPaused;
  private readonly AutoProp<bool> _isPaused;
  public IAutoProp<Vector3> PlayerGlobalPosition => _playerGlobalPosition;
  private readonly AutoProp<Vector3> _playerGlobalPosition;

  public IAutoProp<Basis> CameraBasis => _cameraBasis;
  private readonly AutoProp<Basis> _cameraBasis;

  public Vector3 GlobalCameraDirection => -_cameraBasis.Value.Z;

  public event Action<GameOverReason>? Ended;
  public event Action? Jumped;

  private bool _disposedValue;

  public GameRepo() {
    _isMouseCaptured = new AutoProp<bool>(false);
    _isPaused = new AutoProp<bool>(false);
    _playerGlobalPosition = new AutoProp<Vector3>(Vector3.Zero);
    _cameraBasis = new AutoProp<Basis>(Basis.Identity);
  }

  internal GameRepo(
    AutoProp<bool> isMouseCaptured,
    AutoProp<bool> isPaused,
    AutoProp<Vector3> playerGlobalPosition,
    AutoProp<Basis> cameraBasis
  ) {
    _isMouseCaptured = isMouseCaptured;
    _isPaused = isPaused;
    _playerGlobalPosition = playerGlobalPosition;
    _cameraBasis = cameraBasis;
  }

  public void SetPlayerGlobalPosition(Vector3 playerGlobalPosition) =>
    _playerGlobalPosition.OnNext(playerGlobalPosition);

  public void SetIsMouseCaptured(bool isMouseCaptured) =>
    _isMouseCaptured.OnNext(isMouseCaptured);

  public void SetCameraBasis(Basis cameraBasis) =>
    _cameraBasis.OnNext(cameraBasis);

  public void OnJump() => Jumped?.Invoke();

  public void OnGameEnded(GameOverReason reason) {
    _isMouseCaptured.OnNext(false);
    Pause();
    Ended?.Invoke(reason);
  }

  public void Pause() {
    _isMouseCaptured.OnNext(false);
    _isPaused.OnNext(true);
  }

  public void Resume() {
    _isMouseCaptured.OnNext(true);
    _isPaused.OnNext(false);
  }

  #region Internals

  protected void Dispose(bool disposing) {
    if (!_disposedValue) {
      if (disposing) {
        // Dispose managed objects.
        _isMouseCaptured.OnCompleted();
        _isMouseCaptured.Dispose();

        _playerGlobalPosition.OnCompleted();
        _playerGlobalPosition.Dispose();

        _cameraBasis.OnCompleted();
        _cameraBasis.Dispose();
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
