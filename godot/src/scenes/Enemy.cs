using Godot;

namespace kyoukaitansa.scenes;

[SceneTree]
public partial class Enemy : CsgBox3D
{

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
  }

  public void EnableA() {
    Show();
    SetProcess(true);
  }
  public void DisableA() {
    Hide();
    SetProcess(false);
  }

  public void SetText(string text) {
    _.Label.Text = text;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    Position = Position with { Z = Position.Z + (float)delta };
    if (Position.Z > 0) {
      GD.Print(_.Label.Text  + " freed");
      QueueFree();
    }
  }
}
