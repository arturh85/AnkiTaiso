namespace kyoukaitansa.menu;

using System;
using System.Text.RegularExpressions;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using data;
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

  private Scenario? _selectedScenario;


  public override void _Ready() {
    ScenarioParentContainer.Hide();
    ExampleScenario.Hide();
  }

  public void OnResolved() {
    var ids = AppRepo.GetScenarioIds();
    foreach (var id in ids) {
      var scenario = AppRepo.GetScenario(id);
      if (_selectedScenario == null) {
        OnScenarioSelected(id);
      }

      var control = ExampleScenario.Duplicate() as Control;
      var button =
        control.GetNode(nameof(_.MarginContainer.HBoxContainer.ScenarioParentContainer.ExampleScenario.Button)) as
          Button;
      button.Text = scenario.Id;
      button.Pressed += () => OnScenarioSelected(id);
      var label =
        control.GetNode(nameof(_.MarginContainer.HBoxContainer.ScenarioParentContainer.ExampleScenario.Label)) as Label;
      label.Text = scenario.Title;
      control.Show();
      ScenarioContainer.AddChild(control);
    }
  }

  [Dependency] public IAppRepo AppRepo => this.DependOn<IAppRepo>();


  [Signal]
  public delegate void NewGameEventHandler(string scenarioId, int usedWords);

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
    _.MarginContainer.HBoxContainer.ScenarioParentContainer.OptionWordsPlayed.HSlider.ValueChanged += OnWordsPlayedChanged;
  }

  private void OnWordsPlayedChanged(double value) {
    int cnt = (int)value;
    _.MarginContainer.HBoxContainer.ScenarioParentContainer.OptionWordsPlayed.Label.Text = $"{cnt} Words Played";
  }

  public void OnExitTree() {
    NewGameButton.Pressed -= OnNewGamePressed;
    LoadGameButton.Pressed -= OnLoadGamePressed;
    OptionsButton.Pressed -= OnOptionsPressed;
    QuitButton.Pressed -= OnQuitPressed;
    StartGameButton.Pressed -= OnStartGamePressed;
    _.MarginContainer.HBoxContainer.ScenarioParentContainer.OptionWordsPlayed.HSlider.ValueChanged -= OnWordsPlayedChanged;
  }

  public void OnNewGamePressed() => ScenarioParentContainer.Show();

  public void OnScenarioSelected(string id) {
    var scenario = AppRepo.GetScenario(id);
    _selectedScenario = scenario;
    var content = scenario.ReadWordList();
    var scenarioWord = Regex.Matches(content, "\n").Count;
    _.MarginContainer.HBoxContainer.ScenarioParentContainer.OptionWordsPlayed.HSlider.MinValue = Math.Min(10, scenarioWord);
    _.MarginContainer.HBoxContainer.ScenarioParentContainer.OptionWordsPlayed.HSlider.MaxValue = scenarioWord;
  }

  public void OnStartGamePressed() {
    EmitSignal(SignalName.NewGame, _selectedScenario.Id,
      (int)_.MarginContainer.HBoxContainer.ScenarioParentContainer.OptionWordsPlayed.HSlider.Value);
  }

  public void OnOptionsPressed() => EmitSignal(SignalName.Options);
  public void OnLoadGamePressed() => EmitSignal(SignalName.LoadGame);
  public void OnQuitPressed() => EmitSignal(SignalName.QuitGame);
}
