using Godot;

namespace kyoukaitansa.SpaceShooter;

using utils;

public partial class Player : Area2D {
  public override void _Ready() {
  }

  /*
   * Player Speed
   */
  [Export] public int Speed { get; set; } = 150;

  public override void _Process(double delta) {
    var input = Input.GetVector(GameInputs.MoveLeft, GameInputs.MoveRight, GameInputs.MoveForward, GameInputs.MoveBack);
    Position += input * (float)(Speed * delta);
  }
}
