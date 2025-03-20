using Godot;

namespace ankitaiso.fps_counter;

[SceneTree]
public partial class FpsCounter : Control
{
  public override void _Process(double delta) => FpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
}
