namespace kyoukaitansa.enemy;

using Godot;

[SceneTree]
public partial class Enemy : Node3D {
  public Vector3 MovementTarget;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    var player = _.scene.GetNode("AnimationPlayer") as AnimationPlayer;
    var anim = player.GetAnimation("WALK");
    anim.LoopMode = Animation.LoopModeEnum.Linear;
    player.Play("WALK");
    _.GuiContainer.VBoxContainer.Input.Set("bbcode", "a");
  }

  public void EnableA() {
    Show();
    ProcessMode = ProcessModeEnum.Inherit;
    // SetProcess(true);
  }
  public void DisableA() {
    Hide();
    ProcessMode = ProcessModeEnum.Disabled;
    // SetProcess(false);
  }

  public string GetText() {
    return _.GuiContainer.VBoxContainer.Prompt.Get("bbcode").AsString();
  }
  public void SetText(string text) {
    _.GuiContainer.VBoxContainer.Prompt.Set("bbcode", text);
  }
  public void SetTarget(Vector3 target) {
    MovementTarget = target;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    var pos_3d = GlobalPosition + new Vector3(0, 0, 0);
    var cam = GetViewport().GetCamera3D();
    var pos_2d = cam.UnprojectPosition(pos_3d);
    var container = _.GuiContainer.Get();
    container.GlobalPosition = pos_2d;
    container.Visible = !cam.IsPositionBehind(pos_3d);
    //
    // if (MovementTarget == null) {
    //   GD.Print("NO movement target");
    //   return;
    // }
    Position = Position.MoveToward(MovementTarget, (float)delta * 1.1f);
    var distance = Position.DistanceTo(MovementTarget);
    LookAt(MovementTarget, Vector3.Up);
    if (distance < 2.1) {
      // GD.Print(_.Label.Text  + " freed");
      QueueFree();
    }
  }
}
