namespace ankitaiso.player_camera;

using Chickensoft.Introspection;
using Chickensoft.Serialization;
using Godot;
using state;

[Meta, Id("player_camera_data")]
public partial record PlayerCameraData {
  [Save("state_machine")]
  public required IPlayerCameraLogic StateMachine { get; init; }

  [Save("global_transform")]
  public required Transform3D GlobalTransform { get; init; }

  [Save("local_position")]
  public required Vector3 LocalPosition { get; init; }

  [Save("offset_position")]
  public required Vector3 OffsetPosition { get; init; }
}
