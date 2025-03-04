namespace kyoukaitansa.menu;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
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


  #region Signals

  [Signal]
  public delegate void NewGameEventHandler();

  [Signal]
  public delegate void LoadGameEventHandler();

  [Signal]
  public delegate void QuitGameEventHandler();

  [Signal]
  public delegate void OptionsEventHandler();

  #endregion Signals


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
  }

  public void OnExitTree() {
    NewGameButton.Pressed -= OnNewGamePressed;
    LoadGameButton.Pressed -= OnLoadGamePressed;
    OptionsButton.Pressed -= OnOptionsPressed;
    QuitButton.Pressed -= OnQuitPressed;
  }

  public void OnNewGamePressed() => EmitSignal(SignalName.NewGame);
  public void OnOptionsPressed() => EmitSignal(SignalName.Options);
  public void OnLoadGamePressed() => EmitSignal(SignalName.LoadGame);
  public void OnQuitPressed() => EmitSignal(SignalName.QuitGame);
}
