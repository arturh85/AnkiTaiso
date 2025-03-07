namespace ankitaiso.game_typing.domain;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Chickensoft.Collections;
using data;
using Godot;

public interface IGameTypingRepo : IDisposable {
  /// <summary>Event invoked whenever the player clears an enemy.</summary>
  event Action? EnemyCleared;

  /// <summary>Number of errors by the player.</summary>
  IAutoProp<int> NumErrors { get; }

  public void SetNumErrors(int numErrors);
  public void IncreaseNumErrors();

  /// <summary>Number of words cleared by the player.</summary>
  IAutoProp<int> ClearedWords { get; }
  public void SetClearedWords(int clearedWords);
  public void IncreaseClearedWords();

  /// <summary>Number of total words.</summary>
  IAutoProp<int> TotalWords { get; }
  public void SetTotalWords(int totalWords);

  public Scenario? ActiveScenario { get; set; }
  public ScenarioOptions? ActiveScenarioOptions { get; set; }

  public void SetScenarios(Scenario[] scenarios);
  public Scenario? GetScenario(StringName name);
  public IEnumerable<string> GetScenarioIds();

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
  public IAutoProp<int> NumErrors => _numErrors;
  private readonly AutoProp<int> _numErrors;

  public void IncreaseNumErrors() => _numErrors.OnNext(_numErrors.Value + 1);

  public IAutoProp<int> ClearedWords => _clearedWords;
  private readonly AutoProp<int> _clearedWords;
  public void SetClearedWords(int clearedWords) => _clearedWords.OnNext(clearedWords);
  public void IncreaseClearedWords() => _clearedWords.OnNext(_clearedWords.Value + 1);

  public IAutoProp<int> TotalWords => _totalWords;

  private readonly AutoProp<int> _totalWords;
  public void SetTotalWords(int totalWords) => _totalWords.OnNext(totalWords);
  public Scenario? ActiveScenario { get; set; }
  public ScenarioOptions? ActiveScenarioOptions { get; set; }

  private bool _disposedValue;

  public GameTypingRepo() {
    _numErrors = new AutoProp<int>(0);
    _clearedWords = new AutoProp<int>(0);
    _totalWords = new AutoProp<int>(0);
  }


  public void OnSpawnEnemy() => throw new NotImplementedException();

  public void Pause() => throw new NotImplementedException();

  public void Resume() => throw new NotImplementedException();



  private ImmutableDictionary<string, Scenario> _scenarios = ImmutableDictionary<string, Scenario>.Empty;
  private ScenarioOptions? _activeScenarioOptions;

  public void SetScenarios(Scenario[] scenarios) {
    var dict = new Dictionary<string, Scenario>();
    foreach (var scenario in scenarios) {
      dict.Add(scenario.Id, scenario);
    }
    _scenarios = dict.ToImmutableDictionary();
  }

  public Scenario? GetScenario(StringName name) {
    return _scenarios.GetValueOrDefault(name);
  }

  public IEnumerable<string> GetScenarioIds() => _scenarios.Keys;
  public void SetNumErrors(int numErrors) =>
    _numErrors.OnNext(numErrors);


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
