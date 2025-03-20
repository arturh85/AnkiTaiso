using Godot;
using System;

[SceneTree]
[Tool]
public partial class Level : Path3D {

  public override void _Process(double delta) {

    if (!Engine.IsEditorHint()) {
      return;
    }

    SetGroundPosition();
  }

  private void SetGroundPosition() {
    var spaceState = GetWorld3D().DirectSpaceState;

    for (int ix = 0; ix < Curve.PointCount; ix++) {
      var point = Curve.GetPointPosition(ix);

      var query = PhysicsRayQueryParameters3D.Create(point + new Vector3(0, 999, 0) + GlobalPosition, point + new Vector3(0, -999, 0) + GlobalPosition, 0b00000001);
      var result = spaceState.IntersectRay(query);

      if (result.Count > 0) {
        var newPoint = new Vector3(point.X, result["position"].AsVector3().Y + 1.7f, point.Z);
        Curve.SetPointPosition(ix, newPoint);
        Curve.BakeInterval = 10.0f;
      }
    }
  }
}
