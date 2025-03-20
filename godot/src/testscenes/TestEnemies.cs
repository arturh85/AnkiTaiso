using Godot;

namespace ankitaiso.testscenes;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;

[SceneTree]
[Meta(typeof(IAutoNode))]
public partial class TestEnemies : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  public void OnReady() {
    // OptionButton.AddItem();
  }

  public void OnExitTree() {

  }

}
