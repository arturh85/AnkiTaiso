using ankitaiso.utils;
using Godot;

namespace ankitaiso.testscenes;

public partial class TestFpsPlayerCamera : CharacterBody3D {
  [Export] public float Gravity = 10;
  [Export] public float Speed = 5;
  [Export] public float JumpSpeed = 5;
  [Export] public float MouseSensitivity = 0.002f;

  public override void _PhysicsProcess(double delta) {
    var input = Input.GetVector(
      GameInputs.MoveLeft, GameInputs.MoveRight, GameInputs.MoveForward,
      GameInputs.MoveBack
    );
    var movementDir = Transform.Basis * new Vector3(input.X, 0, input.Y);
    Velocity = Velocity with {
      X = movementDir.X * Speed, Z = movementDir.Z * Speed,
      Y = (float)(Velocity.Y + (-Gravity * delta))
    };

    MoveAndSlide();
    if (IsOnFloor() && Input.IsActionJustPressed(GameInputs.Jump)) {
      Velocity = Velocity with { Y = JumpSpeed };
    }
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseMotion motion) {
      RotateY(-motion.Relative.X * MouseSensitivity);
      var camera = GetNode<Camera3D>("Camera3D");
      camera.RotateX(-motion.Relative.Y * MouseSensitivity);
      camera.Rotation = camera.Rotation with {
        X = Mathf.Clamp(camera.Rotation.X, -Mathf.DegToRad(70), Mathf.DegToRad(70))
      };
    }
  }
}
