namespace ankitaiso.game_typing.domain;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using data;
using Godot;

public interface IGameTypingRepo : IDisposable {
  public Scenario? ActiveScenario { get; set; }
  public ScenarioOptions? ActiveScenarioOptions { get; set; }

  public void SetScenarios(Scenario[] scenarios);
  public Scenario? GetScenario(StringName name);
  public IEnumerable<string> GetScenarioIds();
}

/// <summary>
///   GameTyping repository — stores pure typing game logic
/// </summary>
public class GameTypingRepo : IGameTypingRepo {
  public Scenario? ActiveScenario { get; set; }
  public ScenarioOptions? ActiveScenarioOptions { get; set; }

  private bool _disposedValue;

  private ImmutableDictionary<string, Scenario> _scenarios = ImmutableDictionary<string, Scenario>.Empty;
  public void SetScenarios(Scenario[] scenarios) {
    var dict = new Dictionary<string, Scenario>();
    foreach (var scenario in scenarios) {
      dict.Add(scenario.Id, scenario);
    }

    var dirs = DirAccess.GetDirectoriesAt(ScenarioManager.UserScenarioPath);
    foreach (var deckName in dirs) {
      var scenario = new Scenario {
        Id = deckName,
        Title = deckName,
        Source = "",
        WordList = ScenarioManager.WordListPath(deckName),
        Locale = ""
      };
      dict.Add(deckName, scenario);
    }

    _scenarios = dict.ToImmutableDictionary();
  }

  public Scenario? GetScenario(StringName name) => _scenarios.GetValueOrDefault(name);

  public IEnumerable<string> GetScenarioIds() => _scenarios.Keys;

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
