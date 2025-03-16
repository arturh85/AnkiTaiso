using System.Linq;
using Godot;

namespace ankitaiso.map;
[SceneTree]
[Tool]
public partial class CameraWaypoint : Node3D {

  [Export] public double Speed = 1.0;
  [Export] public Vector3 CameraPosition;

  public override void _Process(double delta) {
    if (!Engine.IsEditorHint()) {
      return;
    }

    var spaceState = GetWorld3D().DirectSpaceState;

    var query = PhysicsRayQueryParameters3D.Create(GlobalPosition, GlobalPosition + new Vector3(0, -999, 0));
    var result = spaceState.IntersectRay(query);

    if (result.Count > 0) {
      //GD.Print(result);
      ZProjection.GlobalPosition = new Vector3(GlobalPosition.X, result["position"].AsVector3().Y  + 0.05f, GlobalPosition.Z);
      Head.Position = new Vector3(0, ZProjection.Position.Y + 1.7f, 0);
      CameraPosition = Head.GlobalPosition;
    }

    CameraWaypoint? next = null;
    var lastWasThis = false;
    foreach (var waypoint in GetParent().GetChildren().OfType<CameraWaypoint>()) {
      if (waypoint == this) {
        lastWasThis = true;
      }
      else if (lastWasThis) {
        next = waypoint;
        break;
      }

    }
    if (next != null) {
      if (PathCenter.Position != next.Head.GlobalPosition) {
        PathCenter.LookAt(next.Head.GlobalPosition, new Vector3(0, 1, 0), true);
      }
      //PathCenter.RotateY((float)Math.PI / 2.0f);
      var diff = next.Head.GlobalPosition - Head.GlobalPosition;
      PathCenter.Scale = new Vector3(1, 1, diff.Length() * 5.0f);
      PathCenter.Show();
      //GD.Print(next.Head.GlobalPosition - Head.GlobalPosition);
    }
    else {
      PathCenter.Scale = new Vector3(0.1f, 0.1f, 0.1f);
      PathCenter.Hide();
    }



  }
  public override void _EnterTree() {


    if (!Engine.IsEditorHint()) {
      ZProjection.Hide();
      Head.Hide();
    }
  }

}
