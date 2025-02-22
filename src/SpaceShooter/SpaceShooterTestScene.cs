namespace kyoukaitansa.SpaceShooter;

using Chickensoft.Log;
using Chickensoft.Log.Godot;
using Godot;

[SceneTree]
public partial class SpaceShooterTestScene : Node2D {
  // FIXME: should be automatic with GodotSharp.SourceGenerators Version>"2.5.0"
  public const string TscnFilePath = "res://src/SpaceShooter/" + nameof(SpaceShooterTestScene) + ".tscn";
  private readonly Log _log = new Log(nameof(SpaceShooterTestScene), new GDWriter());

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() => _log.Print("Scene Ready");

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
  }
}
