namespace ankitaiso.menu.menu_anki;

using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using domain;
using game_typing.domain;
using Godot;

public interface IMenuAnki : IControl {
  event MenuAnki.BackEventHandler Back;
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class MenuAnki : Control, IMenuAnki {
  public override void _Notification(int what) => this.Notify(what);

  [Signal]
  public delegate void BackEventHandler();
  public override void _Ready() {
    // ScenarioParentContainer.Hide();
    ExampleAnkiDeck.Hide();
  }
  public void OnBackPressed() => EmitSignal(SignalName.Back);

  public void OnResolved() {

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
    // NewGameButton.Pressed += OnNewGamePressed;
    // LoadGameButton.Pressed += OnLoadGamePressed;
    // OptionsButton.Pressed += OnOptionsPressed;
    // QuitButton.Pressed += OnQuitPressed;
    // StartGameButton.Pressed += OnStartGamePressed;
    // WordsPlayedHSlider.ValueChanged +=
    //   OnWordsPlayedChanged;
  }
  public void OnExitTree() {
    BackButton.Pressed -= OnBackPressed;
    // NewGameButton.Pressed -= OnNewGamePressed;
    // LoadGameButton.Pressed -= OnLoadGamePressed;
    // OptionsButton.Pressed -= OnOptionsPressed;
    // QuitButton.Pressed -= OnQuitPressed;
    // StartGameButton.Pressed -= OnStartGamePressed;
    // WordsPlayedHSlider.ValueChanged -=
    //   OnWordsPlayedChanged;
  }

}
