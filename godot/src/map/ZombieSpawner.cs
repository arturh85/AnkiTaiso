using Godot;
using System;
using static ankitaiso.player.state.PlayerLogic.State;

[SceneTree]
[Tool]
public partial class ZombieSpawner : Node3D {

  [Export] public Vector3 GroundPosition;

  public void OnReady() {
    SetGroundPosition();
  }

  public override void _EnterTree() {


    if (!Engine.IsEditorHint()) {
      EditorPosition.Hide();
    }
    SetGroundPosition();
  }


  public override void _Process(double delta) {

    if (!Engine.IsEditorHint())
      return;

    SetGroundPosition();

  }

  void SetGroundPosition() {
    var spaceState = GetWorld3D().DirectSpaceState;

    var query = PhysicsRayQueryParameters3D.Create(GlobalPosition, GlobalPosition + new Vector3(0, -999, 0));
    var result = spaceState.IntersectRay(query);

    if (result.Count > 0) {
      //GD.Print(result);
      EditorPosition.GlobalPosition = new Vector3(GlobalPosition.X, (result["position"].AsVector3()).Y, GlobalPosition.Z);
      GroundPosition = EditorPosition.GlobalPosition;
    }
    //GD.Print(EditorPosition.GlobalPosition);
  }
}
