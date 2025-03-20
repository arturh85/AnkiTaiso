namespace ankitaiso.map;

using System;
using System.Drawing;
using Godot;
using static ankitaiso.game.state.GameLogic.Input;

[SceneTree]
[Tool]
public partial class ZombieSpawner : Node3D {

  [Export] public Vector3 GroundPosition;
  [Export] public double MinSpawnTime = 0.0;
  [Export] public double MaxSpawnTime = 1.0;
  [Export] public int MinWordLength = 0;
  [Export] public int MaxWordLength = 99;
  public bool Spawned = false;
  private bool _initialized = false;

  public void OnReady() => SetGroundPosition();

  public override void _EnterTree() {


    if (!Engine.IsEditorHint()) {
      EditorPosition.Hide();
    }
  }


  public override void _Process(double delta) {
    if (!_initialized) {
      _initialized = true;
      SetGroundPosition();
    }

    if (!Engine.IsEditorHint()) {
      return;
    }
    var material = EditorPosition.GetSurfaceOverrideMaterial(0);
    Godot.Color color = Godot.Colors.White;

    color.R = Math.Min((float)((Math.Max(MinWordLength, 2) - 2)) / 14.0f, 1.0f);

    color.B = 1.0f - color.R;
    color.G = 0.0f;
    material.Set("albedo_color", color);
    material.Set("emission", color);
    EditorPosition.SetSurfaceOverrideMaterial(0, material);

    SetGroundPosition();

  }

  private void SetGroundPosition() {
    var spaceState = GetWorld3D().DirectSpaceState;

    var query = PhysicsRayQueryParameters3D.Create(GlobalPosition, GlobalPosition + new Vector3(0, -999, 0), 0b00000001);
    var result = spaceState.IntersectRay(query);

    if (result.Count > 0) {
      //GD.Print(result);
      EditorPosition.GlobalPosition = new Vector3(GlobalPosition.X, result["position"].AsVector3().Y, GlobalPosition.Z);
      GroundPosition = EditorPosition.GlobalPosition;
    }
    //GD.Print(EditorPosition.GlobalPosition);
  }
}
