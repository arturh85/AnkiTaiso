namespace ankitaiso.menu.menu_anki;

using System;
using System.Threading.Tasks;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using domain;
using game_typing.domain;
using Godot;
using utils;

public interface IMenuAnki : IControl {
  event MenuAnki.BackEventHandler Back;
  public Task UpdateDialog();
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class MenuAnki : Control, IMenuAnki {
  public override void _Notification(int what) => this.Notify(what);

  [Signal]
  public delegate void BackEventHandler();

  // [Node] public IHBoxContainer ExampleAnkiDeck { get; set; } = default!;
  // [Node] public ILineEdit AnkiUrlEdit { get; set; } = default!;
  // [Node] public IVBoxContainer AnkiDecksContainer { get; set; } = default!;
  // [Node] public IButton BackButton { get; set; } = default!;

  public override void _Ready() {
    // ScenarioParentContainer.Hide();
    ExampleAnkiDeck.Hide();
  }
  public void OnBackPressed() => EmitSignal(SignalName.Back);

  public void OnResolved() {

  }

  public async Task UpdateDialog() {
    var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
    var ankiService = AnkiConnectApi.GetInstance();

    var deckNames = await ankiService.ListDeckNames(ankiUrl);
    // clear existing childs
    foreach (var child in AnkiDecksContainer.GetChildren()) {
      child.QueueFree();
    }
    foreach (var deckName in deckNames) {
      var control = (ExampleAnkiDeck.Duplicate() as Control)!;
      var button = (
        control.GetNode("Button") as
          Button)!;
      button.Text = deckName;
      // button.Pressed += () => OnScenarioSelected(id);
      var label = (control.GetNode("Label") as Label)!;
      label.Text = deckName;
      control.Show();
      AnkiDecksContainer.AddChild(control);
    }
  }

  [Dependency] public IAppRepo AppRepo => DependentExtensions.DependOn<IAppRepo>(this);
  [Dependency] public IGameTypingRepo GameTypingRepo => DependentExtensions.DependOn<IGameTypingRepo>(this);
  [Dependency] public IMenuRepo MenuRepo => DependentExtensions.DependOn<IMenuRepo>(this);

  public override void _Input(InputEvent @event) {
    if (!Visible) {
      return;
    }
    //
    // if (Input.IsActionJustPressed(BuiltinInputActions.UIAccept)) {
    //   OnNewGamePressed();
    // }
    // else if (Input.IsActionJustPressed(BuiltinInputActions.UICancel)) {
    //   OnQuitPressed();
    // }
  }

  public void OnReady() {
    BackButton.Pressed += OnBackPressed;
  }
  public void OnExitTree() {
    BackButton.Pressed -= OnBackPressed;
  }

}
