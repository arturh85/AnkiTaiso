using ankitaiso.game_typing;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

namespace ankitaiso.utils;

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class TestScene : Node2D {
  public override void _Notification(int what) => this.Notify(what);

  private string? s;

  [Dependency]
  public GameTypingSystem GameTypingSystem => DependentExtensions.DependOn<GameTypingSystem>(this);

  [OnInstantiate]
  private void Initialise() {
    GD.Print("TestScene Initialise (private)");
    s = "a";
  }

  public void Initialize() => GD.Print("TestScene Initialize (public)");

  public override void _EnterTree() {
    GD.Print("TestScene _EnterTree");
    base._EnterTree();
  }

  public override void _ExitTree() {
    GD.Print("TestScene _ExitTree");
    base._ExitTree();
  }

  public override void _Ready() {
    GD.Print("TestScene _Ready");
    base._Ready();
  }

  protected override void Dispose(bool disposing) {
    GD.Print("TestScene Dispose");
    base.Dispose(disposing);
  }

  public void OnPostinitialize() => GD.Print("TestScene OnPostinitialize");

  public void OnPredelete() => GD.Print("TestScene OnPredelete");

  public void OnEnterTree() => GD.Print("TestScene OnEnterTree");

  public void OnExitTree() => GD.Print("TestScene OnExitTree");

  public void OnReady() => GD.Print("TestScene OnReady");

  public void OnParented() => GD.Print("TestScene OnParented");

  public void OnUnparented() => GD.Print("TestScene OnUnparented");

  public void OnSceneInstantiated() => GD.Print("TestScene OnSceneInstantiated");

  public void OnProvided() => GD.Print("TestScene OnProvided");

  public void Setup() => GD.Print("TestScene Setup");

  public void OnResolved() => GD.Print($"TestScene OnResolved {GameTypingSystem.TotalCount}");

  public void OnBeforeReady() => GD.Print("TestScene OnBeforeReady");

  public void OnAfterReady() => GD.Print("TestScene OnAfterReady");
}
