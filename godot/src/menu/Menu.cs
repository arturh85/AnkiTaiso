namespace ankitaiso.menu;

using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using data;
using domain;
using game_typing.domain;
using Godot;
using utils;

public interface IMenu : IControl {
  event Menu.NewGameEventHandler NewGame;
  event Menu.LoadGameEventHandler LoadGame;
  event Menu.OptionsEventHandler Options;
  event Menu.QuitGameEventHandler QuitGame;
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class Menu : Control, IMenu {
  public override void _Notification(int what) => this.Notify(what);

  public override void _Ready() {
    ScenarioParentContainer.Hide();
    ExampleScenario.Hide();
  }

  public void OnResolved() {
    var file = FileAccess.Open("res://src/data/scenarios.json", FileAccess.ModeFlags.Read);
    var scenarios = JsonSerializer.Deserialize<Scenario[]>(file.GetAsText(), JsonSerializerOptions.Web);
    if (scenarios == null) {
      throw new GameException("failed to load scenarios.json");
    }

    GameTypingRepo.SetScenarios(scenarios);

    var ids = GameTypingRepo.GetScenarioIds();
    foreach (var id in ids) {
      var scenario = GameTypingRepo.GetScenario(id)!;
      var control = (ExampleScenario.Duplicate() as Control)!;
      var button = (
        control.GetNode(nameof(_.MarginContainer.HBoxContainer.ScenarioParentContainer.ExampleScenario.Button)) as
          Button)!;
      button.Text = scenario.Id;
      button.Pressed += () => OnScenarioSelected(id);
      if (MenuRepo.GetActiveScenarioId() == null) {
        OnScenarioSelected(id);
        button.KeepPressedOutside = true;
      }

      var label = (
          control.GetNode(nameof(_.MarginContainer.HBoxContainer.ScenarioParentContainer.ExampleScenario.Label)) as
            Label)
        !;
      label.Text = scenario.Title;
      control.Show();
      ScenarioContainer.AddChild(control);
    }
  }

  [Dependency] public IAppRepo AppRepo => this.DependOn<IAppRepo>();
  [Dependency] public IGameTypingRepo GameTypingRepo => this.DependOn<IGameTypingRepo>();
  [Dependency] public IMenuRepo MenuRepo => this.DependOn<IMenuRepo>();

  [Signal]
  public delegate void NewGameEventHandler();

  [Signal]
  public delegate void LoadGameEventHandler();

  [Signal]
  public delegate void QuitGameEventHandler();

  [Signal]
  public delegate void OptionsEventHandler();


  public override void _Input(InputEvent @event) {
    if (!Visible) {
      return;
    }

    if (Input.IsActionJustPressed(BuiltinInputActions.UIAccept)) {
      OnNewGamePressed();
    }
    else if (Input.IsActionJustPressed(BuiltinInputActions.UICancel)) {
      OnQuitPressed();
    }
  }

  public void OnReady() {
    NewGameButton.Pressed += OnNewGamePressed;
    LoadGameButton.Pressed += OnLoadGamePressed;
    OptionsButton.Pressed += OnOptionsPressed;
    QuitButton.Pressed += OnQuitPressed;
    StartGameButton.Pressed += OnStartGamePressed;
    WordsPlayedHSlider.ValueChanged +=
      OnWordsPlayedChanged;
  }

  private void OnWordsPlayedChanged(double value) {
    int cnt = (int)value;
    WordsPlayedLabel.Text = $"{cnt} Words Played";
  }

  public void OnExitTree() {
    NewGameButton.Pressed -= OnNewGamePressed;
    LoadGameButton.Pressed -= OnLoadGamePressed;
    OptionsButton.Pressed -= OnOptionsPressed;
    QuitButton.Pressed -= OnQuitPressed;
    StartGameButton.Pressed -= OnStartGamePressed;
    WordsPlayedHSlider.ValueChanged -=
      OnWordsPlayedChanged;
  }

  public void OnNewGamePressed() => ScenarioParentContainer.Show();

  public void OnScenarioSelected(string id) {
    MenuRepo.SetActiveScenarioId(id);
    var scenario = GameTypingRepo.GetScenario(id);
    var content = scenario.ReadWordList();
    var scenarioWord = LinesRegex().Matches(content).Count;
    WordsPlayedHSlider.MinValue =
      Math.Min(10, scenarioWord);
    WordsPlayedHSlider.MaxValue = scenarioWord;

    // todo: highlight active scenario?
    foreach (var child in ScenarioContainer.GetChildren()) {
    }
  }

  public void OnStartGamePressed() {
    var wordsPlayed = (int)WordsPlayedHSlider.Value;

    GameTypingRepo.SetTotalWords(wordsPlayed);
    GameTypingRepo.SetClearedWords(0);
    GameTypingRepo.SetNumErrors(0);
    var id = MenuRepo.GetActiveScenarioId();
    GameTypingRepo.ActiveScenario = GameTypingRepo.GetScenario(id!);
    GameTypingRepo.ActiveScenarioOptions = new ScenarioOptions() { WordsPlayed = wordsPlayed };
    EmitSignal(SignalName.NewGame);
  }

  public void OnOptionsPressed() => EmitSignal(SignalName.Options);
  public void OnLoadGamePressed() => EmitSignal(SignalName.LoadGame);
  public void OnQuitPressed() => EmitSignal(SignalName.QuitGame);

  [GeneratedRegex("\n")]
  private static partial Regex LinesRegex();
}
